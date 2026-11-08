using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHand : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform holdPoint; // filho da câmera, na frente dela

    [Header("Configuração")]
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private LayerMask pickupMask = ~0;

    private IPickable heldItem;
    private IUsable heldUsable;
    private IReloadable heldReloadable;

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb[Key.E].wasPressedThisFrame && heldItem == null)
            TryPickUp();

        if (kb[Key.Q].wasPressedThisFrame && heldItem != null)
            Throw();

        if (Mouse.current.leftButton.wasPressedThisFrame && heldUsable != null)
            heldUsable.Use();

        if (kb[Key.R].wasPressedThisFrame && heldReloadable != null)
            heldReloadable.Reload();
    }

    private void TryPickUp()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupMask,
                             QueryTriggerInteraction.Ignore))
            return;

        // Usa o Rigidbody pai caso o collider esteja em um filho
        var pickable = hit.collider.GetComponentInParent<IPickable>();
        if (pickable == null || pickable.IsHeld) return;

        pickable.PickUp(holdPoint);
        heldItem = pickable;

        // Pega o uso, se existir (item sem uso simplesmente fica null)
        heldUsable = (pickable as Component)?.GetComponent<IUsable>();
        heldReloadable = (pickable as Component)?.GetComponent<IReloadable>();
    }

    private void Throw()
    {
        // Soma a velocidade do jogador pra o arremesso ficar natural (opcional)
        Vector3 force = cameraTransform.forward * throwForce;

        heldItem.Drop(force);
        heldItem = null;
        heldUsable = null;
        heldReloadable = null;
    }
}