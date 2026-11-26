using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Marca um personagem como NPC conversável: "!" na cabeça, aura ao redor e prompt de
// interação quando o jogador chega perto. A conversa é editada no próprio Inspector.
[DisallowMultipleComponent]
public class NpcDialogue : MonoBehaviour
{
    [Header("Identidade")]
    [SerializeField] private string displayName = "NPC";

    [Header("Conversa (cada fala escolhe quem fala e o texto)")]
    [SerializeField] private DialogueLine[] lines =
    {
        new DialogueLine { speaker = DialogueSpeaker.Npc, text = "Ora, ora... um rato por aqui?" },
        new DialogueLine { speaker = DialogueSpeaker.Player, text = "Só de passagem. Você sabe o que tem nessa cozinha?" },
        new DialogueLine { speaker = DialogueSpeaker.Npc, text = "Perigo, muita sujeira e alguns ratos mal-humorados." },
        new DialogueLine { speaker = DialogueSpeaker.Npc, text = "Fique de olho no seu rabo. Ele não é seu para sempre." },
    };

    [Header("Interação")]
    [SerializeField] private Key interactKey = Key.E;
    [SerializeField] private float interactRange = 2.5f;
    [SerializeField] private string promptLabel = "Falar";

    [Header("Marcadores")]
    [SerializeField] private bool showMarkers = true;
    [SerializeField] private string markerText = "!";
    [SerializeField] private Color markerColor = new Color(1f, 0.83f, 0.15f);
    [SerializeField] private Color auraColor = new Color(1f, 0.78f, 0.28f, 0.22f);

    private Transform player;
    private Transform markerRoot;
    private TextMeshPro marker;
    private Transform aura;
    private Light auraLight;
    private GameObject promptRoot;
    private RectTransform promptRect;
    private Vector3 markerBaseLocal;
    private Vector3 auraBaseScale;

    public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
    public DialogueLine[] Lines => lines;
    public float InteractRange => interactRange;

    private void Awake()
    {
        if (showMarkers) BuildMarkers();
    }

    private void Start()
    {
        var look = FindAnyObjectByType<PlayerLook>();
        if (look != null) player = look.transform;
        else
        {
            var tagged = GameObject.FindGameObjectWithTag("Player");
            if (tagged != null) player = tagged.transform;
        }
    }

    private void OnDisable()
    {
        if (promptRoot != null) promptRoot.SetActive(false);
    }

    private void Update()
    {
        if (player == null) return;

        bool busy = DialogueDirector.PlayerBusy;
        bool inRange = !busy && FlatDistance(player.position, transform.position) <= interactRange;

        if (promptRoot != null) promptRoot.SetActive(inRange && showMarkers);

        if (markerRoot != null) markerRoot.gameObject.SetActive(!busy && showMarkers);

        if (!inRange || Keyboard.current == null) return;

        if (Keyboard.current[interactKey].wasPressedThisFrame)
            DialogueDirector.Ensure().Begin(this);
    }

    private void LateUpdate()
    {
        var camera = Camera.main;
        if (camera == null) return;

        if (markerRoot != null && markerRoot.gameObject.activeSelf)
        {
            markerRoot.localPosition = markerBaseLocal + Vector3.up * (Mathf.Sin(Time.time * 2.6f) * 0.04f);
            markerRoot.rotation = Quaternion.LookRotation(markerRoot.position - camera.transform.position, Vector3.up);
        }

        if (promptRoot != null && promptRoot.activeSelf)
            promptRect.rotation = Quaternion.LookRotation(promptRect.position - camera.transform.position, Vector3.up);

        if (aura != null && aura.gameObject.activeSelf)
        {
            float pulse = 1f + Mathf.Sin(Time.time * 2f) * 0.05f;
            aura.localScale = auraBaseScale * pulse;
            if (auraLight != null) auraLight.intensity = 0.45f * pulse;
        }
    }

