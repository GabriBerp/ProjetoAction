using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class WaterVolume : MonoBehaviour
{
    private BoxCollider col;

    void Reset() => GetComponent<BoxCollider>().isTrigger = true;
    void Awake() => col = GetComponent<BoxCollider>();

    void OnTriggerStay(Collider other)
    {
        var motor = other.GetComponentInParent<CharacterMotor>();
        if (!motor) return;

        float surfaceY = col.bounds.max.y;   // topo do trigger = superfície
        Bounds b = other.bounds;
        motor.Submersion = Mathf.Clamp01((surfaceY - b.min.y) / b.size.y);
    }

    void OnTriggerExit(Collider other)
    {
        var motor = other.GetComponentInParent<CharacterMotor>();
        if (motor) motor.Submersion = 0f;
    }
}