using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// Rato de vitrine da sala de customizacao. Interface diegetica:
// - plaquinha 3D com o nome da cor no pedestal;
// - anel + luz + "SELECIONADO" no rato que esta vestindo a skin atual;
// - prompt "[E] Vestir esta skin" em billboard quando o jogador mira no rato.
public class RatSkinStation : MonoBehaviour
{
    [Header("Skin")]
    [SerializeField] private int skinIndex;
    [SerializeField] private string skinLabel = "RATO";
    [Tooltip("Material do rato de vitrine. E o mesmo que vai para o corpo do jogador.")]
    [SerializeField] private Material skin;
    [Tooltip("Modelo do rato de vitrine (Assets/Models/Rat.fbx).")]
    [SerializeField] private GameObject ratPrefab;
    [Tooltip("Controller usado so para dar ao modelo a pose de rato em pe.")]
    [SerializeField] private RuntimeAnimatorController statueAnimator;
    [Tooltip("Renderer do rato de vitrine. Vazio = procura nos filhos.")]
    [SerializeField] private Renderer ratRenderer;

    [Header("Interacao")]
    [SerializeField] private Key interactKey = Key.E;
    [SerializeField] private float interactRange = 3.5f;
    [SerializeField] private string promptLabel = "Vestir esta skin";

    [Header("Layout")]
    [Tooltip("Altura do texto da plaquinha, no mundo.")]
    [SerializeField] private float plateHeight = 0.2f;
    [Tooltip("Altura do topo do pedestal em relacao a base da estacao.")]
    [SerializeField] private float pedestalTop = 0.28f;
    [Tooltip("Ajuste fino para o rato ficar apoiado em cima do pedestal, e nao afundado nele.")]
    [SerializeField] private float ratLift = 0.09f;

    private static readonly Color SelectionColor = new Color(1f, 0.86f, 0.35f);
    private static readonly Color PromptColor = new Color(0.95f, 0.95f, 0.9f);

    private Transform ratVisual;
    private Transform namePlate;
    private Transform prompt;
    private Transform selectedText;
    private LineRenderer ring;
    private Light glow;
    private Transform view;
    private bool focused;

    private void Start()
    {
        SpawnRat();
        LayoutRat();
        BuildNamePlate();
        BuildSelection();
        BuildPrompt();

        // Primeira entrada na sala: marca o rato da skin que o jogador ja esta usando.
        if (RatSkinLibrary.SelectedIndex < 0 && skin != null && RatSkinLibrary.PlayerBodyMaterial() == skin)
            RatSkinLibrary.Select(skinIndex, skin);

        RefreshSelection();
    }

    private void OnEnable() => RatSkinLibrary.Changed += OnSkinChanged;

    private void OnDisable() => RatSkinLibrary.Changed -= OnSkinChanged;

    private void OnSkinChanged(int _) => RefreshSelection();

    private void Update()
    {
        var cam = Camera.main;
        if (cam == null) return;

        view = cam.transform;

        bool looking = IsLookedAt(cam);
        SetFocused(looking);

        if (looking && Keyboard.current != null && Keyboard.current[interactKey].wasPressedThisFrame)
            RatSkinLibrary.Select(skinIndex, skin);
    }

    private void LateUpdate()
    {
        if (view == null) return;

        // Textos flutuantes acompanham a camera; a plaquinha fica parada, virada para a entrada.
        FaceCamera(prompt);
        FaceCamera(selectedText);
    }

    // O rato de vitrine e o mesmo modelo do jogador, parado em cima do pedestal.
    private void SpawnRat()
    {
        if (ratRenderer == null && ratPrefab != null)
        {
            var instance = Instantiate(ratPrefab, transform);
            instance.name = "RatoVitrine";
            instance.transform.localPosition = new Vector3(0f, pedestalTop, 0f);
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            // Estatua: entra na pose de idle e congela (o bind pose do modelo fica deitado).
            var animator = instance.GetComponentInChildren<Animator>(true);
            if (animator != null)
            {
                if (statueAnimator != null) animator.runtimeAnimatorController = statueAnimator;
                if (animator.runtimeAnimatorController != null)
                {
                    animator.speed = 1f;
                    animator.Update(0.7f);
                    animator.speed = 0f;
                }
                else
                {
                    animator.enabled = false;
                }
            }

            ratRenderer = instance.GetComponentInChildren<Renderer>(true);
        }

        if (ratRenderer == null) ratRenderer = GetComponentInChildren<Renderer>();
        if (ratRenderer != null && skin != null) ratRenderer.sharedMaterial = skin;
    }

    // Encosta o rato no topo do pedestal: o pivo do modelo nao e padronizado.
    private void LayoutRat()
    {
        if (ratRenderer == null) return;

        ratVisual = ratRenderer.transform;
        while (ratVisual.parent != null && ratVisual.parent != transform)
            ratVisual = ratVisual.parent;

        // O pivo do modelo ja fica na base do rato: basta apoiar no topo do pedestal.
        if (ratVisual != transform)
            ratVisual.localPosition = new Vector3(0f, pedestalTop + ratLift, 0f);
    }

