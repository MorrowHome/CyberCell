using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// 使用新输入系统的主摄像机控制：
/// - 固定高度 & 固定俯仰角
/// - 鼠标中键拖动平移
/// - 鼠标右键左右拖动旋转
/// - 鼠标滚轮缩放
/// </summary>
[RequireComponent(typeof(Camera))]
public class FixedPitchCamera_NewInput : MonoBehaviour
{
    [Header("固定设置")]
    public float fixedPitch = 45f; // 固定俯仰角
    public float minHeight = 5f;   // 最小高度
    public float maxHeight = 50f;  // 最大高度

    [Header("控制速度")]
    public float panSpeed = 10f;
    public float rotateSpeed = 120f;
    public float zoomSpeed = 5f;   // 滚轮缩放速度

    [Header("平滑")]
    public bool smooth = true;
    public float smoothSpeed = 12f;

    private float yaw;
    private float currentHeight;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    // 新输入系统
    private PlayerInputActions playerInput;
    private InputAction lookAction;
    private InputAction middleButtonAction;
    private InputAction rightButtonAction;
    private InputAction scrollAction;

    void Awake()
    {
        playerInput = new PlayerInputActions();

        // 定义输入
        lookAction = playerInput.Camera.Look;                // 鼠标移动 (Vector2)
        middleButtonAction = playerInput.Camera.MiddleClick; // 鼠标中键
        rightButtonAction = playerInput.Camera.RightClick;   // 鼠标右键
        scrollAction = playerInput.Camera.Scroll;            // 鼠标滚轮 (Vector2.y)
    }

    void OnEnable() => playerInput.Enable();
    void OnDisable() => playerInput.Disable();

    void Start()
    {
        yaw = transform.eulerAngles.y;
        currentHeight = transform.position.y;
        targetPosition = new Vector3(transform.position.x, currentHeight, transform.position.z);
        targetRotation = Quaternion.Euler(fixedPitch, yaw, 0f);
        transform.rotation = targetRotation;
        transform.position = targetPosition;
    }

    void LateUpdate()
    {
        // 如果鼠标在 UI 上，就不执行建造/删除
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Vector2 look = lookAction.ReadValue<Vector2>();
        float scroll = scrollAction.ReadValue<Vector2>().y;

        HandleRotation(look);
        HandlePan(look);
        HandleZoom(scroll);

        targetRotation = Quaternion.Euler(fixedPitch, yaw, 0f);

        ApplySmoothing();
    }

    private void HandleRotation(Vector2 look)
    {
        if (rightButtonAction.IsPressed())
        {
            yaw += look.x * rotateSpeed * Time.deltaTime * 0.01f; // 缩小灵敏度
        }
    }

    private void HandlePan(Vector2 look)
    {
        if (middleButtonAction.IsPressed())
        {
            Vector3 right = transform.right;
            Vector3 forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 move = (-right * look.x - forward * look.y) * panSpeed * Time.deltaTime * 0.01f;
            targetPosition += move;
            targetPosition.y = currentHeight;
        }
    }

    private void HandleZoom(float scroll)
    {
        if (Mathf.Abs(scroll) > 0.01f)
        {
            currentHeight -= scroll * zoomSpeed * Time.deltaTime * 10f; // 调整缩放
            currentHeight = Mathf.Clamp(currentHeight, minHeight, maxHeight);
            targetPosition.y = currentHeight;
        }
    }

    private void ApplySmoothing()
    {
        if (smooth)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));
        }
        else
        {
            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }
    }
}
