using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

// Conduz a conversa: tira a câmera da primeira pessoa, alterna o enquadramento entre
// quem fala e quem ouve, digita o texto no balão 3D e devolve tudo ao normal no fim.
public class DialogueDirector : MonoBehaviour
{
    public static DialogueDirector Instance { get; private set; }
    public static bool PlayerBusy => Instance != null && Instance.active;
    public bool IsActive => active;

    [Header("Ritmo do texto")]
    [Tooltip("Letras por segundo na animação de máquina de escrever.")]
    [SerializeField] private float charactersPerSecond = 45f;

    [Header("Câmera cinematográfica")]
    [Tooltip("Tempo para o enquadramento trocar de um personagem para o outro.")]
    [SerializeField] private float shotBlend = 0.4f;
    [Tooltip("Distância atrás de quem escuta (câmera por cima do ombro).")]
    [SerializeField] private float shotBack = 0.8f;
    [SerializeField] private float shotSide = 0.4f;
    [SerializeField] private float shotHeight = 0.3f;

    private bool active;
    private NpcDialogue npc;
    private DialogueLine[] lines;
    private int index;

    private bool typing;
    private float revealed;
    private int totalCharacters;

    private PlayerLook look;
    private CharacterMotor motor;
    private Health health;
    private PlayerHand hand;
    private Transform player;
    private Camera mainCamera;
    private CinemachineBrain brain;
    private Transform holdPoint;

    private DialogueBubble bubble;

    private int savedCullingMask;
    private bool savedHoldPointActive;
    private bool savedLookEnabled;
    private bool savedMotorLocked;
    private bool savedHandEnabled;
    private bool savedInvulnerable;
    private bool savedBrainEnabled;
    private CursorLockMode savedCursorLock;
    private bool savedCursorVisible;