    // Altura fixa acima do pedestal: nao depende dos bounds do mesh (que mudam com a renderizacao).
    private Vector3 HeadPoint() => transform.position + Vector3.up * (pedestalTop + 0.35f);

    private Color SkinColor()
    {
        if (skin != null)
        {
            if (skin.HasProperty("_BaseColor")) return skin.GetColor("_BaseColor");
            if (skin.HasProperty("_Color")) return skin.GetColor("_Color");
        }
        return Color.white;
    }

    // Texto 3D de uma linha so: sem quebra automatica, senao o rotulo vira uma coluna estreita.
    private static TextMeshPro CreateLabel(string name, Transform parent, string content, Color color, float worldHeight)
    {
        var text = DialogueFont.CreateWorldText(name, parent, content, color, 44f, worldHeight);
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
        text.rectTransform.sizeDelta = new Vector2(24f, 2f);
        text.ForceMeshUpdate();

        float natural = text.bounds.size.y;
        if (natural > 0.0001f)
            text.transform.localScale = Vector3.one * (worldHeight / natural);

        return text;
    }

    private void BuildNamePlate()
    {
        var text = CreateLabel("Placa", transform, skinLabel.ToUpperInvariant(), SelectionColor, plateHeight);
        namePlate = text.transform;
        namePlate.position = transform.position + transform.forward * 0.31f + Vector3.up * 0.14f;
        namePlate.rotation = transform.rotation * Quaternion.Euler(0f, 180f, 0f);
    }

    private void BuildSelection()
    {
        var ringObject = new GameObject("AnelSelecao");
        ringObject.transform.SetParent(transform, false);
        ringObject.transform.localPosition = new Vector3(0f, 0.012f, 0f);

        ring = ringObject.AddComponent<LineRenderer>();
        ring.useWorldSpace = false;
        ring.loop = true;
        ring.widthMultiplier = 0.04f;
        ring.numCapVertices = 2;
        ring.positionCount = 48;
        ring.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        ring.receiveShadows = false;
        for (int i = 0; i < ring.positionCount; i++)
        {
            float angle = i / (float)ring.positionCount * Mathf.PI * 2f;
            ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * 0.36f, 0f, Mathf.Sin(angle) * 0.36f));
        }

        var shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            var material = new Material(shader) { color = SelectionColor };
            ring.material = material;
        }

        var lightObject = new GameObject("LuzSelecao");
        lightObject.transform.SetParent(transform, false);
        lightObject.transform.localPosition = new Vector3(0f, pedestalTop + 0.35f, 0f);

        glow = lightObject.AddComponent<Light>();
        glow.type = LightType.Point;
        glow.color = SkinColor();
        glow.range = 1.7f;
        glow.intensity = 1.3f;
        glow.shadows = LightShadows.None;

        var text = CreateLabel("Selecionado", transform, "SELECIONADO", SelectionColor, 0.15f);
        selectedText = text.transform;
        selectedText.position = HeadPoint() + Vector3.up * 0.22f;
    }

    private void BuildPrompt()
    {
        var text = CreateLabel("Prompt", transform, $"[{interactKey}] {promptLabel}", PromptColor, 0.18f);
        prompt = text.transform;
        prompt.position = HeadPoint() + Vector3.up * 0.4f;
        prompt.gameObject.SetActive(false);
    }

    private void RefreshSelection()
    {
        bool isSelected = RatSkinLibrary.SelectedIndex == skinIndex;

        if (ring != null) ring.enabled = isSelected;
        if (glow != null) glow.enabled = isSelected;
        if (selectedText != null) selectedText.gameObject.SetActive(isSelected);
    }

    private void SetFocused(bool value)
    {
        if (focused == value) return;

        focused = value;
        if (prompt != null) prompt.gameObject.SetActive(value);
    }

    // Olhar para o rato = primeiro collider atingido no caminho pertence a esta estacao.
    // O proprio corpo do jogador fica entre a camera e o rato, entao ele e ignorado.
    private bool IsLookedAt(Camera cam)
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        var hits = Physics.RaycastAll(ray, interactRange, ~0, QueryTriggerInteraction.Collide);
        if (hits.Length == 0) return false;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        var player = PlayerRoot();
        foreach (var hit in hits)
        {
            var collider = hit.collider;
            if (collider == null) continue;
            if (player != null && collider.transform.IsChildOf(player)) continue;

            return collider.gameObject == gameObject || collider.transform.IsChildOf(transform);
        }

        return false;
    }

    private static Transform playerRoot;

    private static Transform PlayerRoot()
    {
        if (playerRoot != null) return playerRoot;

        var look = FindAnyObjectByType<PlayerLook>();
        playerRoot = look != null ? look.transform : null;
        return playerRoot;
    }

    private void FaceCamera(Transform target)
    {
        if (target == null || !target.gameObject.activeSelf) return;

        // O texto 3D do TMP le para o lado -Z: o +Z aponta para tras do leitor.
        target.rotation = Quaternion.LookRotation(target.position - view.position, Vector3.up);
    }
}
