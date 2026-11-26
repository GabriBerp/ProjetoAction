
using UnityEngine;
using System;

public class CuboPuzzleManager : MonoBehaviour
{
    [SerializeField] private CuboGrabber[] grabbers;

    public bool IsSolved { get; private set; }

    public event Action PuzzleCompleted;

    private void OnEnable()
    {
        foreach (var grabber in grabbers)
        {
            if (grabber != null)
                grabber.StateChanged += CheckPuzzle;
        }
    }

    private void OnDisable()
    {
        foreach (var grabber in grabbers)
        {
            if (grabber != null)
                grabber.StateChanged -= CheckPuzzle;
        }
    }

    private void CheckPuzzle()
    {
        if (IsSolved)
            return;

        foreach (var grabber in grabbers)
        {
            if (grabber == null || !grabber.IsFilled)
                return;
        }

        IsSolved = true;

        Debug.Log("Enigma dos cubos concluído!");

        PuzzleCompleted?.Invoke();
    }
}
