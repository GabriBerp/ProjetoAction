using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoveInput : MonoBehaviour, IMoveInput
{
    public Vector3 MoveDirection { get; private set; }
    public bool Run { get; private set; }

    private bool jumpQueued;

    public bool ConsumeJump()
    {
        bool result = jumpQueued;
        jumpQueued = false;
        return result;
    }

    void Update()
    {
        /*
        if (!GameManager.GameStarted)
        {
            MoveDirection = Vector3.zero;
            Run = false;
            return;
        }
        */

        var kb = Keyboard.current;
        Vector3 dir = Vector3.zero;

        if (kb[Key.W].isPressed) dir += transform.forward;
        if (kb[Key.S].isPressed) dir -= transform.forward;
        if (kb[Key.D].isPressed) dir += transform.right;
        else if (kb[Key.A].isPressed) dir -= transform.right;

        MoveDirection = dir;
        Run = kb[Key.LeftShift].isPressed;

        jumpQueued |= kb[Key.Space].wasPressedThisFrame;
    }
}