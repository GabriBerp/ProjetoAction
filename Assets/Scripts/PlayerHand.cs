using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerHand : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform holdPoint; // filho da câmera, na frente dela
    [SerializeField] private Backpack backpack;
    [Tooltip("Vazio = procura no proprio objeto ou nos pais.")]
    [SerializeField] private Health health;
    [SerializeField] private Defense defense;

    [Header("Configuração")]
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float maxThrowMultiplier = 10f;
    [SerializeField] private LayerMask pickupMask = ~0;

    private IPickable heldItem;
    private IUsable heldUsable;
    private IReloadable heldReloadable;
    private ISwingWeapon heldSwing;
    private IOwnedItem heldOwned;
    private SwingWeapon heldWeapon;

    public event Action ItemUsed;
    public event Action ItemSwung;
    public event Action ItemTossed;

    private void Awake()
    {
        if (health == null) health = GetComponentInParent<Health>();
        if (defense == null) defense = GetComponentInParent<Defense>();
    }

    private void Update()
    {
        var kb = Keyboard.current;
        bool stunned = health != null && health.IsStunned;

        if (kb[Key.E].wasPressedThisFrame && heldItem == null)
            TryPickUp();

        if (kb[Key.Q].wasPressedThisFrame && heldItem != null)
            Throw();

        if (kb[Key.F].wasPressedThisFrame)
            ToggleBackpack();

        if (Mouse.current.leftButton.wasPressedThisFrame && heldUsable != null && !stunned)
            UseHeld();

        UpdateBlock(stunned);

        if (kb[Key.R].wasPressedThisFrame && heldReloadable != null)
            heldReloadable.Reload();
    }

    private void UseHeld()
    {
        if (heldSwing == null)
        {
            heldUsable.Use();
            ItemUsed?.Invoke();
            return;
        }

        // A animacao so toca se a arma aceitou o golpe (cooldown, bloqueio e stun ficam na arma).
        if (!heldSwing.CanSwing) return;
        heldUsable.Use();
        ItemSwung?.Invoke();
    }

    // Segurar = bloqueia. O parry depende do instante em que o botao e APERTADO, por isso so
    // wasPressedThisFrame inicia o bloqueio: segurar o botao antes nao reinicia a janela.
    private void UpdateBlock(bool stunned)
    {
        if (defense == null) return;

        var button = Mouse.current.rightButton;
        if (stunned || heldSwing == null || !button.isPressed)
        {
            defense.StopBlock();
            return;
        }

        if (button.wasPressedThisFrame && !heldSwing.IsSwinging)
            defense.TryStartBlock();
    }

    private void TryPickUp()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupMask,
                             QueryTriggerInteraction.Ignore))
            return;

        var pickable = hit.collider.GetComponentInParent<IPickable>();
        if (pickable == null || pickable.IsHeld) return;

        pickable.PickUp(holdPoint);
        SetHeld(pickable);
    }

    private void ToggleBackpack()
    {
        if (backpack == null) return;

        if (heldItem != null)
        {
            // Com item na mão: guarda (só se a bolsa tiver espaço)
            if (backpack.TryStore(heldItem))
                ClearHeld();
        }
        else
        {
            // Mão vazia: puxa o último item guardado
            if (backpack.TryRetrieve(out IPickable item))
            {
                item.PickUp(holdPoint);
                SetHeld(item);
            }
        }
    }

    private void Throw()
    {
        Rigidbody rb = (heldItem as Component)?.GetComponent<Rigidbody>();
        float mass = rb != null ? rb.mass : 1f;

        float multiplier = Mathf.Min(1f / Mathf.Max(mass, 0.0001f), maxThrowMultiplier);
        Vector3 force = cameraTransform.forward * (throwForce * multiplier);

        // Z vem do proprio item: qualquer coisa seguravel tem dano de arremesso.
        var component = heldItem as Component;
        int throwDamage = component != null && component.TryGetComponent(out Pickable pickable)
            ? pickable.ThrowDamage
            : 0;

        heldItem.Drop(force);

        if (component != null)
        {
            var thrown = component.GetComponent<ThrownItem>();
            if (thrown == null) thrown = component.gameObject.AddComponent<ThrownItem>();
            thrown.Launch(throwDamage, health);
        }

        ClearHeld();
        ItemTossed?.Invoke();
    }

    private void SetHeld(IPickable pickable)
    {
        heldItem = pickable;
        heldUsable = (pickable as Component)?.GetComponent<IUsable>();
        heldReloadable = (pickable as Component)?.GetComponent<IReloadable>();
        heldSwing = (pickable as Component)?.GetComponent<ISwingWeapon>();
        heldWeapon = (pickable as Component)?.GetComponent<SwingWeapon>();
        if (heldWeapon != null) heldWeapon.Broke += OnHeldWeaponBroke;

        heldOwned = (pickable as Component)?.GetComponent<IOwnedItem>();
        if (heldOwned != null) heldOwned.SetOwner(health, cameraTransform);
    }

    private void ClearHeld()
    {
        if (heldWeapon != null) { heldWeapon.Broke -= OnHeldWeaponBroke; heldWeapon = null; }
        if (heldOwned != null) heldOwned.SetOwner(null, null);
        heldOwned = null;
        heldItem = null;
        heldUsable = null;
        heldReloadable = null;
        heldSwing = null;
    }

    // A arma quebrou na mao: a referencia nao pode continuar apontando pra ela.
    private void OnHeldWeaponBroke(SwingWeapon weapon)
    {
        if (weapon != heldWeapon) return;
        ClearHeld();
    }
}
