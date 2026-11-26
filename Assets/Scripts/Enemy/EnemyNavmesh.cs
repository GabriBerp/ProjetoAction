using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNavmesh : MonoBehaviour, IRatSized, ILockable
{
    public enum EnemyState { Patrol, Chase, Resting, FetchItem, Search, Retreat, Dead }

    [Header("Referencias")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Health health;
    [SerializeField] private EnemyAttack attack;
    [Tooltip("Posicao dos olhos (origem da visao). Se vazio usa o proprio transform.")]
    [SerializeField] private Transform eyes;

    [Header("Tamanho / Stats base")]
    [SerializeField] private RatSize size = RatSize.Normal;
    [Tooltip("Aplica a escala do RatSize no localScale. Muda no Inspector = muda na hora.")]
    [SerializeField] private bool applySizeScale = true;
    [Tooltip("No editor, a escala acompanha o valor de 'size' sem precisar dar Play.")]
    [SerializeField] private bool previewScaleInEditor = true;
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

    [Header("Busca (memoria do jogador)")]
    [Tooltip("Quanto tempo ele procura em volta do ultimo ponto conhecido antes de voltar a patrulhar.")]
    [SerializeField] private float searchDuration = 6f;
    [Tooltip("Raio dos pontos extras que ele checa em volta do ultimo ponto conhecido.")]
    [SerializeField] private float searchRadius = 6f;
    [Tooltip("Tempo parado olhando em volta em cada ponto da busca.")]
    [SerializeField] private float searchLookTime = 0.8f;

    [Header("Audição (tiro, impacto, passos)")]
    [Tooltip("Multiplicador do raio do som. 0 = surdo.")]
    [SerializeField] private float hearingMultiplier = 1f;

    [Header("Folego (so tamanhos com getsWinded)")]
    [SerializeField] private Vector2 restTime = new Vector2(3f, 5f);
    [Tooltip("De quanto em quanto tempo de perseguicao ele para pra respirar.")]
    [SerializeField] private Vector2 chaseTireInterval = new Vector2(6f, 12f);

    [Header("Recuo (vida baixa / desarmado contra jogador armado)")]
    [Tooltip("Fracao de vida (0..1) a partir da qual ele recua em vez de avancar.")]
    [SerializeField, Range(0f, 1f)] private float lowHealthRetreat = 0.35f;
    [Tooltip("Distancia que ele tenta colocar entre ele e o jogador ao recuar.")]
    [SerializeField] private float retreatDistance = 10f;
    [Tooltip("Tempo maximo fugindo antes de reavaliar.")]
    [SerializeField] private float retreatDuration = 3f;
    [Tooltip("Espera depois de um recuo antes de recuar de novo (evita ir e voltar sem parar).")]
    [SerializeField] private float retreatCooldown = 5f;

    [Header("Subida (rampas / degraus / pulos)")]
    [Tooltip("Liga o assistente de pulo: quando empaca numa rampa ou num vao, ele pula.")]
    [SerializeField] private bool canJump = true;
    [Tooltip("Altura maxima que um pulo consegue vencer.")]
    [SerializeField] private float jumpMaxHeight = 1.3f;
    [Tooltip("Distancia horizontal maxima de um pulo.")]
    [SerializeField] private float jumpMaxDistance = 3f;
    [Tooltip("Tempo empacado (com caminho e sem sair do lugar) antes de tentar pular.")]
    [SerializeField] private float stuckJumpDelay = 0.45f;
    [SerializeField] private float jumpCooldown = 2f;
    [Tooltip("Quanto tempo sem se mexer conta como 'empacado' (amostra de posicao).")]
    [SerializeField] private float stuckSampleInterval = 0.25f;

    [Header("Pegar item do chao quando toma dano")]
    [Tooltip("Chance bem pequena, por dano recebido, de largar tudo e ir buscar um item solto.")]
    [SerializeField, Range(0f, 1f)] private float fetchItemOnDamageChance = 0.05f;
    [SerializeField] private float itemSearchRadius = 15f;
    [SerializeField] private float itemReachDistance = 0.9f;
    [Tooltip("Se nao chegar no item nesse tempo, volta a patrulhar.")]
    [SerializeField] private float itemSeekTimeout = 25f;
    [SerializeField] private LayerMask itemMask = ~0;

    // ---------- estado interno ----------
    private RatSizeProfile profile;
    private EnemyState state = EnemyState.Patrol;
    private EnemyState resumeState;
    private bool locked;

    private Vector3 spawnOrigin;
    private readonly List<PatrolNode> candidates = new();
    private readonly HashSet<PatrolNode> visited = new();
    private readonly List<PatrolNode> pool = new();
    private readonly HashSet<PatrolNode> rejected = new();   // nodulos sem caminho (em cima de moveis)
    private PatrolNode currentNode, lastNode;
    private float patrolTimer;

    private readonly Collider[] buffer = new Collider[8];
    private readonly Collider[] itemBuffer = new Collider[32];
    private Transform target;
    private Vector3 lastKnownPos;
    private bool canSee;
    private float visionTimer, timeSinceSeen;

    private float restTimer, chaseTireTimer;
    private float retreatTimer, nextRetreatTime;
    private Vector3 retreatDestination;

    private float searchTimer, searchLookTimer;
    private Vector3 searchPoint;
    private bool lookingAround;

    private float orbitAngle, orbitRadius = -1f, orbitSign = 1f;   // orbita do flanqueador
    private float chargeTimer, nextChargeTime;                     // investida do brigao/brutamontes

    private Pickable fetchTarget;
    private float fetchTimer;

    private Rigidbody body;
    private bool jumping;
    private float nextJumpTime;
    private Vector3 stuckCheckPos;
    private float stuckCheckTime, stuckTime;

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
        body = GetComponent<Rigidbody>();
        profile = size.GetProfile();
    }

    private void OnEnable()
    {
        Noise.Heard += OnNoise;

        if (health != null)
        {
            health.Died += HandleDeath;
            health.Damaged += HandleDamaged;
        }
    }

    private void OnDisable()
    {
        Noise.Heard -= OnNoise;

        if (health != null)
        {
            health.Died -= HandleDeath;
            health.Damaged -= HandleDamaged;
        }
    }

    // Preview no editor: mexeu no 'size', a escala (e os numeros) mudam na hora.
    private void OnValidate()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        profile = size.GetProfile();

        if (applySizeScale && (!Application.isPlaying || previewScaleInEditor))
            transform.localScale = profile.scale;

        if (Application.isPlaying) ApplySizeStats();
    }

    private void Start()
    {
        if (autoSetupOnStart) Setup(size);
    }

    // Aplica escala/stats do tamanho atual sem reiniciar a IA.
    private void ApplySizeStats()
    {
        profile = size.GetProfile();

        if (applySizeScale) transform.localScale = profile.scale;

        if (!Application.isPlaying) return;

        if (health != null)
            health.SetMaxHealth(Mathf.RoundToInt(baseHealth * profile.healthMultiplier));

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.speed = state == EnemyState.Chase ? ChaseSpeed : PatrolSpeed;
            agent.acceleration = Mathf.Max(4f, agent.speed * 3f);
        }
    }

    // Troca o tamanho em tempo de execucao (spawner/pool).
    public void SetSize(RatSize newSize)
    {
        size = newSize;
        ApplySizeStats();
    }

    // O spawner chama isso (ou o Start, se autoSetupOnStart). Tambem serve para reaproveitar do pool.
    public void Setup(RatSize newSize)
    {
        size = newSize;
        profile = size.GetProfile();
        spawnOrigin = transform.position;

        if (applySizeScale) transform.localScale = profile.scale;

        agent.enabled = true;
        if (health != null)
        {
            health.SetMaxHealth(Mathf.RoundToInt(baseHealth * profile.healthMultiplier));
            health.ResetState();
        }
        locked = false;

        target = null;
        canSee = false;
        timeSinceSeen = 0f;
        visionTimer = Random.value * visionInterval;   // evita todos escaneando no mesmo frame

        fetchTarget = null;
        orbitRadius = -1f;
        orbitSign = Random.value < 0.5f ? -1f : 1f;
        chargeTimer = 0f;
        nextChargeTime = 0f;
        jumping = false;
        stuckTime = 0f;
        stuckCheckTime = 0f;
        stuckCheckPos = transform.position;

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
            case EnemyState.Patrol:    TickPatrol();    break;
            case EnemyState.Chase:     TickChase();     break;
            case EnemyState.Resting:   TickRest();      break;
            case EnemyState.FetchItem: TickFetchItem(); break;
            case EnemyState.Search:    TickSearch();    break;
            case EnemyState.Retreat:   TickRetreat();   break;
        }

        TickJumpAssist();
    }

    // ---------- PATRULHA ----------
    private void BuildCandidates()
    {
        candidates.Clear();
        rejected.Clear();
        foreach (var n in PatrolNode.All)
            if (n != null && FlatDistance(n.transform.position, spawnOrigin) <= nodeSearchRadius)
                candidates.Add(n);
    }

    private void StartPatrol()
    {
        state = EnemyState.Patrol;
        fetchTarget = null;
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

        // caminho so parcial (vao/degrau no meio): tenta pular ate o nodulo antes de desistir
        if (!agent.pathPending && agent.pathStatus == NavMeshPathStatus.PathPartial &&
            TryJumpTowards(currentNode.transform.position))
            return;

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

        FillPool(visited.Contains);

        if (pool.Count == 0)
        {
            // TODOS visitados: reseta e volta a patrulhar aleatoriamente
            visited.Clear();
            rejected.Clear();
            FillPool(n => n == lastNode);
        }

        if (pool.Count == 0) { currentNode = null; return; }   // sem nodulos no alcance: fica parado

        // Nos andaveis em cima de moveis existem no NavMesh mas podem nao ter caminho daqui.
        // Testa alguns aleatorios e descarta os que nao dao caminho completo.
        for (int attempt = 0; attempt < 10 && pool.Count > 0; attempt++)
        {
            int i = Random.Range(0, pool.Count);
            PatrolNode candidate = pool[i];
            pool.RemoveAt(i);
            if (candidate == null) continue;

            var path = new NavMeshPath();
            if (NavMesh.CalculatePath(transform.position, candidate.transform.position, NavMesh.AllAreas, path)
                && path.status == NavMeshPathStatus.PathComplete)
            {
                currentNode = candidate;
                agent.SetPath(path);
                return;
            }

            rejected.Add(candidate);
        }

        // nenhum caminho completo: o assistente de pulo tenta chegar neste
        currentNode = pool.Count > 0 ? pool[Random.Range(0, pool.Count)] : null;
        if (currentNode != null) agent.SetDestination(currentNode.transform.position);
    }

    private void FillPool(System.Func<PatrolNode, bool> skip)
    {
        pool.Clear();
        foreach (var n in candidates)
            if (n != null && !rejected.Contains(n) && !skip(n)) pool.Add(n);
    }

    // ---------- VISAO ----------
    private void ScanForPlayer()
    {
        // Durante um dialogo o jogador nao pode ser percebido: os inimigos simplesmente o ignoram.
        if (DialogueDirector.PlayerBusy)
        {
            canSee = false;
            return;
        }

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
        if (state == EnemyState.Patrol || state == EnemyState.Search ||
            (state == EnemyState.Resting && resumeState == EnemyState.Patrol))
            StartChase();
    }

    // ---------- PERSEGUICAO ----------
    private void StartChase()
    {
        state = EnemyState.Chase;
        fetchTarget = null;
        agent.isStopped = false;
        agent.speed = ChaseSpeed;
        timeSinceSeen = 0f;
        orbitRadius = -1f;   // recalcula o raio da orbita a partir de onde ele esta
        chargeTimer = 0f;
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
            // Antes de patrulhar, procura um pouco em volta de onde viu o jogador por ultimo.
            if (StartSearch(lastKnownPos, true)) return;
            StartPatrol();
            return;
        }

        // folego (so os grandes)
        if (profile.getsWinded)
        {
            chaseTireTimer -= Time.deltaTime;
            if (chaseTireTimer <= 0f) { EnterRest(EnemyState.Chase); return; }
        }

        // Arma de fogo sem pente e sem reserva: nao da mais tiro. Larga e procura outra arma.
        if (canSee && attack != null && attack.OutOfAmmo)
        {
            DropEmptyWeapon();
            return;
        }

        // Vida baixa (ou desarmado) diante de um jogador armado: recua em vez de avancar.
        if (canSee && ShouldRetreat())
        {
            StartRetreat();
            return;
        }

        float dist = FlatDistance(transform.position, target.position);
        float reach = attack != null ? attack.Range : 0f;

        // ataque
        if (canSee && attack != null && attack.HasWeapon && dist <= reach)
        {
            agent.isStopped = true;
            FaceTarget(target.position);
            attack.TryAttack();
            return;
        }

        // Investida (Brigao/Brutamontes): perto, mas fora do golpe = acelera e vai reto.
        if (profile.chargeFrom > 0f && canSee && dist > reach + 0.4f && dist <= profile.chargeFrom
            && Time.time >= nextChargeTime)
        {
            chargeTimer = 0.8f;
            nextChargeTime = Time.time + 5f;   // espera antes de investir de novo
        }

        if (chargeTimer > 0f)
        {
            chargeTimer -= Time.deltaTime;
            agent.speed = ChaseSpeed * profile.chargeSpeed;
        }
        else agent.speed = ChaseSpeed;

        // Orbita (Flanqueador/Assustadico): circula o alvo em vez de ir reto, fechando o raio aos poucos.
        if (profile.circle > 0f && canSee && dist <= profile.circleRadius + 4f && dist > reach + 0.2f)
        {
            Vector3 toMe = transform.position - lastKnownPos;
            toMe.y = 0f;

            if (orbitRadius < 0f)
            {
                orbitRadius = profile.circleRadius;
                orbitAngle = Mathf.Atan2(toMe.x, toMe.z) * Mathf.Rad2Deg;
            }

            orbitAngle += 55f * profile.circle * orbitSign * Time.deltaTime;
            orbitRadius = Mathf.MoveTowards(orbitRadius, Mathf.Max(reach + 0.8f, 1.6f), 1.1f * Time.deltaTime);

            float rad = orbitAngle * Mathf.Deg2Rad;
            Vector3 wanted = lastKnownPos + new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad)) * orbitRadius;

            if (NavMesh.SamplePosition(wanted, out var orbitHit, 2.5f, NavMesh.AllAreas))
            {
                agent.isStopped = false;
                agent.SetDestination(orbitHit.position);
                return;
            }
        }
        else orbitRadius = -1f;   // saiu da faixa: recomeca a orbita de onde estiver

        // alvo fora do navmesh (em cima de um movel, por exemplo): pula pra alcancar
        if (!agent.pathPending && agent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            if (TryJumpTowards(lastKnownPos)) return;
        }

        agent.isStopped = false;
        agent.SetDestination(lastKnownPos);
    }

    // ---------- BUSCA (memoria curta do jogador) ----------
    // Vai ate o ultimo ponto conhecido e confere alguns pontos em volta antes de desistir.
    private bool StartSearch(Vector3 point, bool resetTimer)
    {
        if (!NavMesh.SamplePosition(point, out var hit, 2.5f, NavMesh.AllAreas)) return false;

        searchPoint = hit.position;
        if (resetTimer) searchTimer = searchDuration * profile.searchScale;
        lookingAround = false;
        searchLookTimer = 0f;

        state = EnemyState.Search;
        fetchTarget = null;
        agent.isStopped = false;
        agent.speed = ChaseSpeed;   // investigar tambem e correr
        agent.SetDestination(searchPoint);
        return true;
    }

    private void TickSearch()
    {
        searchTimer -= Time.deltaTime;
        if (searchTimer <= 0f) { StartPatrol(); return; }

        if (agent.pathPending) return;

        bool arrived = agent.pathStatus == NavMeshPathStatus.PathComplete
                       && agent.remainingDistance <= agent.stoppingDistance + 0.4f;

        if (!arrived)
        {
            if (agent.pathStatus == NavMeshPathStatus.PathInvalid) StartPatrol();
            return;
        }

        // Chegou no ponto: para e olha em volta antes de escolher o proximo.
        if (!lookingAround) { lookingAround = true; searchLookTimer = searchLookTime; }

        transform.Rotate(0f, turnSpeed * 0.6f * Time.deltaTime, 0f);

        searchLookTimer -= Time.deltaTime;
        if (searchLookTimer > 0f) return;

        lookingAround = false;

        if (TryPickSearchPoint(out Vector3 next))
        {
            searchPoint = next;
            agent.SetDestination(next);
            return;
        }

        StartPatrol();
    }

    // Ponto extra em volta do ultimo ponto conhecido, no NavMesh e sem repetir o atual.
    private bool TryPickSearchPoint(out Vector3 point)
    {
        point = searchPoint;

        for (int i = 0; i < 6; i++)
        {
            Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(searchRadius * 0.4f, searchRadius);
            Vector3 wanted = searchPoint + new Vector3(offset.x, 0f, offset.y);

            if (!NavMesh.SamplePosition(wanted, out var hit, 1.5f, NavMesh.AllAreas)) continue;
            if ((hit.position - searchPoint).sqrMagnitude < 0.5f) continue;

            point = hit.position;
            return true;
        }

        return false;
    }

    // ---------- AUDICAO (sem visao) ----------
    // Tiro, impacto de bala e passos de corrida avisam onde o jogador esta, mesmo de costas
    // ou atras de parede: o rato vai investigar o ponto em vez de continuar patrulhando.
    private void OnNoise(Vector3 position, float radius)
    {
        if (state == EnemyState.Dead || locked) return;
        if (DialogueDirector.PlayerBusy) return;   // dialogo: surdo ao jogador
        if (hearingMultiplier <= 0f) return;
        if (state == EnemyState.Retreat || state == EnemyState.FetchItem) return;

        // Ja esta com o jogador na mira: barulho nao melhora o que ele ja sabe.
        if (state == EnemyState.Chase && canSee && timeSinceSeen <= 0.5f) return;

        if (FlatDistance(transform.position, position) > radius * hearingMultiplier) return;

        lastKnownPos = position;
        StartSearch(position, true);
    }

    // ---------- RECUO (medo) ----------
    // Recua quando esta com vida baixa ou sem arma diante de um jogador armado. Ao terminar a
    // fuga ele procura arma antes de reengajar, em vez de voltar correndo de peito aberto.
    private bool ShouldRetreat()
    {
        if (target == null || Time.time < nextRetreatTime) return false;
        if (!IsPlayerArmed(target)) return false;

        // Brigao/Brutamontes nao recuam nunca: encaram o jogador ate o fim.
        float threshold = lowHealthRetreat * profile.retreatScale;
        if (threshold <= 0f) return false;

        bool hurt = health != null && health.Max > 0 &&
                    (float)health.Current / health.Max <= threshold;
        bool unarmed = attack == null || !attack.HasWeapon;

        return hurt || unarmed;
    }

    private void StartRetreat()
    {
        if (!TryFindFleePoint(out Vector3 fleePoint))
        {
            // Sem para onde correr: briga onde esta e nao tenta de novo tao cedo.
            nextRetreatTime = Time.time + retreatCooldown;
            return;
        }

        retreatDestination = fleePoint;
        retreatTimer = retreatDuration;
        state = EnemyState.Retreat;
        agent.isStopped = false;
        agent.speed = ChaseSpeed;   // fugir tambem e correr
        agent.SetDestination(retreatDestination);
    }

    // Ponto de fuga no NavMesh, de preferencia no sentido oposto ao jogador (com um leque
    // lateral pra nao bater em parede) e sempre mais longe do que ele esta agora.
    private bool TryFindFleePoint(out Vector3 point)
    {
        point = transform.position;
        if (target == null) return false;

        Vector3 away = transform.position - target.position;
        away.y = 0f;
        if (away.sqrMagnitude < 0.01f) away = -transform.forward;
        away.Normalize();

        float current = (transform.position - target.position).sqrMagnitude;
        var path = new NavMeshPath();

        for (int i = 0; i < 8; i++)
        {
            float angle = i == 0 ? 0f : Random.Range(-75f, 75f);
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * away;
            Vector3 wanted = transform.position + dir * retreatDistance;

            if (!NavMesh.SamplePosition(wanted, out var hit, 3f, NavMesh.AllAreas)) continue;
            if ((hit.position - target.position).sqrMagnitude <= current) continue;   // nao foge pra perto
            if (!NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, path)) continue;
            if (path.status != NavMeshPathStatus.PathComplete) continue;

            point = hit.position;
            return true;
        }

        return false;
    }

    private void TickRetreat()
    {
        retreatTimer -= Time.deltaTime;

        bool arrived = !agent.pathPending && agent.pathStatus == NavMeshPathStatus.PathComplete
                       && agent.remainingDistance <= agent.stoppingDistance + 0.5f;

        if (retreatTimer > 0f && !arrived) return;

        EndRetreat();
    }

    private void EndRetreat()
    {
        nextRetreatTime = Time.time + retreatCooldown;

        // Assustadico: saiu de perto, volta a patrulhar em vez de reengajar.
        if (profile.archetype == RatArchetype.Skittish) { StartPatrol(); return; }

        bool unarmed = attack == null || !attack.HasWeapon;

        // Desarmado: arma primeiro, reengaja depois.
        if (unarmed && TryFindLooseItem(out Pickable found))
        {
            BeginFetchItem(found);
            return;
        }

        // Sem arma por perto ele nao volta a brigar de maos vazias: se afasta da area.
        if (unarmed) StartPatrol();
        else if (canSee && target != null) StartChase();
        else StartPatrol();
    }

    // O alvo esta com algo que machuca na mao (arma de golpe ou arma de fogo/utilizavel)?
    private static bool IsPlayerArmed(Transform player)
    {
        var hand = player.GetComponentInParent<PlayerHand>();
        if (hand == null) hand = player.GetComponentInChildren<PlayerHand>();   // PlayerHand costuma ser filho do corpo

        var held = hand != null ? hand.Held as Component : null;

        return held != null && (held.GetComponent<ISwingWeapon>() != null
                                || held.GetComponent<IUsable>() != null);
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

    // ---------- BUSCAR ITEM NO CHAO (ao tomar dano) ----------
    private void HandleDamaged(int amount)
    {
        if (state == EnemyState.Dead || locked) return;
        if (state == EnemyState.FetchItem) return;
        if (attack == null) return;   // sem mao pra segurar nada
        if (Random.value > fetchItemOnDamageChance) return;

        if (!TryFindLooseItem(out Pickable found)) return;

        BeginFetchItem(found);
    }

    private void BeginFetchItem(Pickable found)
    {
        fetchTarget = found;
        fetchTimer = 0f;
        state = EnemyState.FetchItem;
        agent.isStopped = false;
        agent.speed = ChaseSpeed;   // corre pra pegar
        agent.SetDestination(fetchTarget.transform.position);
    }

    // A arma que ele segura nao tem mais como atirar: larga no chao e, se houver outra
    // arma/utilizavel por perto, entra no FetchItem pra buscar. Sem opcao, segue sem arma.
    private void DropEmptyWeapon()
    {
        if (attack == null || !attack.OutOfAmmo) return;

        attack.Unequip(transform.forward * 1.5f + Vector3.up * 1.5f);

        if (TryFindLooseItem(out Pickable found)) BeginFetchItem(found);
    }

    private bool TryFindLooseItem(out Pickable found)
    {
        found = null;

        int count = Physics.OverlapSphereNonAlloc(transform.position, itemSearchRadius, itemBuffer,
                                                  itemMask, QueryTriggerInteraction.Ignore);

        var path = new NavMeshPath();
        float best = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            var pickable = itemBuffer[i] != null ? itemBuffer[i].GetComponentInParent<Pickable>() : null;
            if (pickable == null || pickable.IsHeld) continue;

            // So arma ou utilizavel: nao larga o porrete pra pegar um ColorCube.
            if (!attack.CanUse(pickable)) continue;

            var rbItem = pickable.GetComponent<Rigidbody>();
            if (rbItem != null && rbItem.isKinematic) continue;   // preso/encaixado, nao esta "solto no chao"

            Vector3 p = pickable.transform.position;
            if (!NavMesh.SamplePosition(p, out var dest, 1.5f, NavMesh.AllAreas)) continue;
            if (!NavMesh.CalculatePath(transform.position, dest.position, NavMesh.AllAreas, path)) continue;
            if (path.status != NavMeshPathStatus.PathComplete) continue;

            float d = (p - transform.position).sqrMagnitude;
            if (d < best) { best = d; found = pickable; }
        }

        return found != null;
    }

    private void TickFetchItem()
    {
        if (attack == null || fetchTarget == null || fetchTarget.IsHeld)
        {
            StartPatrol();
            return;
        }

        fetchTimer += Time.deltaTime;
        if (fetchTimer >= itemSeekTimeout)
        {
            StartPatrol();
            return;
        }

        Vector3 p = fetchTarget.transform.position;
        float flat = FlatDistance(transform.position, p);
        float dy = Mathf.Abs(p.y - transform.position.y);

        if (flat <= itemReachDistance && dy <= 1.2f)
        {
            var pickable = fetchTarget;
            fetchTarget = null;

            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            // Uma coisa na boca por vez: larga o que tem pra pegar o que achou no chao.
            if (attack.HasWeapon) attack.Unequip(transform.forward * 1.5f + Vector3.up * 1.5f);
            attack.Equip(pickable);

            if (canSee && target != null) StartChase();
            else StartPatrol();
            return;
        }

        if (!agent.pathPending && agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            StartPatrol();
            return;
        }

        if (!agent.pathPending && agent.pathStatus == NavMeshPathStatus.PathPartial &&
            TryJumpTowards(p))
            return;

        agent.isStopped = false;
        agent.SetDestination(p);   // o item pode rolar
    }

    // ---------- PULO / SUBIDA ----------
    // Detecta "empacado": tem destino, deveria estar andando e nao sai do lugar.
    private void TickJumpAssist()
    {
        if (!canJump || jumping || state == EnemyState.Dead) return;

        stuckCheckTime += Time.deltaTime;
        if (stuckCheckTime < stuckSampleInterval) return;

        float moved = Vector3.Distance(transform.position, stuckCheckPos);
        bool wantToMove = agent.hasPath && !agent.isStopped && !agent.pathPending;

        if (wantToMove && moved < 0.05f) stuckTime += stuckCheckTime;
        else stuckTime = 0f;

        stuckCheckPos = transform.position;
        stuckCheckTime = 0f;

        if (stuckTime < stuckJumpDelay) return;

        // 1) proximo canto do caminho (rampa quebrada / degrau alto)
        if (agent.path.corners.Length > 1 && TryJumpTowards(agent.path.corners[1])) return;

        // 2) destino final
        if (TryJumpTowards(agent.destination)) return;

        stuckTime = 0f;
    }

    // Tenta um pulo balistico ate um ponto do NavMesh (so pousa em navmesh).
    public bool TryJumpTowards(Vector3 worldTarget)
    {
        if (!canJump || jumping) return false;
        if (Time.time < nextJumpTime) return false;
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return false;
        if (state == EnemyState.Resting || locked) return false;

        if (!NavMesh.SamplePosition(worldTarget, out var landing, 0.9f, NavMesh.AllAreas)) return false;

        Vector3 flat = landing.position - transform.position;
        flat.y = 0f;
        float distance = flat.magnitude;
        float height = landing.position.y - transform.position.y;

        if (distance > jumpMaxDistance) return false;
        if (height > jumpMaxHeight) return false;
        if (distance < 0.25f && Mathf.Abs(height) < 0.2f) return false;

        StartCoroutine(JumpRoutine(landing.position));
        return true;
    }

    private IEnumerator JumpRoutine(Vector3 landing)
    {
        jumping = true;
        nextJumpTime = Time.time + jumpCooldown;

        agent.isStopped = true;
        agent.updatePosition = false;
        agent.updateRotation = false;

        if (body != null) body.isKinematic = true;   // a fisica nao briga com o arco do pulo

        Vector3 start = transform.position;
        Vector3 flat = landing - start;
        flat.y = 0f;
        float distance = flat.magnitude;
        Vector3 dir = distance > 0.01f ? flat / distance : transform.forward;

        // Balistica: escolhe a velocidade pra cair exatamente no ponto de pouso.
        float flight = Mathf.Clamp(0.32f + distance * 0.12f, 0.32f, 0.85f);
        float g = Mathf.Max(1f, Mathf.Abs(Physics.gravity.y));
        float vy = (landing.y - start.y + 0.5f * g * flight * flight) / flight;
        float vh = distance / flight;

        Vector3 pos = start;
        float elapsed = 0f;

        while (elapsed < flight)
        {
            float dt = Time.deltaTime;
            elapsed += dt;
            vy -= g * dt;
            pos += (dir * vh + Vector3.up * vy) * dt;
            transform.position = pos;

            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(dir), 12f * dt);

            yield return null;
        }

        transform.position = landing;

        agent.updatePosition = true;
        agent.updateRotation = true;
        if (agent.isActiveAndEnabled && agent.isOnNavMesh) agent.Warp(landing);

        if (body != null) body.isKinematic = false;

        agent.isStopped = state == EnemyState.Resting || state == EnemyState.Dead;

        jumping = false;
        stuckTime = 0f;
        stuckCheckPos = transform.position;

        ResumeDestination();
    }

    private void ResumeDestination()
    {
        if (state == EnemyState.Dead || locked) return;

        switch (state)
        {
            case EnemyState.Chase:
                if (target != null) agent.SetDestination(lastKnownPos);
                break;
            case EnemyState.FetchItem:
                if (fetchTarget != null) agent.SetDestination(fetchTarget.transform.position);
                break;
            case EnemyState.Patrol:
                if (currentNode != null) agent.SetDestination(currentNode.transform.position);
                break;
            case EnemyState.Retreat:
                agent.SetDestination(retreatDestination);
                break;
            case EnemyState.Search:
                agent.SetDestination(searchPoint);
                break;
        }
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

        Gizmos.color = new Color(1f, 0.55f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, itemSearchRadius);

        Gizmos.color = new Color(0.3f, 1f, 0.4f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, retreatDistance);
    }
}
