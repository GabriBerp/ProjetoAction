using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Arma corpo-a-corpo. O dono e a mira sao informados por quem segura (PlayerHand/EnemyAttack)
// via SetOwner, entao a arma nao depende da hierarquia. O dano sai no fim do hitDelay, nao no
// clique: e esse intervalo que permite ao alvo reagir com bloqueio ou Perfect Parry.
//
// Durabilidade: cada golpe gasta X (durabilityPerSwing) e cada arremesso que acerta um inimigo
// gasta Y (durabilityPerThrow). Ao chegar em 0 a arma quebra (encolhe e some).
public class SwingWeapon : MonoBehaviour, IUsable, ISwingWeapon, IOwnedItem
{
    // (arma, dono, Time.time do impacto). A IA de bloqueio usa pra mirar a janela de parry.
    public static event Action<SwingWeapon, Health, float> SwingStarted;

    [Header("Dano")]
    [SerializeField] private int damage = 15;
    [SerializeField] private bool hitMultiple = false;
    [Tooltip("Usado quando o dono nao tem IAttackFilter (ex: Player). Inimigos usam o targetMask do EnemyAttack.")]
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Alcance")]
    [Tooltip("Distancia maxima a partir da mira (camera) ou do peito do dono (sem mira).")]
    [SerializeField] private float range = 2f;
    [SerializeField] private float hitRadius = 0.8f;
    [SerializeField] private float ownerOriginHeight = 1f;

    [Header("Tempo")]
    [SerializeField] private float hitDelay = 0.3f;
    [Tooltip("Tempo preso depois do impacto, sem poder bloquear.")]
    [SerializeField] private float recoveryTime = 0.25f;
    [SerializeField] private float cooldown = 0.6f;

    [Header("Durabilidade")]
    [Tooltip("Cargas totais. Ao chegar em 0 a arma quebra.")]
    [SerializeField] private int maxDurability = 12;
    [Tooltip("Durabilidade perdida a cada golpe (X).")]
    [SerializeField] private int durabilityPerSwing = 1;
    [Tooltip("Durabilidade perdida quando a arma arremessada acerta um inimigo (Y).")]
    [SerializeField] private int durabilityPerThrow = 4;
    [Tooltip("Duracao do efeito de quebra antes do objeto sumir.")]
    [SerializeField] private float breakDuration = 0.15f;

    private Health owner;
    private Transform aim;
    private float nextSwingTime;
    private float swingEndTime;
    private int durability;
    private bool broken;
    private readonly HashSet<IDamageable> hitBuffer = new HashSet<IDamageable>();

    public float Range => range;
    public int Damage => damage;
    public bool IsSwinging => Time.time < swingEndTime;
    public float SwingDuration => hitDelay + recoveryTime;

    public int Durability => durability;
    public int MaxDurability => maxDurability;
    public bool Broken => broken;

    // Disparado quando o golpe e aceito (cooldown/stun/bloqueio ja passaram). O visual da arma escuta.
    public event Action<SwingWeapon> Swung;
    // Disparado quando a durabilidade chega a 0. Quem segura a arma usa pra se livrar dela.
    public event Action<SwingWeapon> Broke;

    public bool CanSwing
    {
        get
        {
            if (broken) return false;
            if (Time.time < nextSwingTime) return false;
            if (owner == null || owner.IsDead || owner.IsStunned) return false;

            var defense = owner.GetComponent<Defense>();
            return defense == null || !defense.IsBlocking;
        }
    }

    private void Awake()
    {
        durability = Mathf.Max(0, maxDurability);
    }

    public void SetOwner(Health newOwner, Transform newAim)
    {
        owner = newOwner;
        aim = newAim;
    }

