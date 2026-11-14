using System.Collections.Generic;
using UnityEngine;

// Nodulo de patrulha. Basta colocar este componente em objetos vazios espalhados pela cena.
// Ele se registra sozinho, entao os inimigos nao precisam de FindObjectsByType.
public class PatrolNode : MonoBehaviour
{
    static readonly List<PatrolNode> all = new();
    public static IReadOnlyList<PatrolNode> All => all;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => all.Clear();

    void OnEnable() => all.Add(this);
    void OnDisable() => all.Remove(this);

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.6f);
        Gizmos.DrawSphere(transform.position, 0.3f);
    }
}