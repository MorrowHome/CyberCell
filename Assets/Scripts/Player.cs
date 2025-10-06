using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float flySpeed = 3f;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float mouseSensitivity = 2f;

    [Header("引用")]
    [SerializeField] private Transform cameraHolder;

    [SerializeField] private bool aaa = false;

    private CharacterController controller;
    private PlayerInputActions input;
    private Camera playerCamera;

    private Vector2 moveInput;
    private float verticalInput;
    private bool isSprinting;

    private float xRotation;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = InputManager.Instance.inputActions;
        playerCamera = cameraHolder.GetComponentInChildren<Camera>();
    }

    private void OnEnable()
    {
        var player = input.Player;
        player.Enable();


        player.Sprint.performed += ctx => isSprinting = true;
        player.Sprint.canceled += ctx => isSprinting = false;

        player.FlyUp.performed += ctx => verticalInput = 1f;
        player.FlyUp.canceled += ctx => verticalInput = 0f;
        player.FlyDown.performed += ctx => verticalInput = -1f;
        player.FlyDown.canceled += ctx => verticalInput = 0f;

        player.Look.performed += OnLook;
    }

    private void OnDisable()
    {
        var player = input.Player;


        player.Disable();
    }

    private void Update()
    {
        HandleMovement();
        HandleCursorLock();
    }

    #region Movement
    private void HandleMovement()
    {
        Vector2 inputDir = input.Player.Move.ReadValue<Vector2>();
        Vector3 moveDir = transform.right * inputDir.x + transform.forward * inputDir.y;


        Vector3 finalMove = moveDir * walkSpeed * (isSprinting ? sprintMultiplier : 1f);
        finalMove.y = verticalInput * flySpeed;

        controller.Move(finalMove * Time.deltaTime);
    }
    #endregion

    #region Look
    private void OnLook(InputAction.CallbackContext context)
    {
        if (!aaa) return;
        Vector2 delta = context.ReadValue<Vector2>() * mouseSensitivity;

        // 上下看
        xRotation = Mathf.Clamp(xRotation - delta.y, -90f, 90f);
        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 左右转
        transform.Rotate(Vector3.up * delta.x);
    }
    #endregion

    #region Cursor
    private void HandleCursorLock()
    {
        if (!aaa) return;
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            LockCursor(true);
        }
        else if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            LockCursor(false);
        }
    }

    private void LockCursor(bool locked)
    {
        if (!aaa) return;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
    #endregion
}