    public void Use()
    {
        if (!CanSwing) return;

        float hitTime = Time.time + hitDelay;
        swingEndTime = hitTime + recoveryTime;
        nextSwingTime = Time.time + Mathf.Max(cooldown, hitDelay + recoveryTime);

        SwingStarted?.Invoke(this, owner, hitTime);
        Swung?.Invoke(this);
        StartCoroutine(SwingRoutine(owner));

        // Gasta X agora, mas so quebra no fim do golpe: o ataque que gastou a ultima carga acontece.
        if (Spend(durabilityPerSwing) && isActiveAndEnabled) StartCoroutine(BreakAfter(swingEndTime));
    }

    // Chamado pelo item arremessado quando ele acerta um alvo. Gasta Y e quebra na hora se zerar.
    public void ApplyThrowWear()
    {
        if (broken) return;
        if (Spend(durabilityPerThrow)) Break();
    }

    private IEnumerator SwingRoutine(Health swinger)
    {
        yield return new WaitForSeconds(hitDelay);

        // Tomar stun (inclusive Perfect Parry), morrer ou soltar a arma durante o wind-up cancela o golpe.
        // swinger != owner cobre o drop e tambem a troca de dono.
        if (swinger == null || swinger.IsDead || swinger.IsStunned || swinger != owner) yield break;

        ResolveHit();
    }

    private void ResolveHit()
    {
        GetAim(out Vector3 origin, out Vector3 direction);

        var filter = owner.GetComponent<IAttackFilter>();
        LayerMask mask = filter != null ? filter.TargetMask : hitMask;

        // Esfera recuada pelo raio: o ponto mais distante do golpe e exatamente "range".
        Vector3 center = origin + direction * Mathf.Max(0f, range - hitRadius);

        IDamageable closest = null;
        float closestDist = float.MaxValue;
        hitBuffer.Clear();

        foreach (var c in Physics.OverlapSphere(center, hitRadius, mask, QueryTriggerInteraction.Ignore))
        {
            var target = c.GetComponentInParent<IDamageable>();
            if (target == null || ReferenceEquals(target, owner) || !hitBuffer.Add(target)) continue;

            if (hitMultiple)
            {
                target.ReceberDano(damage, owner);

                Debug.Log($"{owner} causou {damage}");

                continue;
            }

            float dist = (c.bounds.center - origin).sqrMagnitude;
            if (dist < closestDist) { closestDist = dist; closest = target; }
        }

        if (closest != null) {
            closest.ReceberDano(damage, owner);

            Debug.Log($"{closest} recebeu {damage} por estar perto de {owner}");
        }
    }

    private void GetAim(out Vector3 origin, out Vector3 direction)
    {
        if (aim != null)
        {
            origin = aim.position;
            direction = aim.forward;
            return;
        }

        origin = owner.transform.position + Vector3.up * ownerOriginHeight;
        direction = owner.transform.forward;
    }

    // Retorna true se a durabilidade acabou.
    private bool Spend(int amount)
    {
        if (broken || amount <= 0) return false;

        durability = Mathf.Max(0, durability - amount);
        return durability <= 0;
    }

    private IEnumerator BreakAfter(float time)
    {
        float wait = time - Time.time;
        if (wait > 0f) yield return new WaitForSeconds(wait);
        Break();
    }

    private void Break()
    {
        if (broken) return;
        broken = true;

        Broke?.Invoke(this);

        if (isActiveAndEnabled && Application.isPlaying) StartCoroutine(BreakRoutine());
        else Destroy(gameObject);
    }

    // Feedback simples da quebra: a arma encolhe e some.
    private IEnumerator BreakRoutine()
    {
        Vector3 startScale = transform.localScale;
        float t = 0f;

        while (t < breakDuration)
        {
            t += Time.deltaTime;
            float k = breakDuration <= 0f ? 1f : Mathf.Clamp01(t / breakDuration);
            transform.localScale = startScale * (1f - k);
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (owner == null) return;

        GetAim(out Vector3 origin, out Vector3 direction);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin + direction * Mathf.Max(0f, range - hitRadius), hitRadius);
    }
}
