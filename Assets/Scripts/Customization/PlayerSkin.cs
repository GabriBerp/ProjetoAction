using System.Collections.Generic;
using UnityEngine;

// Veste o jogador com a skin escolhida na SALA_CUSTOMIZAÇÃO.
// Cobre os dois ratos do jogador: o corpo (3a pessoa/morte) e o nariz em primeira pessoa.
// Se adiciona sozinho no Player quando a cena carrega, como o PlayerDeathSequence.
public class PlayerSkin : MonoBehaviour
{
    private readonly List<SkinnedMeshRenderer> targets = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        var look = FindAnyObjectByType<PlayerLook>();
        if (look == null) return;
        if (look.GetComponent<PlayerSkin>() != null) return;

        look.gameObject.AddComponent<PlayerSkin>();
    }

    private void OnEnable()
    {
        RatSkinLibrary.Changed += ApplySelected;
        ApplySelected(RatSkinLibrary.SelectedIndex);
    }

    private void OnDisable() => RatSkinLibrary.Changed -= ApplySelected;

    private void ApplySelected(int _)
    {
        var skin = RatSkinLibrary.SelectedMaterial;
        if (skin == null) return;

        if (targets.Count == 0) Collect();

        foreach (var target in targets)
        {
            if (target == null) continue;

            int slots = Mathf.Max(1, target.sharedMaterials.Length);
            var materials = new Material[slots];
            for (int i = 0; i < slots; i++) materials[i] = skin;
            target.sharedMaterials = materials;
        }
    }

    private void Collect()
    {
        targets.Clear();

        foreach (var renderer in GetComponentsInChildren<SkinnedMeshRenderer>(true))
            Add(renderer);

        // O rato visivel em primeira pessoa mora embaixo da camera, nao do Player.
        var nose = GameObject.Find("PlayerNose");
        if (nose == null) return;

        foreach (var renderer in nose.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            Add(renderer);
    }

    private void Add(SkinnedMeshRenderer renderer)
    {
        if (renderer != null && !targets.Contains(renderer)) targets.Add(renderer);
    }
}
