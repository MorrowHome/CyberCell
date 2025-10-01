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
        // 滚轮输入
        float scrollValue = Mouse.current.scroll.ReadValue().y;  // 注意这里是 Vector2，取 y
        if (scrollValue != 0)
        {
            targetZoom -= scrollValue * mouseZoomSensitivity;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        // 平滑过渡
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * smoothSpeed);

        // 跟随玩家
        if (playerTransform != null)
        {
            Vector3 newPosition = playerTransform.position;
            newPosition.z = transform.position.z; // 保持摄像机的 z 位置不变
            transform.position = newPosition;
        }
    }
}
