using System;
using UnityEngine;

[RequireComponent(typeof(CharacterMotor), typeof(Rigidbody))]
public class Glider : MonoBehaviour
{
    [SerializeField] private float glideFallSpeed = 1.5f;
    [SerializeField] private float maxGlideSeconds = 3f;
    [SerializeField] private float rechargeSeconds = 4f;
    [SerializeField, Range(0f, 1f)] private float minStaminaToStart = 0.2f;
    [SerializeField] private bool rechargeOnlyOnGround = false;
    [SerializeField] private MeshRenderer gliderVisual;

    private CharacterMotor motor;
    private IMoveInput input;
    private Rigidbody rb;

    private float stamina = 1f;
    private bool gliding;

    // Só plana depois de soltar o espaço no ar, para segurar o pulo inteiro não começar a planar sozinho no ápice.
    private bool armed;

    public bool IsGliding => gliding;
    public float StaminaNormalized => stamina;

    public event Action GlideStarted;
    public event Action GlideStopped;

    void Awake()
    {
        motor = GetComponent<CharacterMotor>();
        input = GetComponent<IMoveInput>();
        rb = GetComponent<Rigidbody>();
        if (gliderVisual != null) gliderVisual.enabled = false;
    }

    void OnDisable()
    {
        if (gliding) SetGliding(false);
    }

    void Update()
    {
        bool held = input.JumpHeld && !motor.Locked;

        if (motor.IsGrounded) armed = false;
        else if (!input.JumpHeld) armed = true;

        bool canGlide = !motor.IsGrounded
                        && rb.linearVelocity.y < 0f
                        && armed
                        && held;

        if (!gliding && canGlide && stamina >= minStaminaToStart)
            SetGliding(true);
        else if (gliding && !canGlide)
            SetGliding(false);

        if (gliding)
        {
            stamina = Mathf.Max(0f, stamina - Time.deltaTime / maxGlideSeconds);
            if (stamina <= 0f)
            {
                // exige soltar e apertar de novo, senão reinicia o planar no mesmo frame
                armed = false;
                SetGliding(false);
            }
        }
        else if (!rechargeOnlyOnGround || motor.IsGrounded)
        {
            stamina = Mathf.Min(1f, stamina + Time.deltaTime / rechargeSeconds);
        }
    }

    private void SetGliding(bool value)
    {
        gliding = value;
        motor.MaxFallSpeed = value ? glideFallSpeed : float.PositiveInfinity;
        if (gliderVisual != null) gliderVisual.enabled = value;

        if (value) GlideStarted?.Invoke();
        else GlideStopped?.Invoke();
    }
}