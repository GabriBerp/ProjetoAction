using System;
using UnityEngine;

public interface IDamageable 
{
    void ReceberDano(int d);
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
public interface ISwingWeapon { }
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