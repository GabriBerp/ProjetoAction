using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Duas portas com o mesmo doorId levam uma à outra: o jogador que bate em A
// reaparece no arrivalPoint de B (e vice-versa).
// Precisa de um Collider NAO-trigger na porta (a deteccao e por OnCollisionEnter).
public class TransitionDoor : MonoBehaviour
{
    private static readonly Dictionary<string, List<TransitionDoor>> registry = new();
    private static bool busy;

    [Header("Destino")]
    [SerializeField] private string doorId;
    [Tooltip("Onde o jogador aparece quando ESTA porta e o destino. Deixe fora do collider das portas.")]
    [SerializeField] private Transform arrivalPoint;

    [Header("Transição")]
    [SerializeField] private float fadeOutTime = 0.4f;
    [SerializeField] private float holdTime = 0.15f;
    [SerializeField] private float fadeInTime = 0.4f;
    [Tooltip("Tempo extra bloqueando novas transições depois de chegar, pra nao voltar na hora.")]
    [SerializeField] private float cooldown = 0.5f;

    public static bool Busy => busy;

    // Sem domain reload o static sobreviveria entre sessoes de Play.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        registry.Clear();
        busy = false;
    }

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(doorId)) return;
        if (!registry.TryGetValue(doorId, out var list))
            registry[doorId] = list = new List<TransitionDoor>();
        list.Add(this);
    }

    private void OnDisable()
    {
        if (!string.IsNullOrEmpty(doorId) && registry.TryGetValue(doorId, out var list))
            list.Remove(this);
    }

    private void OnCollisionEnter(Collision c)
    {
        if (busy) return;

        var motor = c.collider.GetComponentInParent<CharacterMotor>();
        if (motor == null) return;

        var target = FindPartner();
        if (target == null) return;

        busy = true;
        // Roda no fader (persistente): se esta porta for desativada no meio, a transicao ainda termina e destrava o jogador.
        ScreenFader.Instance.StartCoroutine(Run(motor, target));
    }

    private TransitionDoor FindPartner()
    {
        TransitionDoor partner = null;
        int others = 0;

        if (registry.TryGetValue(doorId, out var list))
        {
            foreach (var door in list)
            {
                if (door == this) continue;
                others++;
                if (partner == null) partner = door;
            }
        }

        if (partner == null)
        {
            Debug.LogWarning($"TransitionDoor '{doorId}': nenhuma outra porta com esse id.", this);
            return null;
        }
        if (others > 1)
            Debug.LogWarning($"TransitionDoor '{doorId}': mais de duas portas com esse id, usando a primeira.", this);
        if (partner.arrivalPoint == null)
        {
            Debug.LogWarning($"TransitionDoor '{doorId}': a porta de destino nao tem arrivalPoint.", partner);
            return null;
        }
        return partner;
    }

    private IEnumerator Run(CharacterMotor motor, TransitionDoor target)
    {
        var rb = motor.GetComponent<Rigidbody>();
        var health = motor.GetComponent<Health>();
        var fader = ScreenFader.Instance;

        bool wasInvulnerable = health != null && health.Invulnerable;
        if (health != null) health.Invulnerable = true;

        yield return KeepLocked(motor, fader.FadeTo(1f, fadeOutTime));
        yield return KeepLocked(motor, Wait(holdTime));

        Teleport(motor, rb, target.arrivalPoint);

        yield return KeepLocked(motor, fader.FadeTo(0f, fadeInTime));

        motor.Locked = false;
        if (health != null) health.Invulnerable = wasInvulnerable;

        yield return new WaitForSeconds(cooldown);
        busy = false;
    }

    private static void Teleport(CharacterMotor motor, Rigidbody rb, Transform point)
    {
        rb.linearVelocity = Vector3.zero;
        motor.transform.position = point.position;
        rb.position = point.position;
        Physics.SyncTransforms();
    }

    // O Health destrava o motor quando um stun em andamento termina; reafirmamos o lock a cada frame.
    private static IEnumerator KeepLocked(CharacterMotor motor, IEnumerator inner)
    {
        while (inner.MoveNext())
        {
            motor.Locked = true;
            yield return inner.Current;
        }
        motor.Locked = true;
    }

    private static IEnumerator Wait(float seconds)
    {
        for (float t = 0f; t < seconds; t += Time.deltaTime)
            yield return null;
    }
}