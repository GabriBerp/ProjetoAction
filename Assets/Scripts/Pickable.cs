using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Pickable : MonoBehaviour, IPickable
{
    [Header("Seguro pelo jogador (local ao ponto de pega)")]
    [SerializeField] private Vector3 heldLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 heldLocalEuler = Vector3.zero;

    [Header("Seguro pelo inimigo (local ao ponto de pega dele)")]
    [Tooltip("O ponto do inimigo costuma ser um pouco mais abaixo do ponto do jogador.")]
    [SerializeField] private Vector3 enemyHeldLocalPosition = new Vector3(0f, -0.18f, 0f);
    [SerializeField] private Vector3 enemyHeldLocalEuler = Vector3.zero;

    [Header("Arremesso")]
    [Tooltip("Dano (Z) causado quando o item arremessado acerta um alvo. Nunca negativo.")]
    [SerializeField] private int throwDamage = 5;

    private Rigidbody rb;
    private Collider[] colliders;
    private IHoldListener[] listeners;

    public bool IsHeld { get; private set; }
    public int ThrowDamage => Mathf.Max(0, throwDamage);
    private CuboGrabber currentGrabber;

    // Em qual ponto o item esta encaixado agora (cada um tem sua pose).
    public HoldSlot Slot { get; private set; } = HoldSlot.Player;

    // Tamanho "do item" (mundo) guardado na primeira pega: evita encolher a cada troca de dono.
    private Vector3 ownScale = Vector3.one;
    private bool ownScaleKnown;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
        listeners = GetComponents<IHoldListener>();
    }

    public void PickUp(Transform holdPoint) => PickUpFor(holdPoint, HoldSlot.Player);

    public void PickUpFor(Transform holdPoint, HoldSlot slot)
    {
        if (holdPoint == null) return;

        // Se estava encaixado em algum suporte, libera o suporte.
        if (currentGrabber != null)
        {
            currentGrabber.Release(this);
            currentGrabber = null;
        }

        bool wasHeld = IsHeld;
        IsHeld = true;
        Slot = slot;

        // Zera antes de virar kinematic: setar velocidade em corpo kinematic gera warning.
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        foreach (var c in colliders)
            c.enabled = false;

        // A escala do item NAO acompanha a escala de quem segura: um rato Big nao deixa
        // a arma gigante. O tamanho "dele mesmo" e o localScale com que foi montado (que
        // ja pode estar dentro de um HoldPoint escalado pelo editor).
        if (!wasHeld || !ownScaleKnown) { ownScale = transform.localScale; ownScaleKnown = true; }

        transform.SetParent(holdPoint, true);
        KeepWorldScale(ownScale, holdPoint);
        ApplyHoldPose(slot);

        foreach (var l in listeners)
            l.OnPickedUp();
    }

    public void Drop(Vector3 force)
    {
        if (currentGrabber != null)
        {
            currentGrabber.Release(this);
            currentGrabber = null;
        }

        IsHeld = false;

        // Sai da hierarquia mantendo o tamanho que tinha na mao.
        Vector3 worldScale = transform.lossyScale;
        transform.SetParent(null, true);
        transform.localScale = worldScale;

        foreach (var c in colliders)
            c.enabled = true;

        rb.isKinematic = false;
        rb.AddForce(force, ForceMode.VelocityChange);

        foreach (var l in listeners)
            l.OnDropped();
    }

    // Coloca o item na pose do slot (jogador ou inimigo). Pode ser chamado pelo dono.
    public void ApplyHoldPose(HoldSlot slot)
    {
        bool enemy = slot == HoldSlot.Enemy;

        transform.localPosition = enemy ? enemyHeldLocalPosition : heldLocalPosition;
        transform.localRotation = Quaternion.Euler(enemy ? enemyHeldLocalEuler : heldLocalEuler);
    }

    // Ajusta a pose do slot em que o item esta agora (ex: depois de trocar de dono).
    public void ApplyHoldPose() => ApplyHoldPose(Slot);

    // Sobrescreve a pose de um slot em tempo de execucao (spawner, prefabs antigos, etc).
    public void SetHoldPose(HoldSlot slot, Vector3 localPosition, Vector3 localEuler)
    {
        if (slot == HoldSlot.Enemy)
        {
            enemyHeldLocalPosition = localPosition;
            enemyHeldLocalEuler = localEuler;
        }
        else
        {
            heldLocalPosition = localPosition;
            heldLocalEuler = localEuler;
        }

        if (IsHeld && Slot == slot) ApplyHoldPose(slot);
    }

    public void Dock(CuboGrabber grabber, Transform holdPoint)
    {
        if (IsHeld || currentGrabber != null)
            return;

        currentGrabber = grabber;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    // localScale = escala de mundo desejada / escala do pai (compensa o pai estar escalado).
    private void KeepWorldScale(Vector3 worldScale, Transform parent)
    {
        Vector3 parentScale = parent != null ? parent.lossyScale : Vector3.one;

        transform.localScale = new Vector3(
            Mathf.Abs(parentScale.x) > 0.0001f ? worldScale.x / parentScale.x : worldScale.x,
            Mathf.Abs(parentScale.y) > 0.0001f ? worldScale.y / parentScale.y : worldScale.y,
            Mathf.Abs(parentScale.z) > 0.0001f ? worldScale.z / parentScale.z : worldScale.z);
    }
}
