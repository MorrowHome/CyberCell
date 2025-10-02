// InputManager.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    // 需要在编辑器 -> InputActions 里生成的 C# 包装类名
    // (默认生成类名与 .inputactions 资源名字相同)
    public PlayerInputActions inputActions { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 创建并启用（只 new 一次）
        inputActions = new PlayerInputActions();
        inputActions.Enable();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            inputActions?.Disable();
            Instance = null;
        }
    }
}
