using System;
using UnityEngine;

// Catalogo da skin de rato equipada. Fica em static para a escolha sobreviver a morte:
// o PlayerDeathSequence so respawna o jogador, nao recarrega a cena.
public static class RatSkinLibrary
{
    // Disparado quando o jogador escolhe outra skin. As estacoes e o corpo do jogador escutam.
    public static event Action<int> Changed;

    public static int SelectedIndex { get; private set; } = -1;
    public static Material SelectedMaterial { get; private set; }

    public static void Select(int index, Material material)
    {
        if (material == null) return;

        SelectedIndex = index;
        SelectedMaterial = material;
        Changed?.Invoke(index);
    }

    // Material que o jogador ja esta vestindo (skin padrao da cena).
    public static Material PlayerBodyMaterial()
    {
        var look = UnityEngine.Object.FindAnyObjectByType<PlayerLook>();
        if (look == null) return null;

        var renderer = look.GetComponentInChildren<SkinnedMeshRenderer>();
        return renderer != null ? renderer.sharedMaterial : null;
    }

    // Sem domain reload o static sobreviveria entre sessoes de Play.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSession()
    {
        SelectedIndex = -1;
        SelectedMaterial = null;
        Changed = null;
    }
}
