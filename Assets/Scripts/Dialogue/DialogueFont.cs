using TMPro;
using UnityEngine;

// Fonte padrão e helpers de texto 3D usados pelos marcadores e pelo balão.
public static class DialogueFont
{
    private static TMP_FontAsset cached;

    public static TMP_FontAsset Default
    {
        get
        {
            if (cached != null) return cached;

            cached = TMP_Settings.defaultFontAsset;
            if (cached == null)
                cached = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

            return cached;
        }
    }

    // Texto 3D simples (mesh), usado no "!". A escala é calculada para a altura desejada no mundo.
    public static TextMeshPro CreateWorldText(string name, Transform parent, string content, Color color,
        float fontSize, float worldHeight)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var text = go.AddComponent<TextMeshPro>();
        text.font = Default;
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;

        text.rectTransform.sizeDelta = new Vector2(4f, 2f);
        text.rectTransform.pivot = new Vector2(0.5f, 0f);

        go.transform.localScale = Vector3.one;
        text.ForceMeshUpdate();

        float natural = text.bounds.size.y;
        float scale = natural > 0.0001f ? worldHeight / natural : 0.02f;
        go.transform.localScale = Vector3.one * scale;

        return text;
    }
}
