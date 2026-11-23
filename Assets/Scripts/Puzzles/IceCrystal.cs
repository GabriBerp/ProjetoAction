using UnityEngine;

public class IceCrystal : MonoBehaviour
{
    [SerializeField] private string cubeTag = "IceCube";
    [SerializeField] private GameObject breakEffectPrefab; // partículas
    [SerializeField] private float minImpactSpeed = 2f;    // evita quebrar com encostadinha

    private IcePuzzle puzzle;
    private bool broken;

    public void Init(IcePuzzle owner) => puzzle = owner;

    private void OnCollisionEnter(Collision collision)
    {
        if (broken || !collision.collider.CompareTag(cubeTag)) return;
        if (collision.relativeVelocity.magnitude < minImpactSpeed) return;

        broken = true;

        if (breakEffectPrefab != null)
            Instantiate(breakEffectPrefab, transform.position, Quaternion.identity);

        puzzle?.NotifyCrystalBroken(this);
        Destroy(gameObject);
    }
}