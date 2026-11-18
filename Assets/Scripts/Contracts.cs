using System;
using UnityEngine;

public interface IDamageable
{
    void ReceberDano(int d);

    // Implementacao padrao: quem nao liga pra origem do dano (caixas, barris) nao precisa mudar nada.
    void ReceberDano(int d, Component attacker) => ReceberDano(d);
}
public interface IPickable
{
    bool IsHeld { get; }
    void PickUp(Transform holdPoint);
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