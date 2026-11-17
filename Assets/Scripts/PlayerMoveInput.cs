using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoveInput : MonoBehaviour, IMoveInput
{
    public Vector3 MoveDirection { get; private set; }
    public bool Run { get; private set; }
    private bool jumpQueued;
    public bool JumpHeld { get; private set; }

    public bool ConsumeJump()
    {
        bool result = jumpQueued;
        jumpQueued = false;
        return result;
    }

    void Update()
    {
        var kb = Keyboard.current;
        Vector3 dir = Vector3.zero;

        if (kb[Key.W].isPressed) dir.z += 1f;
        if (kb[Key.S].isPressed) dir.z -= 1f;
        if (kb[Key.D].isPressed) dir.x += 1f;
        else if (kb[Key.A].isPressed) dir.x -= 1f;

        MoveDirection = dir;   // agora é LOCAL
        Run = kb[Key.LeftShift].isPressed;
        JumpHeld = kb[Key.Space].isPressed;

        jumpQueued |= kb[Key.Space].wasPressedThisFrame;
    }
}