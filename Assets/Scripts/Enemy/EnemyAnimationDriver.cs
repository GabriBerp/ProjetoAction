using UnityEngine;

// Usa o MESMO Animator Controller do jogador (mesmos parametros), mas alimentado pelo inimigo.
public class EnemyAnimationDriver : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private EnemyNavmesh enemy;
    [SerializeField] private EnemyAttack attack;
    [SerializeField] private Health health;

    private static readonly int IsMoving    = Animator.StringToHash("isMoving");
    private static readonly int IsStunned   = Animator.StringToHash("isStunned");
    private static readonly int UsedItem    = Animator.StringToHash("UsedItem");
    private static readonly int SwingAttack = Animator.StringToHash("SwingAttack");
    private static readonly int OnDeath     = Animator.StringToHash("OnDeath");

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (enemy == null) enemy = GetComponent<EnemyNavmesh>();
        if (attack == null) attack = GetComponent<EnemyAttack>();
        if (health == null) health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        attack.ItemUsed  += OnUsed;
        attack.ItemSwung += OnSwung;
        health.StunChanged += OnStunChanged;
        health.Died += OnDied;
    }

    private void OnDisable()
    {
        attack.ItemUsed  -= OnUsed;
        attack.ItemSwung -= OnSwung;
        health.StunChanged -= OnStunChanged;
        health.Died -= OnDied;
    }

    private void Update() => animator.SetBool(IsMoving, enemy.IsMoving);

    private void OnUsed() => animator.SetTrigger(UsedItem);
    private void OnSwung() => animator.SetTrigger(SwingAttack);
    private void OnStunChanged(bool stunned) => animator.SetBool(IsStunned, stunned);
    private void OnDied() => animator.SetTrigger(OnDeath);
}