    // Vira o NPC de frente para o jogador (usado ao começar a conversa).
    public void FacePlayer(Transform target)
    {
        if (target == null) return;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    public void SetMarkersVisible(bool visible)
    {
        if (markerRoot != null) markerRoot.gameObject.SetActive(visible && showMarkers);
        if (aura != null) aura.gameObject.SetActive(visible && showMarkers);
        if (auraLight != null) auraLight.enabled = visible && showMarkers;
        if (!visible && promptRoot != null) promptRoot.SetActive(false);
    }

    private void BuildMarkers()
    {
        if (markerRoot != null) return;

        Vector3 head = HeadPoint();

        // "!" flutuando acima da cabeça.
        marker = DialogueFont.CreateWorldText("Marcador", transform, markerText, markerColor, 72f, 0.34f);
        markerRoot = marker.transform;
        markerRoot.position = new Vector3(head.x, head.y + 0.52f, head.z);
        markerBaseLocal = markerRoot.localPosition;

        // Aura: anel translúcido no chão + luz quente (não atrapalha o enquadramento).
        var auraObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        auraObject.name = "Aura";
        var auraCollider = auraObject.GetComponent<Collider>();
        if (auraCollider != null) Destroy(auraCollider);

        aura = auraObject.transform;
        aura.SetParent(transform, false);

        Vector3 size = BodySize();
        Vector3 feet = FeetPoint();
        float diameter = Mathf.Max(0.5f, Mathf.Max(size.x, size.z) * 1.7f);
        auraBaseScale = new Vector3(diameter, 0.01f, diameter);
        aura.localScale = auraBaseScale;
        aura.position = new Vector3(transform.position.x, feet.y + 0.02f, transform.position.z);

        var shader = Shader.Find("Sprites/Default");
        var auraRenderer = auraObject.GetComponent<Renderer>();
        if (shader != null)
        {
            var material = new Material(shader);
            if (material.HasProperty("_Color")) material.SetColor("_Color", auraColor);
            auraRenderer.sharedMaterial = material;
        }
        else auraRenderer.enabled = false;

        auraRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        auraRenderer.receiveShadows = false;

        var lightObject = new GameObject("LuzAura");
        lightObject.transform.SetParent(transform, false);
        lightObject.transform.position = new Vector3(transform.position.x, feet.y + size.y * 0.6f, transform.position.z);
        auraLight = lightObject.AddComponent<Light>();
        auraLight.type = LightType.Point;
        auraLight.color = auraColor;
        auraLight.range = Mathf.Max(1.2f, size.magnitude * 1.5f);
        auraLight.intensity = 0.45f;
        auraLight.shadows = LightShadows.None;

        // Prompt "[E] Falar" acima da cabeça.
        var promptObject = new GameObject("PromptInteracao", typeof(RectTransform));
        promptObject.transform.SetParent(transform, false);
        var promptCanvas = promptObject.AddComponent<Canvas>();
        promptCanvas.renderMode = RenderMode.WorldSpace;
        promptRect = promptCanvas.GetComponent<RectTransform>();
        promptRect.sizeDelta = new Vector2(420f, 92f);
        promptRect.pivot = new Vector2(0.5f, 0f);
        promptRect.localScale = Vector3.one * 0.0022f;
        promptRect.position = new Vector3(head.x, head.y + 0.22f, head.z);

        var promptBackground = new GameObject("Fundo", typeof(RectTransform), typeof(Image));
        promptBackground.transform.SetParent(promptObject.transform, false);
        var promptBackgroundRect = promptBackground.GetComponent<RectTransform>();
        promptBackgroundRect.anchorMin = Vector2.zero;
        promptBackgroundRect.anchorMax = Vector2.one;
        promptBackgroundRect.offsetMin = Vector2.zero;
        promptBackgroundRect.offsetMax = Vector2.zero;
        var promptBackgroundImage = promptBackground.GetComponent<Image>();
        promptBackgroundImage.color = new Color(0.03f, 0.035f, 0.05f, 0.72f);
        promptBackgroundImage.raycastTarget = false;

        var promptTextObject = new GameObject("Texto", typeof(RectTransform));
        promptTextObject.transform.SetParent(promptObject.transform, false);
        var promptText = promptTextObject.AddComponent<TextMeshProUGUI>();
        promptText.font = DialogueFont.Default;
        promptText.fontSize = 38f;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.color = markerColor;
        promptText.text = $"[{interactKey}] {promptLabel}";
        var promptTextRect = promptText.rectTransform;
        promptTextRect.anchorMin = Vector2.zero;
        promptTextRect.anchorMax = Vector2.one;
        promptTextRect.offsetMin = Vector2.zero;
        promptTextRect.offsetMax = Vector2.zero;

        promptRoot = promptObject;
        promptRoot.SetActive(false);
    }

    private Vector3 FeetPoint()
    {
        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null) return new Vector3(transform.position.x, renderer.bounds.min.y, transform.position.z);
        return transform.position;
    }

    private Vector3 HeadPoint()
    {
        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null) return new Vector3(transform.position.x, renderer.bounds.max.y, transform.position.z);
        return transform.position + Vector3.up * 0.4f;
    }

    private Vector3 BodySize()
    {
        var renderer = GetComponentInChildren<Renderer>();
        return renderer != null ? renderer.bounds.size : Vector3.one * 0.4f;
    }

    private static float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
}
