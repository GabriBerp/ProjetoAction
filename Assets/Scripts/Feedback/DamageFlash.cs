using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Indicativo visual de dano: os renderers do objeto piscam (cor + emissao) quando o Health
// avisa que levou dano. Usa MaterialPropertyBlock, entao nao cria copias de material.
// Pensado para inimigos (e serve para o jogador tambem, se anexado).
public class DamageFlash : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Color flashColor = new Color(1f, 0.18f, 0.12f);
    [SerializeField, Range(0f, 1f)] private float flashStrength = 0.85f;
    [SerializeField] private float flashDuration = 0.18f;
    [SerializeField] private float emissionIntensity = 2.5f;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

    private class Slot
    {
        public Renderer renderer;
        public Color baseColor;
        public bool hasColor;
    }

    private readonly List<Slot> slots = new List<Slot>();
    private MaterialPropertyBlock block;
    private float flashEndTime;
    private bool flashing;

    private void Awake()
    {
        if (health == null) health = GetComponent<Health>();
        block = new MaterialPropertyBlock();
        CacheRenderers();
    }

    private void OnEnable()
    {
        if (health == null) return;
        health.Damaged += OnDamaged;
        health.Died += OnDied;
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.Damaged -= OnDamaged;
            health.Died -= OnDied;
        }
        StopFlash();
    }

    private void CacheRenderers()
    {
        slots.Clear();

        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            if (r == null) continue;

            var material = r.sharedMaterial;
            bool hasColor = material != null && material.HasProperty(BaseColorId);
            Color baseColor = hasColor ? material.GetColor(BaseColorId) : Color.white;

            slots.Add(new Slot { renderer = r, baseColor = baseColor, hasColor = hasColor });
        }
    }

    private void OnDamaged(int amount) => StartFlash();

    private void OnDied()
    {
        if (health != null)
        {
            health.Damaged -= OnDamaged;
            health.Died -= OnDied;
        }
        StopFlash();
    }

    private void StartFlash()
    {
        if (slots.Count == 0) CacheRenderers();
        flashEndTime = Time.time + flashDuration;
        flashing = true;
        Apply(1f);
    }

    private void Update()
    {
        if (!flashing) return;

        float remaining = flashEndTime - Time.time;
        float k = flashDuration <= 0f ? 0f : Mathf.Clamp01(remaining / flashDuration);
        Apply(k);

        if (remaining <= 0f) StopFlash();
    }

    private void Apply(float k)
    {
        float strength = flashStrength * k;

        foreach (var slot in slots)
        {
            if (slot.renderer == null) continue;

            block.Clear();

            if (slot.hasColor)
            {
                Color tinted = Color.Lerp(slot.baseColor, flashColor, strength);
                block.SetColor(BaseColorId, tinted);
                block.SetColor(ColorId, tinted);   // shaders que ainda usam _Color
            }

            if (emissionIntensity > 0f && k > 0f)
                block.SetColor(EmissionId, flashColor * (emissionIntensity * k));

            slot.renderer.SetPropertyBlock(block);
        }
    }

    private void StopFlash()
    {
        if (!flashing && block == null) return;
        flashing = false;

        foreach (var slot in slots)
            if (slot.renderer != null) slot.renderer.SetPropertyBlock(null);
    }
}
