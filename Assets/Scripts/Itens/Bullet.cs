using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float speed = 40f;
    [SerializeField] private float lifetime = 3f;
    [Tooltip("Raio do barulho do impacto. A IA escuta mesmo sem ver o tiro.")]
    [SerializeField] private float impactNoiseRadius = 12f;

    private Rigidbody rb;
    private Action<Bullet> release;
    private Component shooter;
    private float timer;
    private bool released;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    // Chamado uma vez, quando o pool cria a bala
    public void Init(Action<Bullet> releaseAction) => release = releaseAction;

    // Chamado toda vez que a bala é "reutilizada". O atirador vai junto pra o alvo saber
    // de quem veio o dano (bloqueio/parry) e pra bala não acertar quem atirou.
    public void Fire(Vector3 position, Quaternion rotation, Component attacker = null)
    {
        transform.SetPositionAndRotation(position, rotation);
        released = false;
        timer = lifetime;
        shooter = attacker;

        rb.linearVelocity = transform.forward * speed; // Unity 6. Antes: rb.velocity
        rb.angularVelocity = Vector3.zero;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f) Release();
    }

    private void OnCollisionEnter(Collision collision)
    {
        var damageable = collision.collider.GetComponentInParent<IDamageable>();

        // A bala nasce dentro do proprio atirador e atravessa os ratos do mesmo lado:
        // sem isso o primeiro collider encontrado seria o do proprio atirador.
        if (damageable != null && (IsShooter(damageable) || IsAlly(damageable)))
            return;

        damageable?.ReceberDano(damage, shooter);

        // O impacto tambem faz barulho: outros ratos podem investigar sem ter visto o tiro.
        Noise.Emit(collision.contactCount > 0 ? collision.GetContact(0).point : transform.position,
                   impactNoiseRadius);

        Release();
    }

    private bool IsShooter(IDamageable target)
    {
        if (shooter == null || target is not Component c) return false;

        Transform self = shooter.transform;
        return c.transform == self || c.transform.IsChildOf(self) || self.IsChildOf(c.transform);
    }

    // Inimigos não se acertam: o tiro de um rato atravessa os outros ratos.
    private bool IsAlly(IDamageable target)
    {
        if (shooter == null || target is not Component c) return false;

        return shooter.GetComponentInParent<EnemyNavmesh>() != null
            && c.GetComponentInParent<EnemyNavmesh>() != null;
    }

    private void Release()
    {
        if (released) return; // evita devolver duas vezes (erro do pool)
        released = true;
        shooter = null;
        release(this);
    }
}