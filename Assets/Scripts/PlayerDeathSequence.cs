using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

// Morte do jogador no estilo GTA: a câmera sai da primeira pessoa, sobe e se afasta do corpo,
// a tela escurece, o personagem respawna e ganha proteção de spawn.
//
// O componente se adiciona sozinho no Player quando a cena carrega (procura PlayerLook + Health),
// então não é preciso montar nada no Inspector.
public class PlayerDeathSequence : MonoBehaviour
{
    [Header("Câmera da morte (múltiplos da altura do corpo)")]
    [Tooltip("Quanto a câmera sobe em relação à altura do corpo.")]
    [SerializeField] private float riseFactor = 1.6f;
    [Tooltip("Quanto a câmera se afasta para trás em relação à altura do corpo.")]
    [SerializeField] private float backFactor = 2f;
    [Tooltip("Altura do ponto do corpo que a câmera olha (0 = pés, 1 = topo).")]
    [SerializeField] private float lookAtFactor = 0.45f;
    [Tooltip("Curvatura do movimento (0 = linha reta).")]
    [SerializeField] private float arcFactor = 0.25f;
    [SerializeField] private float cameraOutDuration = 1.4f;

    [Header("Fade")]
    [Tooltip("Quanto tempo depois de a câmera começar a sair a tela começa a escurecer.")]
    [SerializeField] private float fadeDelay = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.9f;
    [SerializeField] private float blackHold = 0.4f;
    [SerializeField] private float fadeInDuration = 0.6f;

    [Header("Proteção de spawn")]
    [Tooltip("Segundos de invulnerabilidade contados a partir do momento em que o jogador começa a se mover.")]
    [SerializeField] private float protectionAfterMove = 10f;

    private Health health;
    private CharacterMotor motor;
    private PlayerLook look;
    private Rigidbody rb;
    private Animator animator;
    private Collider bodyCollider;
    private Transform mainCamera;
    private CinemachineBrain brain;
    private GameObject nose;

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private bool noseWasActive = true;
    private Coroutine sequence;
    private Coroutine protection;
    private Transform holdPoint;
    private int originalCullingMask;
    private bool holdPointWasActive;

    private void Awake()
    {
        health = GetComponent<Health>();
        motor = GetComponent<CharacterMotor>();
        look = GetComponent<PlayerLook>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        bodyCollider = GetComponent<Collider>();

        spawnPosition = transform.position;
        spawnRotation = transform.rotation;

        var cam = Camera.main;
        if (cam != null)
        {
            mainCamera = cam.transform;
            brain = cam.GetComponent<CinemachineBrain>();
            var noseTransform = FindDeepChild(mainCamera, "PlayerNose");
            if (noseTransform != null) nose = noseTransform.gameObject;
            var holdTransform = FindDeepChild(mainCamera, "HoldPoint");
            if (holdTransform != null)
                holdPoint = holdTransform;
            originalCullingMask = cam.cullingMask;
        }
    }

    private void OnEnable()
    {
        if (health != null) health.Died += OnDied;
    }

    private void OnDisable()
    {
        if (health != null) health.Died -= OnDied;
    }

    private void OnDied()
    {
        if (sequence != null) return;
        if (mainCamera == null) return;
        sequence = StartCoroutine(DeathSequence());
    }

    private void PrepareDeathCamera()
    {
        // Adiciona temporariamente a Layer Player ao Culling Mask.
        int playerLayer = LayerMask.NameToLayer("Player");

        if (playerLayer != -1)
        {
            Camera cam = mainCamera.GetComponent<Camera>();

            if (cam != null)
                cam.cullingMask |= (1 << playerLayer);
        }

        // Esconde o HoldPoint.
        if (holdPoint != null)
        {
            holdPointWasActive = holdPoint.gameObject.activeSelf;
            holdPoint.gameObject.SetActive(false);
        }
    }

