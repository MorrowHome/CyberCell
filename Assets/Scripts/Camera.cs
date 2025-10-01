using UnityEngine;
using UnityEngine.InputSystem;

public class CameraZoom : MonoBehaviour
{
    private Camera cam;

    [SerializeField] private Transform playerTransform;

    [Header("Zoom Settings")]
    [SerializeField] private float mouseZoomSensitivity = 0.5f;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 20f;

    private float targetZoom;

    void Start()
    {
        cam = GetComponent<Camera>();
        targetZoom = cam.orthographicSize;
    }

    void Update()
    {
        // ��������
        float scrollValue = Mouse.current.scroll.ReadValue().y;  // ע�������� Vector2��ȡ y
        if (scrollValue != 0)
        {
            targetZoom -= scrollValue * mouseZoomSensitivity;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        // ƽ������
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * smoothSpeed);

        // �������
        if (playerTransform != null)
        {
            Vector3 newPosition = playerTransform.position;
            newPosition.z = transform.position.z; // ����������� z λ�ò���
            transform.position = newPosition;
        }
    }
}
