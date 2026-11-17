using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMotor : MonoBehaviour
{
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float runMultiplier = 1.5f;
    [SerializeField] private float jumpForce = 6f;

    private Rigidbody rb;
    private IMoveInput input;
    private int onGround = 0;
    private float lastFallSpeed;   // velocidade vertical (descendo) do último FixedUpdate
    private Vector3 localDir;
    private bool jumpRequested;

    public bool IsGrounded => onGround > 0;

    // Parâmetro: velocidade da queda no momento do impacto (valor positivo)
    public event Action<float> Landed;
    private float speed;
    public event Action Jumped;
    public bool IsMoving => localDir.sqrMagnitude > 0.01f;
    public bool Locked { get; set; }   // true = stun/morte (não move nem pula)
    private PlayerLook look;
    [SerializeField] private float airJumpForce = 5.5f;
    [SerializeField] private int maxAirJumps = 1;
    [SerializeField] private float coyoteTime = 0.12f;

    private float coyoteTimer;
    private int airJumpsLeft;

    public event Action AirJumped;
    public float MaxFallSpeed { get; set; } = float.PositiveInfinity;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<IMoveInput>();
        look = GetComponent<PlayerLook>();
    }

    private void Update()
    {
        if (Locked)
        {
            localDir = Vector3.zero;
            input.ConsumeJump();
            jumpRequested = false;
        }
        else
        {
            localDir = Vector3.ClampMagnitude(input.MoveDirection, 1f);
            // acumula: se o FixedUpdate ainda não rodou, o pedido não se perde
            if (input.ConsumeJump()) jumpRequested = true;
        }

        speed = input.Run ? baseSpeed * runMultiplier : baseSpeed;
    }

    void FixedUpdate()
    {
        float yVel = rb.linearVelocity.y;

        if (!IsGrounded)
            lastFallSpeed = Mathf.Max(0f, -yVel);
        else if (yVel < 0f)
            yVel = 0f;

        // yVel <= 0.01 evita recarregar pulos nos ticks logo após o pulo, quando
        // o OnCollisionExit ainda não chegou e IsGrounded continua true.
        if (IsGrounded && yVel <= 0.01f)
        {
            coyoteTimer = coyoteTime;
            airJumpsLeft = maxAirJumps;
        }
        else
        {
            coyoteTimer = Mathf.Max(0f, coyoteTimer - Time.fixedDeltaTime);
        }

        if (jumpRequested)
        {
            jumpRequested = false;
            if (!Locked)
            {
                if (coyoteTimer > 0f)
                {
                    yVel = jumpForce;
                    coyoteTimer = 0f;   // impede um segundo pulo "de chão" durante a subida
                    Jumped?.Invoke();
                }
                else if (airJumpsLeft > 0)
                {
                    yVel = airJumpForce;
                    airJumpsLeft--;
                    AirJumped?.Invoke();
                }
            }
        }

        yVel = Mathf.Max(yVel, -MaxFallSpeed);

        Quaternion facing = look != null
            ? Quaternion.Euler(0f, look.Yaw, 0f)
            : transform.rotation;

        Vector3 worldDir = facing * localDir;
        rb.linearVelocity = new Vector3(worldDir.x * speed, yVel, worldDir.z * speed);
    }

    void OnCollisionEnter(Collision c)
    {
        if (!c.gameObject.CompareTag("Ground")) return;

        // Só dispara se estava no ar (onGround == 0 antes de incrementar)
        if (onGround == 0 && lastFallSpeed > 0f)
        {
            Landed?.Invoke(lastFallSpeed);
            lastFallSpeed = 0f;
        }
        onGround++;
    }

    void OnCollisionExit(Collision c)
    {
        if (c.gameObject.CompareTag("Ground")) onGround--;
    }
}