    private Vector3 cameraVelocity;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindAnyObjectByType<DialogueDirector>() != null) return;
        new GameObject("DialogueDirector").AddComponent<DialogueDirector>();
    }

    public static DialogueDirector Ensure()
    {
        if (Instance != null) return Instance;
        return new GameObject("DialogueDirector").AddComponent<DialogueDirector>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        mainCamera = Camera.main;
        CachePlayer();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void CachePlayer()
    {
        look = FindAnyObjectByType<PlayerLook>();
        if (look == null) return;

        player = look.transform;
        motor = player.GetComponent<CharacterMotor>();
        health = player.GetComponent<Health>();
        hand = player.GetComponent<PlayerHand>();

        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera != null)
        {
            brain = mainCamera.GetComponent<CinemachineBrain>();
            holdPoint = FindDeepChild(mainCamera.transform, "HoldPoint");
        }
    }

    public void Begin(NpcDialogue source)
    {
        if (active || source == null || source.Lines == null || source.Lines.Length == 0) return;

        CachePlayer();
        if (mainCamera == null || player == null) return;

        npc = source;
        lines = source.Lines;
        index = 0;
        active = true;
        cameraVelocity = Vector3.zero;

        SaveState();
        EnterDialogueState();

        npc.FacePlayer(player);
        npc.SetMarkersVisible(false);

        ShowLine();
    }

    private void SaveState()
    {
        savedCullingMask = mainCamera.cullingMask;
        savedHoldPointActive = holdPoint != null && holdPoint.gameObject.activeSelf;
        savedLookEnabled = look != null && look.enabled;
        savedMotorLocked = motor != null && motor.Locked;
        savedHandEnabled = hand != null && hand.enabled;
        savedInvulnerable = health != null && health.Invulnerable;
        savedBrainEnabled = brain != null && brain.enabled;
        savedCursorLock = Cursor.lockState;
        savedCursorVisible = Cursor.visible;
    }

    private void EnterDialogueState()
    {
        // Mostra a layer Player (normalmente escondida na primeira pessoa).
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer != -1) mainCamera.cullingMask |= 1 << playerLayer;

        // Some com o holdPoint (e com o nariz preso na câmera).
        if (holdPoint != null) holdPoint.gameObject.SetActive(false);

        // Solta a câmera do Cinemachine e do mouse.
        if (brain != null) brain.enabled = false;
        if (look != null) look.enabled = false;
        if (motor != null) motor.Locked = true;
        if (hand != null) hand.enabled = false;

        // Imune a dano enquanto conversa.
        if (health != null) health.Invulnerable = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (bubble == null)
        {
            bubble = DialogueBubble.Create("BalaoDialogo", transform, new Vector2(700f, 240f), 0.0022f);
            bubble.Canvas.worldCamera = mainCamera;
        }
    }

    private void ShowLine()
    {
        var line = lines[index];
        bool npcSpeaking = line.speaker == DialogueSpeaker.Npc;
        Transform speaker = npcSpeaking ? npc.transform : player;

        string speakerName = npcSpeaking ? npc.DisplayName : "Você";
        Color speakerColor = npcSpeaking ? new Color(1f, 0.84f, 0.25f) : new Color(0.6f, 0.85f, 1f);

        bubble.Show(speakerName, speakerColor, line.text, 44f);
        bubble.Canvas.worldCamera = mainCamera;

        totalCharacters = bubble.TotalCharacters;
        revealed = 0f;
        typing = totalCharacters > 0;

        // A câmera já parte para o novo enquadramento neste frame.
        Transform listener = npcSpeaking ? player : npc.transform;
        ComputeShot(speaker, listener);
    }

    private void Update()
    {
        if (!active) return;

        float dt = Time.deltaTime;

        if (typing)
        {
            revealed += charactersPerSecond * dt;
            int visible = Mathf.Min(totalCharacters, Mathf.FloorToInt(revealed));
            bubble.SetVisibleCharacters(visible);

            if (visible >= totalCharacters) FinishTyping();
        }

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // X: completa a fala na hora (pula a máquina de escrever).
        if (keyboard[Key.X].wasPressedThisFrame && typing) FinishTyping();

        // Z: avança quando a fala terminou de aparecer.
        if (keyboard[Key.Z].wasPressedThisFrame && !typing) Advance();
    }

    private void FinishTyping()
    {
        typing = false;
        revealed = totalCharacters;
        bubble.SetVisibleCharacters(totalCharacters);
        bubble.Hint.text = index < lines.Length - 1 ? "[Z] continuar" : "[Z] encerrar";
    }

    private void Advance()
    {
        index++;
        if (index >= lines.Length)
        {
            End();
            return;
        }

        ShowLine();
    }

    public void End()
    {
        if (!active) return;
        active = false;

        if (mainCamera != null) mainCamera.cullingMask = savedCullingMask;

        if (holdPoint != null) holdPoint.gameObject.SetActive(savedHoldPointActive);
        if (brain != null) brain.enabled = savedBrainEnabled;
        if (look != null) look.enabled = savedLookEnabled;
        if (motor != null) motor.Locked = savedMotorLocked;
        if (hand != null) hand.enabled = savedHandEnabled;
        if (health != null) health.Invulnerable = savedInvulnerable;

        Cursor.lockState = savedCursorLock;
        Cursor.visible = savedCursorVisible;

        if (bubble != null) bubble.Hide();
        if (npc != null) npc.SetMarkersVisible(true);

        npc = null;
        lines = null;
        cameraVelocity = Vector3.zero;
    }

    private void LateUpdate()
    {
        if (!active || mainCamera == null) return;

        bool npcSpeaking = lines != null && index < lines.Length && lines[index].speaker == DialogueSpeaker.Npc;
        Transform speaker = npcSpeaking ? npc.transform : player;
        Transform listener = npcSpeaking ? player : npc.transform;

        Vector3 target = ComputeShot(speaker, listener);

        mainCamera.transform.position = Vector3.SmoothDamp(
            mainCamera.transform.position, target, ref cameraVelocity, shotBlend);

        Vector3 lookAt = HeadPoint(speaker) + Vector3.up * 0.02f;
        Vector3 toTarget = lookAt - mainCamera.transform.position;
        if (toTarget.sqrMagnitude > 0.0001f)
        {
            Quaternion desired = Quaternion.LookRotation(toTarget, Vector3.up);
            mainCamera.transform.rotation = Quaternion.Slerp(
                mainCamera.transform.rotation, desired, 1f - Mathf.Exp(-10f * Time.deltaTime));
        }

        bubble.PlaceAbove(HeadPoint(speaker) + Vector3.up * 0.10f, mainCamera);
    }

    private Vector3 ComputeShot(Transform speaker, Transform listener)
    {
        Vector3 speakerHead = HeadPoint(speaker);
        Vector3 listenerHead = HeadPoint(listener);

        Vector3 axis = speakerHead - listenerHead;
        axis.y = 0f;
        if (axis.sqrMagnitude < 0.0001f) axis = speaker.forward;
        axis.Normalize();

        // Ombros alternados: a câmera fica atrás de quem escuta.
        float sideSign = speaker == npc.transform ? 1f : -1f;
        Vector3 side = Vector3.Cross(Vector3.up, axis) * (shotSide * sideSign);

        Vector3 desired = listenerHead - axis * shotBack + side + Vector3.up * shotHeight;
        Vector3 lookAt = speakerHead + Vector3.up * 0.02f;

        // Não atravessa parede/móvel: puxa a câmera para o primeiro obstáculo.
        if (Physics.Linecast(lookAt, desired, out RaycastHit hit, ~0, QueryTriggerInteraction.Ignore)
            && !IsBody(hit.collider, speaker) && !IsBody(hit.collider, listener))
        {
            Vector3 direction = (desired - lookAt).normalized;
            desired = hit.point + direction * 0.15f;
        }

        return desired;
    }

    private static bool IsBody(Collider collider, Transform owner)
    {
        if (collider == null || owner == null) return false;
        return collider.transform == owner || collider.transform.IsChildOf(owner);
    }

    private Vector3 HeadPoint(Transform target)
    {
        if (target == null) return transform.position;

        var renderer = target.GetComponentInChildren<Renderer>();
        if (renderer != null && renderer.enabled)
            return new Vector3(target.position.x, renderer.bounds.max.y, target.position.z);

        var collider = target.GetComponentInChildren<Collider>();
        if (collider != null)
            return new Vector3(target.position.x, collider.bounds.max.y, target.position.z);

        return target.position + Vector3.up * 0.5f;
    }

    private static Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (var child in parent.GetComponentsInChildren<Transform>(true))
            if (child.name == childName) return child;
        return null;
    }
}
