using UnityEngine;

public class PlayerAnimationDriver : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterMotor motor;
    [SerializeField] private PlayerHand hand;
    [SerializeField] private Health health;

    private static readonly int IsMoving    = Animator.StringToHash("isMoving");
    private static readonly int IsStunned   = Animator.StringToHash("isStunned");
    private static readonly int Jumped      = Animator.StringToHash("Jumped");
    private static readonly int UsedItem    = Animator.StringToHash("UsedItem");
    private static readonly int SwingAttack = Animator.StringToHash("SwingAttack");
    private static readonly int TossItem    = Animator.StringToHash("TossItem");
    private static readonly int OnDeath     = Animator.StringToHash("OnDeath");

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        motor.Jumped      += OnJumped;
        hand.ItemUsed     += OnUsed;
        hand.ItemSwung    += OnSwung;
        hand.ItemTossed   += OnTossed;
        health.StunChanged += OnStunChanged;
        health.Died       += OnDied;
    }

    private void OnDisable()
    {
        motor.Jumped      -= OnJumped;
        hand.ItemUsed     -= OnUsed;
        hand.ItemSwung    -= OnSwung;
        hand.ItemTossed   -= OnTossed;
        health.StunChanged -= OnStunChanged;
        health.Died       -= OnDied;
    }

    private void Update()
    {
        animator.SetBool(IsMoving, motor.IsMoving);
    }

    private void OnJumped()  => animator.SetTrigger(Jumped);
    private void OnUsed()    => animator.SetTrigger(UsedItem);
    private void OnSwung()   => animator.SetTrigger(SwingAttack);
    private void OnTossed()  => animator.SetTrigger(TossItem);
    private void OnStunChanged(bool stunned) => animator.SetBool(IsStunned, stunned);
    private void OnDied()    => animator.SetTrigger(OnDeath);
}