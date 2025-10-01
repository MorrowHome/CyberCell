using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// ʹ��������ϵͳ������������ƣ�
/// - �̶��߶� & �̶�������
/// - ����м��϶�ƽ��
/// - ����Ҽ������϶���ת
/// - ����������
/// </summary>
[RequireComponent(typeof(Camera))]
public class FixedPitchCamera_NewInput : MonoBehaviour
{
    [Header("�̶�����")]
    public float fixedPitch = 45f; // �̶�������
    public float minHeight = 5f;   // ��С�߶�
    public float maxHeight = 50f;  // ���߶�

    [Header("�����ٶ�")]
    public float panSpeed = 10f;
    public float rotateSpeed = 120f;
    public float zoomSpeed = 5f;   // ���������ٶ�

    [Header("ƽ��")]
    public bool smooth = true;
    public float smoothSpeed = 12f;

    private float yaw;
    private float currentHeight;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    // ������ϵͳ
    private PlayerInputActions playerInput;
    private InputAction lookAction;
    private InputAction middleButtonAction;
    private InputAction rightButtonAction;
    private InputAction scrollAction;

    void Awake()
    {
        playerInput = new PlayerInputActions();

        // ��������
        lookAction = playerInput.Camera.Look;                // ����ƶ� (Vector2)
        middleButtonAction = playerInput.Camera.MiddleClick; // ����м�
        rightButtonAction = playerInput.Camera.RightClick;   // ����Ҽ�
        scrollAction = playerInput.Camera.Scroll;            // ������ (Vector2.y)
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
            yaw += look.x * rotateSpeed * Time.deltaTime * 0.01f; // ��С������
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
            currentHeight -= scroll * zoomSpeed * Time.deltaTime * 10f; // ��������
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
