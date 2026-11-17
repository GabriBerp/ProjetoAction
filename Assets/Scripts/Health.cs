using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Vida generica: serve para Player, Inimigos, caixas, barris, portas quebraveis, etc.
//
// Tudo que e especifico de personagem e OPCIONAL:
//  - Stun: so acontece se stunDuration > 0 (caixa = 0). ApplyStun() funciona sempre.
//  - ILockable: se nao existir no objeto, simplesmente nao trava nada.
//  - Defense: se nao existir no objeto, nada e bloqueado.
//  - disableOnDeath: lista opcional de componentes pra desligar na morte.
//  - onDamaged / onDied: UnityEvents no Inspector (particula, som, loot, etc, sem escrever codigo).
public class Health : MonoBehaviour, IDamageable, IHealth
{
    public enum DeathAction { Nothing, Deactivate, Destroy }

    [Header("Vida")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private bool invulnerable = false;

    [Header("Stun ao levar dano (0 = sem stun)")]
    [SerializeField] private float stunDuration = 0f;

    [Header("Morte")]
    [SerializeField] private Behaviour[] disableOnDeath;
    [Tooltip("Nothing: personagens (o proprio script cuida) / Deactivate: pool / Destroy: caixas, barris...")]
    [SerializeField] private DeathAction deathAction = DeathAction.Nothing;
    [SerializeField] private float deathActionDelay = 0f;

    [Header("Eventos (Inspector)")]
    public UnityEvent<int> onDamaged;
    public UnityEvent onDied;

    private int health;
    private bool dead;
    private bool stunned;
    private float stunEndTime;
    private Coroutine stunRoutine;
    private ILockable lockable;
    private Defense defense;

    public int Current => health;
    public int Max => maxHealth;
    public bool IsDead => dead;
    public bool IsStunned => stunned;
    public bool Invulnerable { get => invulnerable; set => invulnerable = value; }

    // Eventos de codigo (usados pelos animation drivers, UI, etc.)
    public event Action<bool> StunChanged;
    public event Action Died;
    public event Action<int> Damaged;
    public event Action<int, int> HealthChanged;   // atual, maximo

    private void Awake()
    {
        lockable = GetComponent<ILockable>();   // pode ser null (caixa)
        defense = GetComponent<Defense>();      // pode ser null
        health = maxHealth;
    }

    private void OnDisable()
    {
        if (stunRoutine != null)
        {
            stunRoutine = null;
            stunned = false;
            if (!dead) SetLocked(false);
        }
    }

    public void ReceberDano(int d) => ReceberDano(d, null);

    public void ReceberDano(int d, Component attacker)
    {
        if (dead || invulnerable || d <= 0) return;

        // Sem atacante (queda, armadilha, etc.) nao ha o que bloquear.
        if (defense != null && attacker != null && defense.TryDefend(attacker) != DefenseResult.None)
            return;

        health = Mathf.Max(0, health - d);
        Damaged?.Invoke(d);
        onDamaged?.Invoke(d);
        HealthChanged?.Invoke(health, maxHealth);

        if (health <= 0) { Die(); return; }

        ApplyStun(stunDuration);
    }

    // Se ja existe um stun mais longo em andamento, ele e mantido.
    public void ApplyStun(float duration)
    {
        if (dead || duration <= 0f) return;

        float end = Time.time + duration;
        if (stunned && end <= stunEndTime) return;

        if (stunRoutine != null) StopCoroutine(stunRoutine);
        stunEndTime = end;
        stunRoutine = StartCoroutine(Stun(duration));
    }

    public void Heal(int amount)
    {
        if (dead || amount <= 0) return;
        health = Mathf.Min(maxHealth, health + amount);
        HealthChanged?.Invoke(health, maxHealth);
    }

    // Ex: inimigo aplicando a vida do RatSize.
    public void SetMaxHealth(int value, bool refill = true)
    {
        maxHealth = Mathf.Max(1, value);
        health = refill ? maxHealth : Mathf.Min(health, maxHealth);
        HealthChanged?.Invoke(health, maxHealth);
    }

    // Para reaproveitar o objeto (pool/respawn).
    public void ResetState()
    {
        if (stunRoutine != null) { StopCoroutine(stunRoutine); stunRoutine = null; }
        dead = false;
        stunned = false;
        health = maxHealth;
        SetLocked(false);
        foreach (var b in disableOnDeath) if (b != null) b.enabled = true;
        if (defense != null) defense.ResetState();
        HealthChanged?.Invoke(health, maxHealth);
    }

    private IEnumerator Stun(float duration)
    {
        stunned = true;
        SetLocked(true);
        StunChanged?.Invoke(true);
        yield return new WaitForSeconds(duration);
        stunned = false;
        SetLocked(false);
        StunChanged?.Invoke(false);
        stunRoutine = null;
    }

    private void Die()
    {
        dead = true;
        stunned = false;
        if (stunRoutine != null) { StopCoroutine(stunRoutine); stunRoutine = null; }
        SetLocked(true);

        // Eventos antes de desligar os componentes, pra eles ainda poderem reagir (ex: soltar o item)
        Died?.Invoke();
        onDied?.Invoke();
        foreach (var b in disableOnDeath) if (b != null) b.enabled = false;

        switch (deathAction)
        {
            case DeathAction.Destroy:    Destroy(gameObject, deathActionDelay); break;
            case DeathAction.Deactivate: StartCoroutine(DeactivateAfter(deathActionDelay)); break;
        }
    }

    private IEnumerator DeactivateAfter(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    private void SetLocked(bool value)
    {
        if (lockable != null) lockable.Locked = value;
    }
}
