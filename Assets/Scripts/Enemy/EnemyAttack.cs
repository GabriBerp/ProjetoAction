using System;
using UnityEngine;

// Ataque do inimigo. O modo depende do item na mao:
//   ISwingWeapon -> corpo-a-corpo (usa meleeRange / meleeCooldown; o dano sai da propria arma)
//   IUsable      -> a distancia  (usa rangedRange / rangedCooldown)
public class EnemyAttack : MonoBehaviour, IAttackFilter
{
    public enum AttackMode { None, Melee, Ranged }

    [SerializeField] private Transform holdPoint;
    [Tooltip("Item inicial (precisa implementar IPickable). Opcional, tambem da pra chamar Equip().")]
    [SerializeField] private MonoBehaviour startingItem;
    [SerializeField] private Health health;
    [SerializeField] private Defense defense;
    [Tooltip("Camadas que a arma corpo-a-corpo deste inimigo pode acertar.")]
    [SerializeField] private LayerMask targetMask;

    [Header("Corpo-a-corpo (ISwingWeapon)")]
    [Tooltip("Distancia pra IA decidir atacar. O alcance real do golpe e o da SwingWeapon.")]
    [SerializeField] private float meleeRange = 1.8f;
    [SerializeField] private float meleeCooldown = 1.2f;

    [Header("A distancia (IUsable)")]
    [SerializeField] private float rangedRange = 10f;
    [SerializeField] private float rangedCooldown = 2f;

    private IPickable item;
    private IUsable usable;
    private ISwingWeapon swing;
    private float nextAttackTime;

    public AttackMode Mode { get; private set; } = AttackMode.None;
    public bool HasWeapon => Mode != AttackMode.None;
    public bool IsAttacking => swing != null && swing.IsSwinging;
    public LayerMask TargetMask => targetMask;
    public float Range => Mode == AttackMode.Melee ? meleeRange : Mode == AttackMode.Ranged ? rangedRange : 0f;

    // Mesmos eventos que o PlayerHand, pra reaproveitar as animacoes
    public event Action ItemUsed;
    public event Action ItemSwung;

    private void Awake()
    {
        if (health == null) health = GetComponent<Health>();
        if (defense == null) defense = GetComponent<Defense>();
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

        // IPickable (Pickable) e IUsable/ISwingWeapon (SwingWeapon) sao componentes diferentes
        // do mesmo objeto, entao o cast direto nunca encontra a arma.
        var itemComponent = newItem as Component;
        usable = itemComponent != null ? itemComponent.GetComponent<IUsable>() : null;
        swing = itemComponent != null ? itemComponent.GetComponent<ISwingWeapon>() : null;

        item = newItem;
        Mode = swing != null ? AttackMode.Melee
             : usable != null ? AttackMode.Ranged
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
        swing = null;
        Mode = AttackMode.None;
    }

    // O EnemyNavmesh chama isso quando o alvo esta no alcance. Retorna true se atacou.
    public bool TryAttack()
    {
        if (!HasWeapon || Time.time < nextAttackTime) return false;
        if (health != null && (health.IsStunned || health.IsDead)) return false;
        if (defense != null && defense.IsBlocking) return false;

        if (Mode == AttackMode.Melee)
        {
            if (!swing.CanSwing) return false;

            nextAttackTime = Time.time + meleeCooldown;
            if (usable != null) usable.Use();
            ItemSwung?.Invoke();
        }
        else
        {
            nextAttackTime = Time.time + rangedCooldown;
            usable.Use();
            ItemUsed?.Invoke();
        }
        return true;
    }

    private void OnDied() => Unequip(transform.forward * 2f + Vector3.up);
}
