using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNavmesh : MonoBehaviour, IRatSized, ILockable
{
    public enum EnemyState { Patrol, Chase, Resting, Dead }

    [Header("Referencias")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Health health;
    [SerializeField] private EnemyAttack attack;
    [Tooltip("Posicao dos olhos (origem da visao). Se vazio usa o proprio transform.")]
    [SerializeField] private Transform eyes;

    [Header("Tamanho / Stats base")]
    [SerializeField] private RatSize size = RatSize.Normal;
    [SerializeField] private bool applySizeScale = true;
    [SerializeField] private int baseHealth = 100;
    [SerializeField] private float baseSpeed = 3f;
    [SerializeField] private float turnSpeed = 360f;
    [SerializeField] private bool autoSetupOnStart = true;

    [Header("Patrulha (Nodulos)")]
    [Tooltip("Alcance a partir do ponto de spawn em que ele pode escolher nodulos.")]
    [SerializeField] private float nodeSearchRadius = 40f;
    [Tooltip("Precisao: chegou a essa distancia = visitado (e os vizinhos dentro dela tambem).")]
    [SerializeField] private float nodeReachDistance = 8f;
    [Tooltip("Se nao chegar no nodulo nesse tempo, marca como visitado (evita ficar preso).")]
    [SerializeField] private float nodeTimeout = 40f;

    [Header("Visao (cone)")]
    [SerializeField] private float viewDistance = 12f;
    [SerializeField, Range(10f, 360f)] private float viewAngle = 90f;
    [Tooltip("Raio em que ele percebe o jogador mesmo fora do cone (ex: pelas costas).")]
    [SerializeField] private float senseRadius = 2f;
    [SerializeField] private LayerMask playerMask;       // Layer "Jogador"
    [Tooltip("Paredes/obstaculos que bloqueiam a visao. NAO inclua a layer do jogador nem dos inimigos.")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float visionInterval = 0.15f;

    [Header("Perseguicao")]
    [SerializeField] private float chaseSpeedMultiplier = 1.25f;
    [Tooltip("Tempo sem ver o jogador ate desistir (ele vai ate a ultima posicao conhecida).")]
    [SerializeField] private float loseSightTime = 4f;

    [Header("Folego (so tamanhos com getsWinded)")]
    [SerializeField] private Vector2 restTime = new Vector2(3f, 5f);
    [Tooltip("De quanto em quanto tempo de perseguicao ele para pra respirar.")]
    [SerializeField] private Vector2 chaseTireInterval = new Vector2(6f, 12f);

    // ---------- estado interno ----------
    private RatSizeProfile profile;
    private EnemyState state = EnemyState.Patrol;
    private EnemyState resumeState;
    private bool locked;

    private Vector3 spawnOrigin;
    private readonly List<PatrolNode> candidates = new();
    private readonly HashSet<PatrolNode> visited = new();
    private readonly List<PatrolNode> pool = new();
    private PatrolNode currentNode, lastNode;
    private float patrolTimer;

    private readonly Collider[] buffer = new Collider[8];
    private Transform target;
    private Vector3 lastKnownPos;
    private bool canSee;
    private float visionTimer, timeSinceSeen;

    private float restTimer, chaseTireTimer;

    // ---------- API publica ----------
    public RatSize Size => size;
    public EnemyState State => state;
    public bool IsMoving => state != EnemyState.Dead && !locked && agent.enabled && agent.isOnNavMesh
                            && !agent.isStopped && agent.velocity.sqrMagnitude > 0.05f;

    public bool Locked
    {
        get => locked;
        set
        {
            locked = value;
            if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            {
                if (value) { agent.isStopped = true; agent.velocity = Vector3.zero; }
                else agent.isStopped = state == EnemyState.Resting || state == EnemyState.Dead;
            }
        }
    }

    private float PatrolSpeed => baseSpeed * profile.speedMultiplier;
    private float ChaseSpeed => PatrolSpeed * chaseSpeedMultiplier;

    // ---------- ciclo de vida ----------
    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (health == null) health = GetComponent<Health>();
        if (attack == null) attack = GetComponent<EnemyAttack>();
        if (eyes == null) eyes = transform;
        profile = size.GetProfile();
    }

    private void OnEnable()  { if (health != null) health.Died += HandleDeath; }
    private void OnDisable() { if (health != null) health.Died -= HandleDeath; }

    private void Start()
    {
        if (autoSetupOnStart) Setup(size);
    }

    // O spawner chama isso (ou o Start, se autoSetupOnStart). Tambem serve para reaproveitar do pool.
    public void Setup(RatSize newSize)
    {
        size = newSize;
        profile = size.GetProfile();
        spawnOrigin = transform.position;

        if (applySizeScale) transform.localScale = profile.scale;

        agent.enabled = true;
        health.SetMaxHealth(Mathf.RoundToInt(baseHealth * profile.healthMultiplier));
        health.ResetState();
        locked = false;

        target = null;
        canSee = false;
        timeSinceSeen = 0f;
        visionTimer = Random.value * visionInterval;   // evita todos escaneando no mesmo frame

        BuildCandidates();
        visited.Clear();
        lastNode = currentNode = null;

        StartPatrol();
    }

    private void Update()
    {
        if (state == EnemyState.Dead || locked || !agent.isOnNavMesh) return;

        visionTimer -= Time.deltaTime;
        if (visionTimer <= 0f)
        {
            visionTimer = visionInterval;
            ScanForPlayer();
        }

        switch (state)
        {
            case EnemyState.Patrol:  TickPatrol(); break;
            case EnemyState.Chase:   TickChase();  break;
            case EnemyState.Resting: TickRest();   break;
        }
    }

    // ---------- PATRULHA ----------
    private void BuildCandidates()
    {
        candidates.Clear();
        foreach (var n in PatrolNode.All)
            if (FlatDistance(n.transform.position, spawnOrigin) <= nodeSearchRadius)
                candidates.Add(n);
    }

    private void StartPatrol()
    {
        state = EnemyState.Patrol;
        agent.isStopped = false;
        agent.speed = PatrolSpeed;
        PickNextNode();
    }

    private void TickPatrol()
    {
        if (currentNode == null) { PickNextNode(); return; }

        patrolTimer += Time.deltaTime;

        // caminho impossivel: ignora esse nodulo
        if (!agent.pathPending && agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            visited.Add(currentNode);
            PickNextNode();
            return;
        }

        if (FlatDistance(transform.position, currentNode.transform.position) <= nodeReachDistance)
        {
            MarkVisitedAround(currentNode);
            lastNode = currentNode;

            if (profile.getsWinded) EnterRest(EnemyState.Patrol);   // recupera o folego antes de seguir
            else PickNextNode();
        }
        else if (patrolTimer >= nodeTimeout)
        {
            visited.Add(currentNode);   // so ele, ja que nao chegou la de verdade
            PickNextNode();
        }
    }

    private void MarkVisitedAround(PatrolNode reached)
    {
        visited.Add(reached);
        Vector3 p = reached.transform.position;
        foreach (var n in candidates)
            if (n != null && FlatDistance(n.transform.position, p) <= nodeReachDistance)
                visited.Add(n);
    }

    private void PickNextNode()
    {
        patrolTimer = 0f;
        agent.isStopped = false;
        agent.speed = PatrolSpeed;

        pool.Clear();
        foreach (var n in candidates)
            if (n != null && !visited.Contains(n)) pool.Add(n);

        if (pool.Count == 0)
        {
            // TODOS visitados: reseta e volta a patrulhar aleatoriamente
            visited.Clear();
            foreach (var n in candidates)
                if (n != null && n != lastNode) pool.Add(n);
            if (pool.Count == 0)
                foreach (var n in candidates) if (n != null) pool.Add(n);
        }

        if (pool.Count == 0) { currentNode = null; return; }   // sem nodulos no alcance: fica parado

        currentNode = pool[Random.Range(0, pool.Count)];
        agent.SetDestination(currentNode.transform.position);
    }

    // ---------- VISAO ----------
    private void ScanForPlayer()
    {
        Transform found = null;
        Vector3 eye = eyes.position;
        int n = Physics.OverlapSphereNonAlloc(eye, viewDistance, buffer, playerMask);

        for (int i = 0; i < n; i++)
        {
            Collider c = buffer[i];
            Transform root = c.attachedRigidbody != null ? c.attachedRigidbody.transform : c.transform;
            if (root.TryGetComponent(out Health h) && h.IsDead) continue;

            Vector3 point = c.bounds.center;
            Vector3 to = point - eye;
            Vector3 flat = new Vector3(to.x, 0f, to.z);

            bool sensed = flat.magnitude <= senseRadius;
            if (!sensed && Vector3.Angle(transform.forward, flat) > viewAngle * 0.5f) continue;
            if (obstacleMask.value != 0 && Physics.Linecast(eye, point, obstacleMask)) continue;

            found = root;
            break;
        }

        canSee = found != null;
        if (!canSee) return;

        target = found;
        lastKnownPos = found.position;

        // Patrulhando (ou descansando da patrulha) e viu o jogador: persegue.
        // Descansando DURANTE a perseguicao nao e interrompido.
        if (state == EnemyState.Patrol || (state == EnemyState.Resting && resumeState == EnemyState.Patrol))
            StartChase();
    }

    // ---------- PERSEGUICAO ----------
    private void StartChase()
    {
        state = EnemyState.Chase;
        agent.isStopped = false;
        agent.speed = ChaseSpeed;
        timeSinceSeen = 0f;
        chaseTireTimer = Random.Range(chaseTireInterval.x, chaseTireInterval.y);
    }

    private void TickChase()
    {
        if (target == null || (target.TryGetComponent(out Health th) && th.IsDead))
        {
            StartPatrol();
            return;
        }

        if (canSee) timeSinceSeen = 0f;
        else timeSinceSeen += Time.deltaTime;

        if (timeSinceSeen > loseSightTime)
        {
            StartPatrol();
            return;
        }

        // folego (so os grandes)
        if (profile.getsWinded)
        {
            chaseTireTimer -= Time.deltaTime;
            if (chaseTireTimer <= 0f) { EnterRest(EnemyState.Chase); return; }
        }

        // ataque
        if (canSee && attack != null && attack.HasWeapon
            && FlatDistance(transform.position, target.position) <= attack.Range)
        {
            agent.isStopped = true;
            FaceTarget(target.position);
            attack.TryAttack();
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(lastKnownPos);
        }
    }

    // ---------- FOLEGO ----------
    private void EnterRest(EnemyState resume)
    {
        resumeState = resume;
        restTimer = Random.Range(restTime.x, restTime.y);
        state = EnemyState.Resting;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }

    private void TickRest()
    {
        restTimer -= Time.deltaTime;
        if (restTimer > 0f) return;

        if (resumeState == EnemyState.Chase) StartChase();
        else StartPatrol();   // escolhe o proximo nodulo so agora
    }

    // ---------- MORTE ----------
    private void HandleDeath()
    {
        state = EnemyState.Dead;
        if (agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }
        agent.enabled = false;
        // Aqui voce pode avisar o spawner / devolver ao pool (ouca health.Died de fora).
    }

    // ---------- util ----------
    private void FaceTarget(Vector3 p)
    {
        Vector3 d = p - transform.position;
        d.y = 0f;
        if (d.sqrMagnitude < 0.001f) return;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(d), turnSpeed * Time.deltaTime);
    }

    private static float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f; b.y = 0f;
        return Vector3.Distance(a, b);
    }

    private void OnDrawGizmosSelected()
    {
        Transform e = eyes != null ? eyes : transform;
        Gizmos.color = Color.yellow;
        Vector3 l = Quaternion.Euler(0f, -viewAngle * 0.5f, 0f) * transform.forward * viewDistance;
        Vector3 r = Quaternion.Euler(0f,  viewAngle * 0.5f, 0f) * transform.forward * viewDistance;
        Gizmos.DrawRay(e.position, l);
        Gizmos.DrawRay(e.position, r);
        Gizmos.DrawWireSphere(e.position, senseRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Application.isPlaying ? spawnOrigin : transform.position, nodeSearchRadius);
    }
}