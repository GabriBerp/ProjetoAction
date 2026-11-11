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
}