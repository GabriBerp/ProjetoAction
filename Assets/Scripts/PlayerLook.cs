using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    [SerializeField] private Transform cameraRoot;
    [SerializeField] private float sensitivity = 0.1f;
    [SerializeField] private float minPitch = -60f;
    [SerializeField] private float maxPitch = 60f;

    private float pitch;
    private float yaw;
    public float Yaw => yaw;

    void Awake()
    {
        yaw = transform.eulerAngles.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // if (!GameManager.GameStarted) return;

        Vector2 delta = Mouse.current.delta.ReadValue() * sensitivity;

        yaw += delta.x;
        pitch = Mathf.Clamp(pitch - delta.y, minPitch, maxPitch);

        // Gira direto no transform, no mesmo frame do mouse
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}