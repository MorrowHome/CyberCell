using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [Header("�ƶ�����")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float flySpeed = 3f;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float mouseSensitivity = 2f;

    private CharacterController characterController;
    private PlayerInputActions inputActions;

    [SerializeField] private Transform cameraHolder; // ����������󣬹̶����ͷ���߶�
    private Camera playerCamera;

    private Vector2 moveInput = Vector2.zero;
    private float verticalInput = 0f;
    private bool isSprinting = false;

    private float xRotation = 0f; // �����������ת

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        inputActions = InputManager.Instance.inputActions;
        playerCamera = cameraHolder.GetComponentInChildren<Camera>();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

        inputActions.Player.Sprint.started += OnSprint;
        inputActions.Player.Sprint.canceled += OnSprint;

        inputActions.Player.FlyUp.started += OnFlyUp;
        inputActions.Player.FlyUp.canceled += OnFlyUp;
        inputActions.Player.FlyDown.started += OnFlyDown;
        inputActions.Player.FlyDown.canceled += OnFlyDown;


        inputActions.Player.Look.performed += OnLook;
    }

    private void Update()
    {
        HandleMovement();
        HandleCursor();
    }

    private void OnDisable()
    {
        inputActions.Player.Sprint.started -= OnSprint;
        inputActions.Player.Sprint.canceled -= OnSprint;
        inputActions.Player.FlyUp.started -= OnFlyUp;
        inputActions.Player.FlyUp.canceled -= OnFlyUp;
        inputActions.Player.FlyDown.started -= OnFlyDown;
        inputActions.Player.FlyDown.canceled -= OnFlyDown;
        inputActions.Player.Look.performed -= OnLook;

        inputActions.Player.Disable();
    }

    #region Movement
    private void HandleMovement()
    {
        // ��ȡ�ƶ�����
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        // ˮƽ�ƶ������������ҳ���
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        // ��������
        move.y = verticalInput * flySpeed;

        float finalSpeed = speed * (isSprinting ? sprintMultiplier : 1f);

        characterController.Move(move * finalSpeed * Time.deltaTime);
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        isSprinting = context.ReadValueAsButton();
    }

    private void OnFlyUp(InputAction.CallbackContext context)
    {
        verticalInput = context.ReadValueAsButton() ? 1f : 0f;
    }

    private void OnFlyDown(InputAction.CallbackContext context)
    {
        verticalInput = context.ReadValueAsButton() ? -1f : 0f;
    }
    #endregion

    #region Look
    private void OnLook(InputAction.CallbackContext context)
    {
        Vector2 lookDelta = context.ReadValue<Vector2>() * mouseSensitivity;

        // �����������ת
        xRotation -= lookDelta.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // ���������ת
        transform.Rotate(Vector3.up * lookDelta.x);
    }
    #endregion

    private void HandleCursor()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }


}
