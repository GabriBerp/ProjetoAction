using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerHand : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform holdPoint; // filho da câmera, na frente dela
    [SerializeField] private Backpack backpack;

    [Header("Configuração")]
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private LayerMask pickupMask = ~0;

    private IPickable heldItem;
    private IUsable heldUsable;
    private IReloadable heldReloadable;
    public event Action ItemUsed;
    public event Action ItemSwung;
    public event Action ItemTossed;

    private ISwingWeapon heldSwing;

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb[Key.E].wasPressedThisFrame && heldItem == null)
            TryPickUp();

        if (kb[Key.Q].wasPressedThisFrame && heldItem != null)
            Throw();

        if (kb[Key.F].wasPressedThisFrame)
            ToggleBackpack();

        if (Mouse.current.leftButton.wasPressedThisFrame && heldUsable != null)
        {
            heldUsable.Use();
            if (heldSwing != null) {
                ItemSwung?.Invoke();
            }
            else {
                ItemUsed?.Invoke();
            }
        }

        if (kb[Key.R].wasPressedThisFrame && heldReloadable != null)
            heldReloadable.Reload();
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
        Vector3 force = cameraTransform.forward * throwForce;
        heldItem.Drop(force);
        ClearHeld();
        ItemTossed?.Invoke();
    }

    private void SetHeld(IPickable pickable)
    {
        heldItem = pickable;
        heldUsable = (pickable as Component)?.GetComponent<IUsable>();
        heldReloadable = (pickable as Component)?.GetComponent<IReloadable>();
        heldSwing = (pickable as Component)?.GetComponent<ISwingWeapon>();
    }

    private void ClearHeld()
    {
        heldItem = null;
        heldUsable = null;
        heldReloadable = null;
        heldSwing = null;
    }
}