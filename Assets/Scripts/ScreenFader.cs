using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Cria o próprio Canvas preto sob demanda: nada pra montar na cena.
public class ScreenFader : MonoBehaviour
{
    private static ScreenFader instance;
    private CanvasGroup group;

    public static ScreenFader Instance
    {
        get
        {
            if (instance == null) Create();
            return instance;
        }
    }

    // Com "Enter Play Mode Options" (sem domain reload) o static sobreviveria apontando pra um objeto morto.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => instance = null;

    private static void Create()
    {
        var go = new GameObject("ScreenFader");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<ScreenFader>();
        instance.Build();
    }

    private void Build()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        group = gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        var imageGo = new GameObject("Black", typeof(RectTransform), typeof(Image));
        imageGo.transform.SetParent(transform, false);

        var rect = (RectTransform)imageGo.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var image = imageGo.GetComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = false;
    }

    public IEnumerator FadeTo(float target, float duration)
    {
        float start = group.alpha;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            group.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        group.alpha = target;
    }
}