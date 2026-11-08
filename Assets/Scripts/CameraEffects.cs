using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CharacterMotor motor;

    [Header("Balanço")]
    [SerializeField] private float frequency = 1.8f;
    [SerializeField] private float verticalAmount = 0.05f;
    [SerializeField] private float horizontalAmount = 0.03f;
    [SerializeField] private float bobSmooth = 8f;
    [SerializeField] private float maxSpeedReference = 7.5f;

    [Header("Impacto")]
    [SerializeField] private float minFallSpeed = 3f;
    [SerializeField] private float maxFallSpeed = 15f;
    [SerializeField] private float maxDip = 0.25f;
    [SerializeField] private float stiffness = 120f;
    [SerializeField] private float damping = 12f;

    private Vector3 restPos;        // posição de repouso, nunca muda
    private Vector3 bobOffset;      // estado do balanço
    private float timer;
    private float impactOffset;     // estado do impacto
    private float impactVel;

    void Awake()
    {
        restPos = transform.localPosition;
    }

    void OnEnable()  { motor.Landed += OnLanded; }
    void OnDisable() { motor.Landed -= OnLanded; }

    void OnLanded(float fallSpeed)
    {
        if (fallSpeed < minFallSpeed) return;
        float t = Mathf.InverseLerp(minFallSpeed, maxFallSpeed, fallSpeed);
        impactOffset = -maxDip * t;
        impactVel = 0f;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // --- Balanço ---
        Vector3 vel = rb.linearVelocity;
        vel.y = 0f;
        float speed = vel.magnitude;

        Vector3 bobTarget = Vector3.zero;
        if (speed > 0.1f && motor.IsGrounded)
        {
            float speedFactor = speed / maxSpeedReference;
            timer += dt * speed * frequency;
            bobTarget = new Vector3(
                Mathf.Cos(timer) * horizontalAmount * speedFactor,
                Mathf.Sin(timer * 2f) * verticalAmount * speedFactor,
                0f);
        }
        bobOffset = Vector3.Lerp(bobOffset, bobTarget, dt * bobSmooth);

        // --- Impacto (mola amortecida) ---
        float force = -stiffness * impactOffset - damping * impactVel;
        impactVel += force * dt;
        impactOffset += impactVel * dt;

        // Posição absoluta: sempre repouso + efeitos
        transform.localPosition = restPos + bobOffset + new Vector3(0f, impactOffset, 0f);
        
        // Debug.Log($"rest:{restPos.y:F3}  impact:{impactOffset:F3}  local:{transform.localPosition.y:F3}  parent:{transform.parent.position.y:F3}");
    }
}