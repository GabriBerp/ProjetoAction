using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class Gun : MonoBehaviour, IUsable, IReloadable
{
    [Header("Bala")]
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Transform muzzle; // filho da arma, na ponta do cano (eixo Z pra frente)

    [Header("Munição")]
    [SerializeField] private int clipSize = 12;
    [SerializeField] private int currentAmmo = 12;
    [SerializeField] private int reserveAmmo = 36;

    [Header("Tempo")]
    [SerializeField] private float fireRate = 0.15f;  // segundos entre tiros
    [SerializeField] private float reloadTime = 1.5f;

    [Header("Pool")]
    [SerializeField] private int poolDefaultCapacity = 20;
    [SerializeField] private int poolMaxSize = 50;

    private ObjectPool<Bullet> pool;
    private float nextFireTime;
    private bool isReloading;
    private float reloadStartedAt;

    // Útil pra UI
    public int CurrentAmmo => currentAmmo;
    public int ReserveAmmo => reserveAmmo;
    public int ClipSize => clipSize;
    public bool IsUsable => currentAmmo > 0 || reserveAmmo > 0;
    public bool IsReloading => isReloading;
    public float ReloadProgress => isReloading && reloadTime > 0f
        ? Mathf.Clamp01((Time.time - reloadStartedAt) / reloadTime)
        : 0f;

    private void Awake()
    {
        pool = new ObjectPool<Bullet>(
            createFunc: CreateBullet,
            actionOnGet: b => b.gameObject.SetActive(true),
            actionOnRelease: b => b.gameObject.SetActive(false),
            actionOnDestroy: b => Destroy(b.gameObject),
            collectionCheck: true,
            defaultCapacity: poolDefaultCapacity,
            maxSize: poolMaxSize
        );
    }

    private Bullet CreateBullet()
    {
        // Sem parent: a bala não pode ser filha da arma, senão acompanharia o jogador
        Bullet bullet = Instantiate(bulletPrefab);
        bullet.Init(b => pool.Release(b));
        return bullet;
    }

    public void Use()
    {
        if (isReloading) return;
        if (Time.time < nextFireTime) return;
        if (currentAmmo <= 0) return; // sem bala na arma (precisa recarregar)

        nextFireTime = Time.time + fireRate;
        currentAmmo--;

        Bullet bullet = pool.Get();
        bullet.Fire(muzzle.position, muzzle.rotation);
    }

    public void Reload()
    {
        if (isReloading) return;
        if (currentAmmo >= clipSize) return; // já está cheia
        if (reserveAmmo <= 0) return;        // sem cartucho, não tem o que recarregar

        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        reloadStartedAt = Time.time;
        yield return new WaitForSeconds(reloadTime);

        int needed = clipSize - currentAmmo;
        int taken = Mathf.Min(needed, reserveAmmo);

        currentAmmo += taken;
        reserveAmmo -= taken;

        isReloading = false;
    }

    private void OnDestroy() => pool?.Dispose();
}