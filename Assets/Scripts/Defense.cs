using System;
using UnityEngine;

// Bloqueio com cargas. Serve para Player e Inimigos: quem decide QUANDO bloquear
// (input ou IA) chama TryStartBlock/StopBlock; as regras ficam todas aqui.
public class Defense : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Health health;
    [Tooltip("Define a 'frente' do bloqueio. Player: a camera. Vazio: este objeto.")]
    [SerializeField] private Transform facing;

    [Header("Cargas")]
    [SerializeField] private int maxCharges = 3;
    [Tooltip("Tempo sem bloquear (e sem gastar carga) antes da recarga comecar.")]
    [SerializeField] private float rechargeDelay = 1f;
    [Tooltip("Segundos por carga recuperada.")]
    [SerializeField] private float rechargeTime = 2f;

    [Header("Bloqueio")]
    [Tooltip("Cone frontal que protege. 360 = qualquer direcao.")]
    [SerializeField] private float blockAngle = 140f;
    [Tooltip("Espera apos soltar o bloqueio. Impede parry por spam de clique.")]
    [SerializeField] private float blockCooldown = 0.35f;

    [Header("Parry e quebra de defesa")]
    [Tooltip("Bloqueio iniciado ate X segundos antes do impacto = Perfect Parry.")]
    [SerializeField] private float perfectParryWindow = 0.2f;
    [SerializeField] private float perfectParryStun = 2f;
    [SerializeField] private float guardBreakStun = 2f;

    private int charges;
    private bool isBlocking;
    private float blockStartTime;
    private float nextBlockTime;
    private float rechargeResumeTime;
    private float rechargeTimer;

    public bool IsBlocking => isBlocking;
    public int Charges => charges;
    public int MaxCharges => maxCharges;
    public float PerfectParryWindow => perfectParryWindow;
    public float RechargeProgress => charges >= maxCharges ? 1f : Mathf.Clamp01(rechargeTimer / rechargeTime);

    public event Action<bool> BlockChanged;
    public event Action<int, int> ChargesChanged;   // atual, maximo
    public event Action Blocked;
    public event Action<Health> PerfectParried;     // atacante que tomou o stun
    public event Action GuardBroken;

    private void Awake()
    {
        if (health == null) health = GetComponent<Health>();
        charges = maxCharges;
    }

    private void OnEnable()
    {
        if (health == null) return;
        health.StunChanged += OnStunChanged;
        health.Died += EndBlock;
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.StunChanged -= OnStunChanged;
            health.Died -= EndBlock;
        }
        EndBlock();
    }

    private void Update()
    {
        if (isBlocking || charges >= maxCharges || Time.time < rechargeResumeTime) return;

        rechargeTimer += Time.deltaTime;
        if (rechargeTimer >= rechargeTime)
        {
            rechargeTimer = 0f;
            SetCharges(charges + 1);
        }
    }

    public bool TryStartBlock()
    {
        if (isBlocking) return true;
        if (charges <= 0 || Time.time < nextBlockTime) return false;
        if (health != null && (health.IsStunned || health.IsDead)) return false;

        isBlocking = true;
        blockStartTime = Time.time;
        BlockChanged?.Invoke(true);
        return true;
    }

    public void StopBlock() => EndBlock();

    // Chamado pelo Health antes de aplicar o dano. Qualquer resultado diferente de None
    // significa que o dano foi absorvido.
    public DefenseResult TryDefend(Component attacker)
    {
        if (!isBlocking || charges <= 0 || !IsInFront(attacker.transform.position))
            return DefenseResult.None;

        if (Time.time - blockStartTime <= perfectParryWindow)
        {
            var attackerHealth = attacker.GetComponentInParent<Health>();
            if (attackerHealth != null) attackerHealth.ApplyStun(perfectParryStun);
            PerfectParried?.Invoke(attackerHealth);
            return DefenseResult.PerfectParry;
        }

        // Ultima carga: o golpe e bloqueado, mas a guarda quebra.
        if (charges == 1)
        {
            SetCharges(0);
            ConsumedCharge();
            EndBlock();
            if (health != null) health.ApplyStun(guardBreakStun);
            GuardBroken?.Invoke();
            return DefenseResult.GuardBreak;
        }

        SetCharges(charges - 1);
        ConsumedCharge();
        Blocked?.Invoke();
        return DefenseResult.Blocked;
    }

    // Para reaproveitar o objeto (pool/respawn).
    public void ResetState()
    {
        EndBlock();
        rechargeTimer = 0f;
        nextBlockTime = 0f;
        SetCharges(maxCharges);
    }

    private void ConsumedCharge()
    {
        rechargeTimer = 0f;
        rechargeResumeTime = Time.time + rechargeDelay;
    }

    private void EndBlock()
    {
        if (!isBlocking) return;

        isBlocking = false;
        nextBlockTime = Time.time + blockCooldown;
        rechargeResumeTime = Time.time + rechargeDelay;
        BlockChanged?.Invoke(false);
    }

    private void OnStunChanged(bool stunned)
    {
        if (stunned) EndBlock();
    }

    private void SetCharges(int value)
    {
        charges = Mathf.Clamp(value, 0, maxCharges);
        ChargesChanged?.Invoke(charges, maxCharges);
    }

    // Achata o Y: a camera do player olha pra cima/baixo e isso nao deve abrir ou fechar o cone.
    private bool IsInFront(Vector3 worldPos)
    {
        if (blockAngle >= 360f) return true;

        Transform f = facing != null ? facing : transform;
        Vector3 toAttacker = worldPos - f.position;
        Vector3 forward = f.forward;
        toAttacker.y = 0f;
        forward.y = 0f;

        if (toAttacker.sqrMagnitude < 0.0001f || forward.sqrMagnitude < 0.0001f) return true;
        return Vector3.Angle(forward, toAttacker) <= blockAngle * 0.5f;
    }
}
