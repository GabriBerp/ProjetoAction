using UnityEngine;
using UnityEngine.Events;

public class IcePuzzle : MonoBehaviour
{
    [SerializeField] private IceCrystal[] crystals;
    public UnityEvent onPuzzleCompleted;   // ligue animações/portas pelo Inspector

    // Evento estático opcional, para outros scripts escutarem por código
    public static event System.Action<int> PuzzleCompleted;
    [SerializeField] private int puzzleId = 1;

    private int remaining;
    private bool completed;

    private void Awake()
    {
        remaining = crystals.Length;
        foreach (var c in crystals) c.Init(this);
    }

    public void NotifyCrystalBroken(IceCrystal crystal)
    {
        if (completed) return;
        remaining--;

        if (remaining <= 0)
        {
            completed = true;
            onPuzzleCompleted?.Invoke();
            PuzzleCompleted?.Invoke(puzzleId);
        }
    }
}