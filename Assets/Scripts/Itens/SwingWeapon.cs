using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Arma corpo-a-corpo. O dono e a mira sao informados por quem segura (PlayerHand/EnemyAttack)
// via SetOwner, entao a arma nao depende da hierarquia. O dano sai no fim do hitDelay, nao no
// clique: e esse intervalo que permite ao alvo reagir com bloqueio ou Perfect Parry.
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

    private Health owner;
    private Transform aim;
    private float nextSwingTime;
    private float swingEndTime;
    private readonly HashSet<IDamageable> hitBuffer = new HashSet<IDamageable>();

    public float Range => range;
    public int Damage => damage;
    public bool IsSwinging => Time.time < swingEndTime;

    public bool CanSwing
    {
        get
        {
            if (Time.time < nextSwingTime) return false;
            if (owner == null || owner.IsDead || owner.IsStunned) return false;

            var defense = owner.GetComponent<Defense>();
            return defense == null || !defense.IsBlocking;
        }
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
        StartCoroutine(SwingRoutine(owner));
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
                continue;
            }

            float dist = (c.bounds.center - origin).sqrMagnitude;
            if (dist < closestDist) { closestDist = dist; closest = target; }
        }

        if (closest != null) closest.ReceberDano(damage, owner);
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

    private void OnDrawGizmosSelected()
    {
        if (owner == null) return;

        GetAim(out Vector3 origin, out Vector3 direction);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin + direction * Mathf.Max(0f, range - hitRadius), hitRadius);
    }
}