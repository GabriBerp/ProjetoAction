using UnityEngine;

// Placa de texto 3D simples criada em runtime (ex.: "SAIDA" em cima da porta).
public class WorldSign : MonoBehaviour
{
    [SerializeField] private string text = "SAIDA";
    [SerializeField] private Color color = new Color(1f, 0.9f, 0.6f);
    [SerializeField] private float worldHeight = 0.16f;
    [SerializeField] private bool billboard = true;

    private Transform label;

    private void Start()
    {
        label = DialogueFont.CreateWorldText("Texto", transform, text, color, 48f, worldHeight).transform;
        label.localPosition = Vector3.zero;
    }

    private void LateUpdate()
    {
        if (!billboard || label == null) return;

        var cam = Camera.main;
        if (cam == null) return;

        label.rotation = Quaternion.LookRotation(label.position - cam.transform.position, Vector3.up);
    }
}
