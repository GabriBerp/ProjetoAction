using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Balão de fala em world space (texto 3D acima de quem fala).
// Canvas em pixels com escala pequena no mundo pra caber em cima de um rato.
public class DialogueBubble
{
    public GameObject Root;
    public Canvas Canvas;
    public RectTransform Rect;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Body;
    public TextMeshProUGUI Hint;

    private static readonly Color BackColor = new Color(0.03f, 0.035f, 0.05f, 0.78f);

    public static DialogueBubble Create(string objectName, Transform parent, Vector2 pixels, float worldScale)
    {
        var root = new GameObject(objectName, typeof(RectTransform));
        root.transform.SetParent(parent, false);

        var bubble = new DialogueBubble { Root = root };

        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        bubble.Canvas = canvas;

        var rect = canvas.GetComponent<RectTransform>();
        rect.sizeDelta = pixels;
        rect.pivot = new Vector2(0.5f, 0f);
        rect.localScale = Vector3.one * worldScale;
        bubble.Rect = rect;

        var background = new GameObject("Fundo", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(root.transform, false);
        var backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        background.GetComponent<Image>().color = BackColor;
        background.GetComponent<Image>().raycastTarget = false;

        bubble.Name = CreateText(root.transform, "Nome", new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(20f, -52f), new Vector2(-20f, -10f), 34f, TextAlignmentOptions.TopLeft);

        bubble.Body = CreateText(root.transform, "Texto", new Vector2(0f, 0f), new Vector2(1f, 1f),
            new Vector2(20f, 34f), new Vector2(-20f, -58f), 44f, TextAlignmentOptions.TopLeft);

        bubble.Hint = CreateText(root.transform, "Dica", new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(0f, 0f), new Vector2(0f, 0f), 26f, TextAlignmentOptions.BottomRight);
        var hintRect = bubble.Hint.rectTransform;
        hintRect.pivot = new Vector2(1f, 0f);
        hintRect.sizeDelta = new Vector2(190f, 30f);
        hintRect.anchoredPosition = new Vector2(-18f, 8f);

        root.SetActive(false);
        return bubble;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax, float fontSize, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var text = go.AddComponent<TextMeshProUGUI>();
        text.font = DialogueFont.Default;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = false;
        text.richText = true;

        var rect = text.rectTransform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        return text;
    }

    public void PlaceAbove(Vector3 headPosition, Camera view)
    {
        Rect.position = headPosition;
        if (view != null)
            Rect.rotation = Quaternion.LookRotation(Rect.position - view.transform.position, Vector3.up);
        else
            Rect.rotation = Quaternion.identity;
    }

    public void Show(string speakerName, Color speakerColor, string line, float fontSize)
    {
        Root.SetActive(true);
        Name.text = speakerName;
        Name.color = speakerColor;
        Body.text = line;
        Body.fontSize = fontSize;
        Body.maxVisibleCharacters = 0;
        Body.ForceMeshUpdate();
        Hint.text = "";
    }

    public void SetVisibleCharacters(int count) => Body.maxVisibleCharacters = count;

    public int TotalCharacters
    {
        get
        {
            Body.ForceMeshUpdate();
            return Body.textInfo.characterCount;
        }
    }

    public void Hide() => Root.SetActive(false);
}
