using System.Collections;
using UnityEngine;

// Reage ao inicio de um golpe de outro personagem: decide se bloqueia e QUANDO aperta o bloqueio.
// perfectChance controla a chance de acertar a janela de Perfect Parry; fora dela e bloqueio normal.
public class EnemyBlockAI : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Defense defense;
    [SerializeField] private EnemyAttack attack;

    [Range(0f, 1f)] [SerializeField] private float blockChance = 0.5f;
    [Range(0f, 1f)] [SerializeField] private float perfectChance = 0.2f;
    [Tooltip("Folga somada ao alcance da arma do atacante pra decidir se vale reagir.")]
    [SerializeField] private float reactionMargin = 1f;
    [Tooltip("Quanto tempo segura o bloqueio depois do impacto.")]
    [SerializeField] private float holdAfterImpact = 0.3f;

    private Coroutine routine;

    private void Awake()
    {
        if (health == null) health = GetComponent<Health>();
        if (defense == null) defense = GetComponent<Defense>();
        if (attack == null) attack = GetComponent<EnemyAttack>();
    }

    private void OnEnable() => SwingWeapon.SwingStarted += OnSwingStarted;

    private void OnDisable()
    {
        SwingWeapon.SwingStarted -= OnSwingStarted;
        if (routine != null) { StopCoroutine(routine); routine = null; }
        if (defense != null) defense.StopBlock();
    }

    private void OnSwingStarted(SwingWeapon weapon, Health attacker, float impactTime)
    {
        if (routine != null || attacker == null || attacker == health) return;
        if (health.IsDead || health.IsStunned || defense == null) return;
        if (attack != null && attack.Mode != EnemyAttack.AttackMode.Melee) return;

        // Inimigos nao reagem a golpes de outros inimigos.
        if (attacker.GetComponent<EnemyAttack>() != null) return;

        float reach = weapon.Range + reactionMargin;
        if ((attacker.transform.position - transform.position).sqrMagnitude > reach * reach) return;
        if (Random.value > blockChance) return;

        routine = StartCoroutine(BlockRoutine(impactTime));
    }

    private IEnumerator BlockRoutine(float impactTime)
    {
        float window = defense.PerfectParryWindow;
        float lead = Random.value < perfectChance
            ? Random.Range(0.02f, window * 0.8f)
            : Random.Range(window * 1.5f, window * 3f);

        float wait = impactTime - lead - Time.time;
        if (wait > 0f) yield return new WaitForSeconds(wait);

        if (!health.IsStunned && (attack == null || !attack.IsAttacking) && defense.TryStartBlock())
        {
            yield return new WaitForSeconds(Mathf.Max(0f, impactTime - Time.time) + holdAfterImpact);
            defense.StopBlock();
        }

        routine = null;
    }
}
