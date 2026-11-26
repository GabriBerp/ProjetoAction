using System;
using UnityEngine;

// Som do mundo: tiro, impacto de bala, passos de corrida. Quem faz barulho chama Noise.Emit
// e a IA dos ratos escuta por evento, sem precisar de visao, collider ou Linecast.
// O raio e do SOM (arma barulhenta alcanca mais longe); quem escuta decide se esta no alcance.
public static class Noise
{
    // (posicao do som, raio de alcance em metros)
    public static event Action<Vector3, float> Heard;

    public static void Emit(Vector3 position, float radius)
    {
        if (radius <= 0f) return;
        Heard?.Invoke(position, radius);
    }

    // Com "Enter Play Mode Options" sem domain reload, assinantes de uma sessao anterior sobrevivem.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Heard = null;
}
