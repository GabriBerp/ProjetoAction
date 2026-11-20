using UnityEngine;

// Dano de arremesso: qualquer item seguravel arremessado contra um alvo causa Z de dano
// (Pickable.throwDamage, minimo 0). Se o item for uma arma corpo-a-corpo, o impacto tambem
// gasta Y de durabilidade (SwingWeapon.ApplyThrowWear) e pode quebra-la.
//
// O componente e ligado pelo PlayerHand.Throw() e desliga sozinho depois do primeiro acerto.
public class ThrownItem : MonoBehaviour
{
    [Tooltip("Tempo minimo apos o arremesso antes de poder acertar (evita acertar quem jogou).")]
    [SerializeField] private float armDelay = 0.06f;
    [Tooltip("Abaixo dessa velocidade o item nao conta como arremessado (so caindo).")]
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float sweepRadius = 0.08f;
    [SerializeField] private float maxLifetime = 6f;

    private int damage;
    private Component thrower;
    private Rigidbody body;
    private SwingWeapon weapon;
    private bool spent;
    private float armTime;
    private float expireTime;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        weapon = GetComponent<SwingWeapon>();
    }

    // Chamado depois do Drop: o item ja esta solto, com a forca aplicada.
    public void Launch(int throwDamage, Component throwerComponent)
    {
        damage = Mathf.Max(0, throwDamage);   // Z nunca negativo
        thrower = throwerComponent;
        if (body == null) body = GetComponent<Rigidbody>();
        if (weapon == null) weapon = GetComponent<SwingWeapon>();

        spent = false;
        armTime = Time.time + armDelay;
        expireTime = Time.time + maxLifetime;
        enabled = true;
    }

    private void Update()
    {
        if (spent || Time.time < armTime) return;

        if (Time.time > expireTime) { spent = true; enabled = false; return; }
        if (body == null) return;

        Vector3 velocity = body.linearVelocity;
        float speed = velocity.magnitude;
        if (speed < minSpeed) return;

        // Varre o caminho do frame pra nao atravessar alvos em arremessos rapidos.
        Vector3 origin = transform.position;
        Vector3 direction = velocity / speed;
        float distance = speed * Time.deltaTime;

        var hits = Physics.SphereCastAll(origin, sweepRadius, direction, distance, ~0, QueryTriggerInteraction.Ignore);
        if (hits.Length == 0) return;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (var hit in hits)
        {
            if (hit.collider == null || hit.collider.transform.IsChildOf(transform)) continue;
            if (TryHit(hit.collider)) return;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.contactCount == 0) return;
        TryHit(collision.collider);
    }

    private bool TryHit(Collider other)
    {
        if (spent || other == null) return false;

        var target = other.GetComponentInParent<IDamageable>();
        if (target == null) return false;

        // Nao acerta quem arremessou (o item pode encostar no dono ao sair da mao).
        if (thrower != null && other.transform.IsChildOf(thrower.transform)) return false;

        spent = true;
        enabled = false;

        if (damage > 0) target.ReceberDano(damage, thrower);
        if (weapon != null) weapon.ApplyThrowWear();

        return true;
    }
}
