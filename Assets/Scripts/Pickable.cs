using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Pickable : MonoBehaviour, IPickable
{
    [SerializeField] private Vector3 heldLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 heldLocalEuler = Vector3.zero;

    private Rigidbody rb;
    private Collider[] colliders;
    private IHoldListener[] listeners;

    public bool IsHeld { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
        listeners = GetComponents<IHoldListener>();
    }
    
    public void PickUp(Transform holdPoint)
    {
        IsHeld = true;

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;       // Unity 6. Em versões antigas: rb.velocity
        rb.angularVelocity = Vector3.zero;

        foreach (var c in colliders) c.enabled = false;

        transform.SetParent(holdPoint);
        transform.localPosition = heldLocalPosition;
        transform.localRotation = Quaternion.Euler(heldLocalEuler);

        foreach (var l in listeners) l.OnPickedUp();
    }

    public void Drop(Vector3 force)
    {
        IsHeld = false;

        transform.SetParent(null);

        foreach (var c in colliders) c.enabled = true;

        rb.isKinematic = false;
        rb.AddForce(force, ForceMode.VelocityChange);

        foreach (var l in listeners) l.OnDropped();
    }
}