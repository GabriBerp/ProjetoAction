using UnityEngine;

public class LavaPool : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        var damageable = other.GetComponentInParent<IDamageable>();
        damageable?.ReceberDano(9999);
    }
}