    private IEnumerator DeathSequence()
    {
        PrepareDeathCamera();
        // 1) Sai da primeira pessoa: o mouse não gira mais a cabeça, o corpo para
        //    e o nariz preso na câmera some.
        if (look != null) look.enabled = false;
        if (motor != null) motor.Locked = true;
        if (nose != null)
        {
            noseWasActive = nose.activeSelf;
            nose.SetActive(false);
        }
        if (brain != null) brain.enabled = false;

        Vector3 camStartPos = mainCamera.position;
        Quaternion camStartRot = mainCamera.rotation;

        float bodyHeight = bodyCollider != null ? Mathf.Max(0.2f, bodyCollider.bounds.size.y) : 1f;
        Vector3 focus = transform.position + Vector3.up * (bodyHeight * lookAtFactor);

        Vector3 away = camStartPos - focus;
        away.y = 0f;
        away = away.sqrMagnitude > 0.0001f ? away.normalized : -transform.forward;
        Vector3 camEndPos = focus + away * (bodyHeight * backFactor) + Vector3.up * (bodyHeight * riseFactor);

        // 2) Câmera sobe e se afasta enquanto a tela escurece.
        Coroutine fade = null;
        for (float t = 0f; t < cameraOutDuration; t += Time.deltaTime)
        {
            float k = Mathf.SmoothStep(0f, 1f, t / cameraOutDuration);

            Vector3 pos = Vector3.Lerp(camStartPos, camEndPos, k);
            pos += Vector3.up * (Mathf.Sin(k * Mathf.PI) * bodyHeight * arcFactor);
            mainCamera.position = pos;

            Vector3 toFocus = focus - pos;
            if (toFocus.sqrMagnitude > 0.0001f)
                mainCamera.rotation = Quaternion.Slerp(camStartRot, Quaternion.LookRotation(toFocus, Vector3.up), k);

            if (fade == null && t >= fadeDelay)
                fade = StartCoroutine(ScreenFader.Instance.FadeTo(1f, fadeOutDuration));

            yield return null;
        }

        mainCamera.position = camEndPos;
        mainCamera.rotation = Quaternion.LookRotation(focus - camEndPos, Vector3.up);

        if (fade != null) yield return fade;
        else yield return ScreenFader.Instance.FadeTo(1f, fadeOutDuration);

        // 3) Respawn (ainda no preto).
        if (blackHold > 0f) yield return new WaitForSeconds(blackHold);
        Respawn();
        RestoreDeathCamera();
        yield return null;

        // 4) Volta a luz com a câmera de novo na primeira pessoa e liga a proteção.
        yield return ScreenFader.Instance.FadeTo(0f, fadeInDuration);

        StartProtection();
        sequence = null;
    }

    private void Respawn()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = spawnPosition;
        }
        transform.SetPositionAndRotation(spawnPosition, spawnRotation);

        if (health != null) health.ResetState();   // vida cheia
        if (motor != null) motor.Locked = false;

        // Sai do estado de morte da animação.
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        if (look != null)
        {
            look.ResetLook(spawnRotation.eulerAngles.y);
            look.enabled = true;
        }

        if (nose != null) nose.SetActive(noseWasActive);

        // Devolve a câmera pro Cinemachine, que volta a seguir a cabeça.
        if (brain != null) brain.enabled = true;
    }

    private void StartProtection()
    {
        if (protection != null) StopCoroutine(protection);
        protection = StartCoroutine(ProtectionRoutine());
    }

    private IEnumerator ProtectionRoutine()
    {
        if (health == null) yield break;
        health.Invulnerable = true;

        // Protegido enquanto estiver parado; some 10s depois que começa a se mover.
        while (motor != null && !motor.IsMoving) yield return null;
        yield return new WaitForSeconds(protectionAfterMove);

        if (!health.IsDead) health.Invulnerable = false;
        protection = null;
    }

    private static Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (var t in parent.GetComponentsInChildren<Transform>(true))
            if (t.name == childName) return t;
        return null;
    }

    // Com "Enter Play Mode Options" (sem domain reload) a cena recarrega sem reiniciar o assembly;
    // este hook garante que o componente exista no Player da cena ativa.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        var playerLook = FindAnyObjectByType<PlayerLook>();
        if (playerLook == null) return;
        if (playerLook.GetComponent<Health>() == null) return;
        if (playerLook.GetComponent<PlayerDeathSequence>() != null) return;

        playerLook.gameObject.AddComponent<PlayerDeathSequence>();
    }

    private void RestoreDeathCamera()
    {
        // Restaura o Culling Mask original.
        Camera cam = mainCamera.GetComponent<Camera>();

        if (cam != null)
            cam.cullingMask = originalCullingMask;

        // Restaura o HoldPoint.
        if (holdPoint != null)
            holdPoint.gameObject.SetActive(holdPointWasActive);
    }
}
