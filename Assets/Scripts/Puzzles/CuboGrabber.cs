
using UnityEngine;
using System;

public class CuboGrabber : MonoBehaviour
{
    [Header("Identificação")]
    [SerializeField] private MeshRenderer colorId;

    [Header("Encaixe")]
    [SerializeField] private Transform holdPoint;

    private Pickable heldItem;

    public Pickable Held => heldItem;

    public bool IsFilled => heldItem != null;

    public event Action StateChanged;

    private void OnCollisionEnter(Collision collision)
    {
        if (heldItem != null)
            return;

        Pickable pickable =
            collision.collider.GetComponentInParent<Pickable>();

        if (pickable == null)
            return;

        TryDock(pickable);
    }

    private void TryDock(Pickable pickable)
    {
        if (pickable == null || pickable.IsHeld)
            return;

        if (pickable == heldItem)
            return;

        // Procura o MeshRenderer responsável pela cor do cubo.
        MeshRenderer itemColor =
            pickable.GetComponentInChildren<MeshRenderer>();

        if (itemColor == null || colorId == null)
            return;

        // Compara os materiais dos dois objetos.
        if (itemColor.sharedMaterial != colorId.sharedMaterial)
            return;

        heldItem = pickable;

        pickable.Dock(this, holdPoint);

        StateChanged?.Invoke();
    }

    public void Release(Pickable pickable)
    {
        if (heldItem != pickable)
            return;

        heldItem = null;

        StateChanged?.Invoke();
    }
}
