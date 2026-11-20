using UnityEngine;

// Feedback visual do golpe: a arma segura varre de um lado pro outro (e volta) sempre que
// o SwingWeapon aceita um ataque. Fica no proprio item, entao serve para Player e inimigo.
// A rotacao e aplicada em cima da rotacao de descanso definida pelo Pickable ao equipar.
[RequireComponent(typeof(SwingWeapon))]
public class WeaponSwingVisual : MonoBehaviour
{
    [Tooltip("Amplitude em graus da varredura (x = inclinacao, y = lado a lado, z = giro).")]
    [SerializeField] private Vector3 sweepEuler = new Vector3(10f, 60f, 18f);
    [Tooltip("Fracao do golpe usada na ida; o resto e a volta pro descanso.")]
    [SerializeField, Range(0.2f, 0.9f)] private float sweepOutFraction = 0.55f;
    [Tooltip("Quando o golpe termina e a arma continua animada, tempo extra pra voltar.")]
    [SerializeField] private float settleTime = 0.12f;
    [Tooltip("Tempo total da animacao. 0 = usa o SwingWeapon.SwingDuration.")]
    [SerializeField] private float durationOverride = 0f;

    private SwingWeapon weapon;
    private Pickable pickable;
    private Quaternion restRotation;
    private bool swinging;
    private float startTime;
    private float duration;

    private void Awake()
    {
        weapon = GetComponent<SwingWeapon>();
        pickable = GetComponent<Pickable>();
        restRotation = transform.localRotation;
    }

    private void OnEnable()
    {
        if (weapon != null) weapon.Swung += OnSwung;
        restRotation = transform.localRotation;
        swinging = false;
    }

    private void OnDisable()
    {
        if (weapon != null) weapon.Swung -= OnSwung;
        if (swinging) transform.localRotation = restRotation;
        swinging = false;
    }

    private void OnSwung(SwingWeapon w)
    {
        // Arremessado/solto ou quebrado: nao ha golpe pra animar.
        if (pickable != null && !pickable.IsHeld) return;

        restRotation = transform.localRotation;
        duration = durationOverride > 0f ? durationOverride : weapon.SwingDuration;
        startTime = Time.time;
        swinging = true;
    }

    private void Update()
    {
        if (!swinging) return;

        float total = Mathf.Max(0.05f, duration + settleTime);
        float t = Mathf.Clamp01((Time.time - startTime) / total);

        float angle = Sweep(t);
        transform.localRotation = Quaternion.Euler(sweepEuler * angle) * restRotation;

        if (t >= 1f)
        {
            transform.localRotation = restRotation;
            swinging = false;
        }
    }

    // -1 -> 0 -> 1 na ida, 1 -> 0 na volta pro descanso.
    private float Sweep(float t)
    {
        if (t <= sweepOutFraction)
            return Mathf.SmoothStep(-1f, 1f, t / sweepOutFraction);

        float back = sweepOutFraction >= 1f ? 1f : (t - sweepOutFraction) / (1f - sweepOutFraction);
        return Mathf.Lerp(1f, 0f, Mathf.SmoothStep(0f, 1f, back));
    }
}
