using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float stunDuration = 0.6f;
    [SerializeField] private CharacterMotor motor;
    [SerializeField] private PlayerHand hand;

    private int health;
    private bool dead;
    private Coroutine stunRoutine;

    public event Action<bool> StunChanged;
    public event Action Died;

    private void Awake() => health = maxHealth;

    public void ReceberDano(int d)
    {
        if (dead) return;
        health -= d;

        if (health <= 0) { Die(); return; }

        if (stunRoutine != null) StopCoroutine(stunRoutine);
        stunRoutine = StartCoroutine(Stun());
    }

    private IEnumerator Stun()
    {
        motor.Locked = true;
        StunChanged?.Invoke(true);
        yield return new WaitForSeconds(stunDuration);
        motor.Locked = false;
        StunChanged?.Invoke(false);
    }

    private void Die()
    {
        dead = true;
        if (stunRoutine != null) StopCoroutine(stunRoutine);
        motor.Locked = true;
        hand.enabled = false;
        Died?.Invoke();
    }
}