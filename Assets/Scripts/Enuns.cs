using System;
using UnityEngine;

public enum RatSize
{
  Mini,
  Agile,
  Normal,
  Strong,
  Big
}

public enum Effects
{
  Tetanus,
  Poisoned,
  RatFever,
  Stunned,
  Honeyed,
  Agitated,
  Satiated,
  Armored
}

// Resultado de tentativa de uso de item (feedback de sucesso/falha).
public enum UseResult
{
  Success,
  Empty,
  Cooldown,
  Blocked,
  NoTarget,
  Failed
}

// Categoria simples para metadados/prompt de itens.
public enum ItemCategory
{
  Generic,
  Tool,
  WeaponRanged,
  WeaponMelee,
  Consumable,
  Key
}

// Tipo de acao para feedback world-space.
public enum FeedbackCue
{
  Pickup,
  Store,
  Retrieve,
  Drop,
  Use,
  UseFailed,
  Reload,
  ReloadComplete,
  ToggleOn,
  ToggleOff,
  Empty,
  Damaged,
  LowHealth
}
