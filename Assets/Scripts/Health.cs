using UnityEngine;
using System;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] int max = 100;
    int atual;
    public event Action OnMorreu;

    void Awake() => atual = max;

    public void ReceberDano(int d)
    {
        atual -= d;
        if (atual <= 0) OnMorreu?.Invoke();
    }
}
