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
    private IOwnedItem owned;
    private SwingWeapon swingWeapon;
    private IReloadable reloadable;
    private IAmmoSource ammo;
    private float nextAttackTime;

    public AttackMode Mode { get; private set; } = AttackMode.None;
    public bool HasWeapon => Mode != AttackMode.None;
    public bool IsAttacking => swing != null && swing.IsSwinging;
    public LayerMask TargetMask => targetMask;
    public IPickable CurrentItem => item;
    public Transform HoldPoint => holdPoint;
    public float Range => Mode == AttackMode.Melee ? meleeRange : Mode == AttackMode.Ranged ? rangedRange : 0f;

    // Tem cartucho na reserva pra encher o pente?
    public bool CanReload => Mode == AttackMode.Ranged && reloadable != null && ammo != null
                             && ammo.Ammo < ammo.MagazineSize && ammo.Reserve > 0;

    // Arma de fogo sem pente e sem reserva: nao tem mais como atirar com ela.
    public bool OutOfAmmo => Mode == AttackMode.Ranged && ammo != null
                             && ammo.Ammo <= 0 && ammo.Reserve <= 0;

    // Mesmos eventos que o PlayerHand, pra reaproveitar as animacoes
    public event Action ItemUsed;
    public event Action ItemSwung;

    private void Awake()
    {
        if (health == null) health = GetComponent<Health>();
        if (defense == null) defense = GetComponent<Defense>();
        if (holdPoint == null) holdPoint = FindHoldPoint();
    }

    private void OnEnable()  { if (health != null) health.Died += OnDied; }
    private void OnDisable() { if (health != null) health.Died -= OnDied; }

    private void Start()
    {
        if (startingItem is IPickable p) Equip(p);
    }

    // O ponto de pega do inimigo e um filho chamado HoldPoint. Sem ele o item ficaria
    // pendurado na raiz (nao escala com o rato) e a pose do slot nao apareceria.
    private Transform FindHoldPoint()
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t == transform) continue;
            if (t.name.Equals("HoldPoint", StringComparison.OrdinalIgnoreCase) ||
                t.name.Equals("Mouth", StringComparison.OrdinalIgnoreCase))
                return t;
        }

        return null;
    }

    // O item serve pra ele? Arma (ISwingWeapon) ou utilizavel (IUsable). Um enfeite no chao
    // (ColorCube, chave...) nao vale trocar a arma que ele ja tem.
    public bool CanUse(IPickable candidate)
    {
        var c = candidate as Component;
        if (c == null) return false;
        if (c.GetComponent<ISwingWeapon>() != null) return true;
        if (c.GetComponent<IUsable>() == null) return false;

        // Arma de fogo vazia (nem pente, nem reserva) nao vale o desvio.
        return !(c.TryGetComponent(out IAmmoSource ammoSource)
                 && ammoSource.Ammo <= 0 && ammoSource.Reserve <= 0);
    }

    public void Equip(IPickable newItem)
    {
        if (newItem == null) return;
        if (item != null) Unequip(Vector3.zero);

        if (holdPoint == null) holdPoint = FindHoldPoint();
        if (holdPoint == null)
        {
            Debug.LogWarning($"{name}: sem HoldPoint, o item vai ficar solto.", this);
            return;
        }

        // IPickable (Pickable) e IUsable/ISwingWeapon (SwingWeapon) sao componentes diferentes
        // do mesmo objeto, entao o cast direto nunca encontra a arma.
        var itemComponent = newItem as Component;
        usable = itemComponent != null ? itemComponent.GetComponent<IUsable>() : null;
        swing = itemComponent != null ? itemComponent.GetComponent<ISwingWeapon>() : null;
        owned = itemComponent != null ? itemComponent.GetComponent<IOwnedItem>() : null;
        swingWeapon = itemComponent != null ? itemComponent.GetComponent<SwingWeapon>() : null;
        reloadable = itemComponent != null ? itemComponent.GetComponent<IReloadable>() : null;
        ammo = itemComponent != null ? itemComponent.GetComponent<IAmmoSource>() : null;
        if (swingWeapon != null) swingWeapon.Broke += OnWeaponBroke;

        item = newItem;
        Mode = swing != null ? AttackMode.Melee
             : usable != null ? AttackMode.Ranged
                              : AttackMode.None;

        // Slot do inimigo: posicao propria do item (um pouco abaixo da do jogador) e
        // escala de mundo preservada, mesmo que este rato esteja grande.
        item.PickUpFor(holdPoint, HoldSlot.Enemy);
        if (owned != null) owned.SetOwner(health, null);
        nextAttackTime = Time.time + 0.5f;   // pequeno delay antes do primeiro ataque
    }

    public void Unequip(Vector3 dropForce)
    {
        if (swingWeapon != null) { swingWeapon.Broke -= OnWeaponBroke; swingWeapon = null; }
        if (item == null) return;
        if (owned != null) owned.SetOwner(null, null);
        item.Drop(dropForce);
        item = null;
        usable = null;
        swing = null;
        owned = null;
        reloadable = null;
        ammo = null;
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
            // Pente vazio: recarrega em vez de "atirar" no vazio (a arma recusa o disparo).
            if (ammo != null && ammo.Ammo <= 0)
            {
                if (CanReload) Reload();
                return false;
            }

            nextAttackTime = Time.time + rangedCooldown;
            usable.Use();
            ItemUsed?.Invoke();
        }
        return true;
    }

    // A IA usa quando o pente esvazia. A arma ignora pedidos repetidos durante a recarga.
    public void Reload() => reloadable?.Reload();

    // A arma quebrou na mao (durabilidade zerada): fica sem arma, sem tentar soltar o que sumiu.
    private void OnWeaponBroke(SwingWeapon weapon)
    {
        if (weapon != swingWeapon) return;

        swingWeapon.Broke -= OnWeaponBroke;
        swingWeapon = null;
        item = null;
        usable = null;
        swing = null;
        owned = null;
        reloadable = null;
        ammo = null;
        Mode = AttackMode.None;
    }

    private void OnDied() => Unequip(transform.forward * 2f + Vector3.up);
}
