using UnityEngine;

// Dano de queda por distancia. Serve para o jogador e para os inimigos.
//
// A conta e feita pela altura perdida entre o ponto mais alto da queda e o chao:
//  - cair menos que safeHeight nao machuca;
//  - acima disso, cada metro vale damagePerMeter, com teto em maxDamage.
//
// O Glider suspende o dano enquanto esta planando. Ao parar de planar no ar,
// a contagem recomeca daquela altura (nao conta o que ja foi planado).
[RequireComponent(typeof(Health))]
public class FallDamage : MonoBehaviour
{
    [Header("Queda")]
    [Tooltip("Altura de queda que ainda nao machuca (metros).")]
    [SerializeField] private float safeHeight = 4f;

    [Tooltip("Dano por metro caindo acima da altura segura.")]
    [SerializeField] private float damagePerMeter = 7f;

    [Tooltip("Teto de dano de uma queda so.")]
    [SerializeField] private int maxDamage = 100;

    [Header("Deteccao de chao")]
    [Tooltip("O que conta como chao para encerrar a queda.")]
    [SerializeField] private LayerMask groundMask = ~0;

    [Tooltip("Folga extra abaixo dos pes para considerar que esta apoiado (metros).")]
    [SerializeField] private float groundTolerance = 0.15f;

    [Tooltip("Salto de altura em um frame que e teleporte (porta, respawn), nao queda (metros).")]
    [SerializeField] private float teleportThreshold = 3f;

    private Health health;
    private Rigidbody body;
    private Collider[] colliders;

    private bool suspended;   // planando: sem dano e sem contar
    private bool tracking;    // queda em andamento
    private float startY;     // altura mais alta da queda em andamento
    private float lastY;      // altura do frame anterior (detecta teleporte)

    // Disparado ao terminar uma queda que machucou (dano aplicado). Util para feedback.
    public event System.Action<int> FallHit;

    private void Awake()
    {
        health = GetComponent<Health>();
        body = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>(true);
    }

    // Garante o componente em um personagem que ainda nao tem.
    public static FallDamage Ensure(GameObject target)
    {
        if (target == null) return null;
        if (target.TryGetComponent(out FallDamage existing)) return existing;
        if (target.GetComponent<Health>() == null) return null;

        return target.AddComponent<FallDamage>();
    }

    // Jogador e inimigos ganham o componente sozinhos, como o PlayerDeathSequence.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        foreach (var motor in FindObjectsByType<CharacterMotor>(FindObjectsInactive.Exclude))
            Ensure(motor.gameObject);

        foreach (var enemy in FindObjectsByType<EnemyNavmesh>(FindObjectsInactive.Exclude))
            Ensure(enemy.gameObject);
    }

    // O planador chama true ao abrir e false ao fechar.
    public void SetSuspended(bool value)
    {
        suspended = value;

        // Retomou a queda: conta a partir daqui.
        tracking = !value;
        startY = value ? 0f : transform.position.y;
    }

    private void Update()
    {
        float y = transform.position.y;

        // Porta, respawn ou reposicionamento: nao e queda.
        bool teleported = Mathf.Abs(y - lastY) > teleportThreshold;
        lastY = y;

        if (teleported)
        {
            tracking = false;
            startY = y;
            return;
        }

        // Pulo roteirizado do inimigo (corpo cinemático) nao e queda.
        if (body != null && body.isKinematic)
        {
            tracking = false;
            return;
        }

        if (IsGrounded())
        {
            if (tracking) Land(y);
            tracking = false;
            return;
        }

        if (suspended) return;

        if (!tracking)
        {
            tracking = true;
            startY = y;
        }
        else if (y > startY)
        {
            startY = y;   // subiu antes de cair: a queda conta do ponto mais alto
        }
    }

    private void Land(float y)
    {
        float drop = startY - y;
        if (drop < safeHeight) return;

        int damage = Mathf.Clamp(Mathf.RoundToInt((drop - safeHeight) * damagePerMeter), 0, maxDamage);
        if (damage <= 0) return;

        FallHit?.Invoke(damage);
        health.ReceberDano(damage);
    }

    private bool IsGrounded()
    {
        if (colliders == null) return true;

        foreach (var collider in colliders)
        {
            if (collider == null || !collider.enabled) continue;

            var bounds = collider.bounds;
            var origin = new Vector3(bounds.center.x, bounds.min.y + 0.02f, bounds.center.z);
            float distance = bounds.extents.y * 0.5f + groundTolerance;

            if (Physics.Raycast(origin, Vector3.down, distance, groundMask, QueryTriggerInteraction.Ignore))
                return true;
        }

        return false;
    }
}
