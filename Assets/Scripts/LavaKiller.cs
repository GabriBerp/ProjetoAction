using System.Collections.Generic;
using UnityEngine;

// Mata SOMENTE o jogador que encostar nas particulas de lava.
// O modulo Trigger do ParticleSystem e configurado aqui (inclusive a lista de colliders),
// porque sem collider registrado o OnParticleTrigger nunca dispara.
[RequireComponent(typeof(ParticleSystem))]
public class LavaKiller : MonoBehaviour
{
    [Tooltip("Collider do jogador. Vazio = encontra sozinho (layer/tag/PlayerHand).")]
    [SerializeField] private Collider playerCollider;
    [Tooltip("Camada do jogador. Vazio = usa a layer 'Player' do projeto, se existir.")]
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private int damage = 9999;
    [Tooltip("Engrossa o raio das particulas pro trigger pegar o corpo do rato.")]
    [SerializeField] private float particleRadiusScale = 1.5f;
    [SerializeField] private bool debugLogs;

    private ParticleSystem ps;
    private readonly List<ParticleSystem.Particle> entered = new List<ParticleSystem.Particle>();
    private readonly List<Collider> registered = new List<Collider>();
    private IDamageable player;
    private Health playerHealth;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        ConfigureTrigger();
        FindPlayer();
    }

    private void OnEnable()
    {
        if (ps == null) ps = GetComponent<ParticleSystem>();
        if (registered.Count == 0) FindPlayer();
    }

    // O ParticleSystem tem que estar com o modulo Trigger ligado e com Enter = Callback.
    private void ConfigureTrigger()
    {
        var trigger = ps.trigger;
        trigger.enabled = true;
        trigger.enter = ParticleSystemOverlapAction.Callback;
        trigger.exit = ParticleSystemOverlapAction.Ignore;
        trigger.inside = ParticleSystemOverlapAction.Ignore;
        trigger.outside = ParticleSystemOverlapAction.Ignore;
        trigger.radiusScale = Mathf.Max(0.1f, particleRadiusScale);
        trigger.colliderQueryMode = ParticleSystemColliderQueryMode.One;
    }

    private void FindPlayer()
    {
        if (ps == null) return;

        if (playerCollider == null)
            playerCollider = PlayerCollider();

        if (playerCollider != null && !registered.Contains(playerCollider))
        {
            ps.trigger.AddCollider(playerCollider);
            registered.Add(playerCollider);
        }

        if (player == null)
        {
            Transform root = playerCollider != null ? playerCollider.transform.root : null;
            if (root != null)
            {
                player = root.GetComponentInChildren<IDamageable>();
                playerHealth = root.GetComponentInChildren<Health>();
            }

            if (player == null)
            {
                // Sem collider arrastado: procura o personagem do jogador pela layer/tag/script.
                foreach (var candidate in Object.FindObjectsByType<PlayerHand>())
                {
                    player = candidate.GetComponentInParent<IDamageable>();
                    playerHealth = candidate.GetComponentInParent<Health>();
                    RegisterColliders(candidate.transform.root);
                    break;
                }
            }
        }
    }

    private Collider PlayerCollider()
    {
        // 1) O componente que so o jogador tem.
        var hand = Object.FindAnyObjectByType<PlayerHand>();
        if (hand != null)
        {
            var root = hand.transform.root;
            RegisterColliders(root);
            foreach (var c in root.GetComponentsInChildren<Collider>(true))
                if (c.enabled && !c.isTrigger) return c;
        }

        // 2) Tag Player.
        var byTag = GameObject.FindWithTag("Player");
        if (byTag != null)
        {
            RegisterColliders(byTag.transform.root);
            var c = byTag.GetComponentInChildren<Collider>();
            if (c != null) return c;
        }

        // 3) Layer do jogador (no projeto a layer 3 e 'Player').
        int layer = playerMask.value != 0 ? LayerMaskToLayer(playerMask) : LayerMask.NameToLayer("Player");
        if (layer >= 0)
        {
            foreach (var c in Object.FindObjectsByType<Collider>())
            {
                if (c.gameObject.layer != layer) continue;
                RegisterColliders(c.transform.root);
                return c;
            }
        }

        return null;
    }

    private void RegisterColliders(Transform root)
    {
        if (root == null || ps == null) return;

        foreach (var c in root.GetComponentsInChildren<Collider>(true))
        {
            if (registered.Contains(c)) continue;
            ps.trigger.AddCollider(c);
            registered.Add(c);
        }
    }

    private bool IsPlayer(Component other)
    {
        if (other == null) return false;

        Transform root = other.transform.root;

        // Inimigos nunca contam, mesmo que estejam na mesma layer por engano.
        if (root.GetComponentInChildren<EnemyNavmesh>() != null) return false;

        if (playerHealth != null && root == playerHealth.transform.root) return true;
        if (root.GetComponentInChildren<PlayerHand>() != null) return true;
        if (root.GetComponentInChildren<CharacterMotor>() != null) return true;
        if (root.CompareTag("Player")) return true;

        int layer = playerMask.value != 0 ? LayerMaskToLayer(playerMask) : LayerMask.NameToLayer("Player");
        return layer >= 0 && other.gameObject.layer == layer;
    }

    private void OnParticleTrigger()
    {
        if (ps == null) return;
        if (player == null) FindPlayer();
        if (player == null) return;
        if (playerHealth != null && playerHealth.IsDead) return;

        int count = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, entered, out var colliderData);
        for (int i = 0; i < count; i++)
        {
            int colliderCount = colliderData.GetColliderCount(i);
            for (int j = 0; j < colliderCount; j++)
            {
                Component other = colliderData.GetCollider(i, j);
                if (!IsPlayer(other)) continue;

                var damageable = other.GetComponentInParent<IDamageable>();
                if (damageable == null) continue;

                damageable.ReceberDano(damage);
                if (debugLogs) Debug.Log($"{name}: particula de lava matou o jogador.");
                return;
            }
        }
    }

    private static int LayerMaskToLayer(LayerMask mask)
    {
        for (int i = 0; i < 32; i++)
            if ((mask.value & (1 << i)) != 0) return i;
        return -1;
    }
}
