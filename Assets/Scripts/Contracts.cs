using System;
using UnityEngine;

public interface IDamageable
{
    void ReceberDano(int d);

    // Implementacao padrao: quem nao liga pra origem do dano (caixas, barris) nao precisa mudar nada.
    void ReceberDano(int d, Component attacker) => ReceberDano(d);
}
// Quem esta segurando o item. Cada slot tem a sua posicao local propria no Pickable,
// entao o mesmo item encaixa na boca do jogador e um pouco abaixo no inimigo.
public enum HoldSlot
{
    Player,
    Enemy
}

public interface IPickable
{
    bool IsHeld { get; }
    void PickUp(Transform holdPoint);
    // Padrao: pega como jogador. Quem tem ponto proprio para inimigo (Pickable) sobrescreve.
    void PickUpFor(Transform holdPoint, HoldSlot slot) => PickUp(holdPoint);
    void Drop(Vector3 force);
}
public interface IHoldListener
{
    void OnPickedUp();
    void OnDropped();
}
public interface IUsable
{
    void Use();
}
public interface ISwingWeapon
{
    bool CanSwing => true;
    bool IsSwinging => false;
}
public interface IAttackFilter
{
    LayerMask TargetMask { get; }
}
public interface IOwnedItem
{
    // Quem segura o item chama ao equipar e chama (null, null) ao soltar.
    // aim = de onde sai a mira (player: camera). Null = usa o corpo do dono.
    void SetOwner(Health owner, Transform aim);
}
public interface IReloadable
{
    void Reload();
}
public interface IMoveInput
{
    Vector3 MoveDirection { get; }
    bool Run { get; }
    bool ConsumeJump();
    bool JumpHeld => false;
}
public interface ILockable
{
    bool Locked { get; set; }
}

public interface IRatSized
{
    RatSize Size { get; }
}

public interface IHealth
{
    int Current { get; }
    int Max { get; }
    bool IsDead { get; }
    void Heal(int amount);
}

public enum DefenseResult
{
    None,
    Blocked,
    PerfectParry,
    GuardBreak
}
public interface IItemInfo
{
    string DisplayName { get; }
    ItemCategory Category => ItemCategory.Generic;
    Sprite Icon => null;
}

public interface IDurable
{
    int Durability { get; }
    int MaxDurability { get; }
    event Action DurabilityChanged;
}

public interface IAmmoSource
{
    int Ammo { get; }
    int MagazineSize { get; }
    // Reserva fora do pente. Padrao 0 = a arma nao guarda cartucho sobrando.
    int Reserve => 0;
    event Action AmmoChanged;
}

public interface IDamageSource
{
    // Dano do golpe/tiro. O dano de arremesso continua em Pickable.ThrowDamage.
    int Damage { get; }
}

public interface IUseReporter
{
    event Action<UseResult> UseResolved;
}

public static class ItemLabels
{
    public static string Category(ItemCategory category) => category switch
    {
        ItemCategory.Tool => "Ferramenta",
        ItemCategory.WeaponRanged => "Arma à distância",
        ItemCategory.WeaponMelee => "Corpo a corpo",
        ItemCategory.Consumable => "Consumível",
        ItemCategory.Key => "Chave",
        _ => "Item"
    };
}

// Leitura unica dos dados de um item. Os valores estaticos (nome, dano de arremesso) sao copiados;
// os dinamicos (durabilidade, municao) ficam como referencia para a UI assinar os eventos.
public readonly struct ItemSnapshot
{
    public readonly string Name;
    public readonly ItemCategory Category;
    public readonly Sprite Icon;
    public readonly int ThrowDamage;
    public readonly IDamageSource Damage;
    public readonly IDurable Durability;
    public readonly IAmmoSource Ammo;
    public readonly IUseReporter Reporter;
    public readonly bool Reloadable;

    public bool IsValid => Name != null;

    private ItemSnapshot(Component c)
    {
        c.TryGetComponent(out IItemInfo info);
        c.TryGetComponent(out IDamageSource damage);
        c.TryGetComponent(out IDurable durability);
        c.TryGetComponent(out IAmmoSource ammo);
        c.TryGetComponent(out IUseReporter reporter);

        Name = info != null && !string.IsNullOrEmpty(info.DisplayName)
            ? info.DisplayName
            : c.name.Replace("(Clone)", "").Trim();

        // Sem IItemInfo a categoria e inferida pelo que o item sabe fazer.
        if (info != null) Category = info.Category;
        else if (ammo != null) Category = ItemCategory.WeaponRanged;
        else if (c.TryGetComponent<ISwingWeapon>(out _)) Category = ItemCategory.WeaponMelee;
        else Category = ItemCategory.Generic;

        Icon = info?.Icon;
        ThrowDamage = c.TryGetComponent(out Pickable pickable) ? pickable.ThrowDamage : 0;
        Damage = damage;
        Durability = durability;
        Ammo = ammo;
        Reporter = reporter;
        Reloadable = c.TryGetComponent<IReloadable>(out _);
    }

    public static ItemSnapshot From(IPickable pickable)
    {
        var component = pickable as Component;
        return component == null ? default : new ItemSnapshot(component);
    }
}