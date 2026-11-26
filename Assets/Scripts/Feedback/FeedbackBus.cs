using System;
using UnityEngine;

// Canal unico entre gameplay e popups no mundo. Quem raisa so informa o que aconteceu e onde;
// quem escuta (FeedbackPopups) decide como mostrar.
public static class FeedbackBus
{
    public static event Action<FeedbackCue, Vector3, int> Raised;

    public static void Raise(FeedbackCue cue, Vector3 position, int value = 0) =>
        Raised?.Invoke(cue, position, value);

    // Cues sem texto no mundo (Pickup, Store, Drop...) devolvem false e nao geram popup.
    public static bool TryDescribe(FeedbackCue cue, int value, out string text, out Color color)
    {
        switch (cue)
        {
            case FeedbackCue.Damaged:
                text = $"-{value}"; color = new Color(1f, 0.55f, 0.2f); return true;
            case FeedbackCue.LowHealth:
                text = "Vida baixa"; color = new Color(0.9f, 0.25f, 0.2f); return true;
            case FeedbackCue.Empty:
                text = "Vazio"; color = new Color(0.8f, 0.8f, 0.8f); return true;
            case FeedbackCue.UseFailed:
                text = "Falhou"; color = new Color(1f, 0.75f, 0.2f); return true;
            case FeedbackCue.Reload:
                text = "Recarregando"; color = Color.white; return true;
            case FeedbackCue.ReloadComplete:
                text = "Pronto"; color = new Color(0.35f, 0.85f, 0.4f); return true;
            default:
                text = null; color = default; return false;
        }
    }

    // Com "Enter Play Mode Options" sem domain reload, assinantes de uma sessao anterior sobreviveriam.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Raised = null;
}
