using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Textos flutuantes no mundo (numeros de dano etc.), em pool, criados por codigo:
// nao precisa de prefab. Usa TextMeshPro 3D, sem Canvas.
public class FeedbackPopups : MonoBehaviour
{
    [SerializeField] private int maxActive = 24;
    [SerializeField] private float lifetime = 0.9f;
    [SerializeField] private float riseSpeed = 0.8f;
    [SerializeField] private float fontSize = 3f;
    [SerializeField] private float horizontalJitter = 0.15f;
    [SerializeField] private float referenceDistance = 3f;
    [SerializeField] private Vector2 scaleRange = new Vector2(0.7f, 3f);
    [Tooltip("Vazio = fonte padrao do TMP.")]
    [SerializeField] private TMP_FontAsset font;

    private class Popup
    {
        public TextMeshPro Text;
        public Vector3 Origin;
        public float Age;
        public Color Color;
    }

    private readonly List<Popup> active = new List<Popup>();
    private readonly Stack<Popup> free = new Stack<Popup>();

    private void OnEnable() => FeedbackBus.Raised += OnCue;
    private void OnDisable() => FeedbackBus.Raised -= OnCue;

    private void OnCue(FeedbackCue cue, Vector3 position, int value)
    {
        if (!FeedbackBus.TryDescribe(cue, value, out string text, out Color color)) return;

        var cam = Camera.main;
        Vector3 jitter = cam != null ? cam.transform.right * Random.Range(-horizontalJitter, horizontalJitter) : Vector3.zero;

        Popup popup = Acquire();
        popup.Origin = position + jitter;
        popup.Age = 0f;
        popup.Color = color;
        popup.Text.text = text;
        popup.Text.color = color;
        popup.Text.gameObject.SetActive(true);
        active.Add(popup);
    }

    private Popup Acquire()
    {
        if (free.Count > 0) return free.Pop();

        if (active.Count >= maxActive)
        {
            Popup oldest = active[0];
            active.RemoveAt(0);
            return oldest;
        }

        var go = new GameObject("Popup");
        go.transform.SetParent(transform, false);
        var text = go.AddComponent<TextMeshPro>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        if (font != null) text.font = font;
        return new Popup { Text = text };
    }

    private void Update()
    {
        var cam = Camera.main;
        if (cam == null) return;

        for (int i = active.Count - 1; i >= 0; i--)
        {
            Popup popup = active[i];
            popup.Age += Time.deltaTime;
            float t = popup.Age / lifetime;

            if (t >= 1f)
            {
                popup.Text.gameObject.SetActive(false);
                active.RemoveAt(i);
                free.Push(popup);
                continue;
            }

            Vector3 position = popup.Origin + Vector3.up * (riseSpeed * popup.Age);
            Transform tf = popup.Text.transform;
            tf.position = position;
            tf.rotation = cam.transform.rotation;

            float distance = Vector3.Distance(position, cam.transform.position);
            tf.localScale = Vector3.one * Mathf.Clamp(distance / referenceDistance, scaleRange.x, scaleRange.y);

            Color color = popup.Color;
            color.a = t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f;
            popup.Text.color = color;
        }
    }
}
