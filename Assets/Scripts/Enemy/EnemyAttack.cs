using System;
using System.Collections;
using UnityEngine;

// Ataque do inimigo. O modo depende do item na mao:
//   ISwingWeapon -> corpo-a-corpo (usa meleeRange / meleeCooldown)
//   IUsable      -> a distancia  (usa rangedRange / rangedCooldown)
public class EnemyAttack : MonoBehaviour
{
    public enum AttackMode { None, Melee, Ranged }

    [SerializeField] private Transform holdPoint;
    [Tooltip("Item inicial (precisa implementar IPickable). Opcional, tambem da pra chamar Equip().")]
    [SerializeField] private MonoBehaviour startingItem;
    [SerializeField] private Health health;
    [SerializeField] private LayerMask targetMask;

    [Header("Corpo-a-corpo (ISwingWeapon)")]
    [SerializeField] private float meleeRange = 1.8f;
    [SerializeField] private float meleeCooldown = 1.2f;
    [Tooltip("Se a arma ja causa dano sozinha (hitbox propria), desligue.")]
    [SerializeField] private bool dealMeleeDamageFromEnemy = true;
    [SerializeField] private int meleeDamage = 10;
    [SerializeField] private float meleeHitDelay = 0.3f;   // sincronizar com a animacao
    [SerializeField] private float meleeHitRadius = 1f;

    [Header("A distancia (IUsable)")]
    [SerializeField] private float rangedRange = 10f;
    [SerializeField] private float rangedCooldown = 2f;

    private IPickable item;
    private IUsable usable;
    private float nextAttackTime;

    public AttackMode Mode { get; private set; } = AttackMode.None;
    public bool HasWeapon => Mode != AttackMode.None;
    public float Range => Mode == AttackMode.Melee ? meleeRange : Mode == AttackMode.Ranged ? rangedRange : 0f;

    // Mesmos eventos que o PlayerHand, pra reaproveitar as animacoes
    public event Action ItemUsed;
    public event Action ItemSwung;

    private void Awake()
    {
        if (health == null) health = GetComponent<Health>();
    }

    private void OnEnable()  { if (health != null) health.Died += OnDied; }
    private void OnDisable() { if (health != null) health.Died -= OnDied; }

    private void Start()
    {
        if (startingItem is IPickable p) Equip(p);
    }

    public void Equip(IPickable newItem)
    {
        if (item != null) Unequip(Vector3.zero);

        item = newItem;
        usable = newItem as IUsable;
        Mode = newItem is ISwingWeapon ? AttackMode.Melee
             : usable != null          ? AttackMode.Ranged
                                       : AttackMode.None;

        item.PickUp(holdPoint);
        nextAttackTime = Time.time + 0.5f;   // pequeno delay antes do primeiro ataque
    }

    public void Unequip(Vector3 dropForce)
    {
        if (item == null) return;
        item.Drop(dropForce);
        item = null;
        usable = null;
        Mode = AttackMode.None;
    }

    // O EnemyNavmesh chama isso quando o alvo esta no alcance. Retorna true se atacou.
    public bool TryAttack()
    {
        if (!HasWeapon || Time.time < nextAttackTime) return false;

        if (Mode == AttackMode.Melee)
        {
            nextAttackTime = Time.time + meleeCooldown;
            ItemSwung?.Invoke();
            if (dealMeleeDamageFromEnemy) StartCoroutine(MeleeHit());
        }
        else
        {
            nextAttackTime = Time.time + rangedCooldown;
            usable.Use();
            ItemUsed?.Invoke();
        }
        return true;
    }

    private IEnumerator MeleeHit()
    {
        yield return new WaitForSeconds(meleeHitDelay);
        if (!isActiveAndEnabled) yield break;

        Vector3 center = transform.position + Vector3.up + transform.forward * (meleeRange * 0.5f);
        foreach (var c in Physics.OverlapSphere(center, meleeHitRadius, targetMask))
        {
            var dmg = c.GetComponentInParent<IDamageable>();
            if (dmg != null && !ReferenceEquals(dmg, health))
            {
                dmg.ReceberDano(meleeDamage);
                break;
            }
        }
    }

    private void OnDied() => Unequip(transform.forward * 2f + Vector3.up);
}