using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float speed = 40f;
    [SerializeField] private float lifetime = 3f;

    private Rigidbody rb;
    private Action<Bullet> release;
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

    // Chamado toda vez que a bala é "reutilizada"
    public void Fire(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
        released = false;
        timer = lifetime;

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
        damageable?.ReceberDano(damage);

        Release();
    }

    private void Release()
    {
        if (released) return; // evita devolver duas vezes (erro do pool)
        released = true;
        release(this);
    }
}