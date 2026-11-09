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

    public bool IsGrounded => onGround > 0;

    // Parâmetro: velocidade da queda no momento do impacto (valor positivo)
    public event Action<float> Landed;
    private Vector3 dir;
    private float speed;
    private float yVel;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<IMoveInput>();
    }

    private void Update() {
        dir = Vector3.ClampMagnitude(input.MoveDirection, 1f);
        speed = input.Run ? baseSpeed * runMultiplier : baseSpeed;

        yVel = rb.linearVelocity.y;

        if (!IsGrounded)
            lastFallSpeed = Mathf.Max(0f, -yVel);
        else if (yVel < 0f)
            yVel = 0f; // no chão, não reaplica velocidade descendente

        if (input.ConsumeJump() && IsGrounded)
            yVel = jumpForce;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector3(dir.x * speed, yVel, dir.z * speed);
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