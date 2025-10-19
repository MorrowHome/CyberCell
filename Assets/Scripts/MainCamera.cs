using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// 自由旋转 + 平移 + 缩放 摄像机控制（新输入系统版）
/// - 鼠标右键：旋转（Yaw + Pitch）
/// - 鼠标中键：平移
/// - 滚轮：缩放（围绕焦点）
/// </summary>
[RequireComponent(typeof(Camera))]
public class FreeLookCamera : MonoBehaviour
{
    [Header("焦点控制")]
    public Vector3 pivot = Vector3.zero;    // 当前旋转焦点
    public float distance = 20f;            // 当前距离
    public float minDistance = 5f;
    public float maxDistance = 80f;

    [Header("旋转限制")]
    public float minPitch = 10f;
    public float maxPitch = 80f;

    [Header("速度参数")]
    public float rotateSpeed = 100f;
    public float panSpeed = 0.5f;
    public float zoomSpeed = 10f;

    [Header("平滑参数")]
    public float smoothTime = 0.12f;

    [Header("初始化设置")]
    public bool useCustomInit = false;      // 是否使用自定义初始状态
    public Vector3 initPivot = Vector3.zero;
    public float initYaw = 0f;
    public float initPitch = 45f;
    public float initDistance = 20f;

    private float yaw;
    private float pitch;
    private float targetDistance;
    private Vector3 targetPivot;
    private Vector3 pivotVelocity;

    private PlayerInputActions input;
    private InputAction look;
    private InputAction scroll;
    private InputAction middle;
    private InputAction right;

    void Awake()
    {
        input = new PlayerInputActions();
        look = input.Camera.Look;
        scroll = input.Camera.Scroll;
        middle = input.Camera.MiddleClick;
        right = input.Camera.RightClick;
    }

    void OnEnable() => input.Enable();
    void OnDisable() => input.Disable();

    void Start()
    {
        if (useCustomInit)
        {
            // 使用 Inspector 自定义的初始状态
            pivot = initPivot;
            yaw = initYaw;
            pitch = Mathf.Clamp(initPitch, minPitch, maxPitch);
            distance = Mathf.Clamp(initDistance, minDistance, maxDistance);
        }
        else
        {
            // 自动从当前位置推算初始状态
            Vector3 dir = (transform.position - pivot).normalized;
            distance = Vector3.Distance(transform.position, pivot);
            pitch = Mathf.Asin(dir.y) * Mathf.Rad2Deg;
            yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        }

        targetPivot = pivot;
        targetDistance = distance;
    }

    void LateUpdate()
    {
        if (EventSystem.current && EventSystem.current.IsPointerOverGameObject())
            return;

        Vector2 lookDelta = look.ReadValue<Vector2>();
        float scrollDelta = scroll.ReadValue<Vector2>().y;

        HandleRotation(lookDelta);
        HandlePan(lookDelta);
        HandleZoom(scrollDelta);

        pivot = Vector3.SmoothDamp(pivot, targetPivot, ref pivotVelocity, smoothTime);
        distance = Mathf.Lerp(distance, targetDistance, 1f - Mathf.Exp(-5f * Time.deltaTime));

        // 构造旋转与位置
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        transform.position = pivot + offset;
        transform.rotation = rotation;
    }

    void HandleRotation(Vector2 lookDelta)
    {
        if (right.IsPressed())
        {
            yaw += lookDelta.x * rotateSpeed * Time.deltaTime * 0.1f;
            pitch -= lookDelta.y * rotateSpeed * Time.deltaTime * 0.1f;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
    }

    void HandlePan(Vector2 lookDelta)
    {
        if (middle.IsPressed())
        {
            Vector3 rightDir = transform.right;
            Vector3 forwardDir = transform.forward;
            forwardDir.y = 0;
            forwardDir.Normalize();

            float panFactor = Mathf.Max(Mathf.Pow(distance * 0.1f, 0.8f), 1f);
            Vector3 move = (-rightDir * lookDelta.x - forwardDir * lookDelta.y)
                           * panSpeed * panFactor * Time.deltaTime;

            targetPivot += move;
        }
    }


    void HandleZoom(float scrollDelta)
    {
        if (Mathf.Abs(scrollDelta) < 0.01f) return;

        targetDistance -= scrollDelta * zoomSpeed * Time.deltaTime * 10f;
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
    }
}
