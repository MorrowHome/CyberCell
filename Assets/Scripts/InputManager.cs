// InputManager.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    // ��Ҫ�ڱ༭�� -> InputActions �����ɵ� C# ��װ����
    // (Ĭ������������ .inputactions ��Դ������ͬ)
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

        // ���������ã�ֻ new һ�Σ�
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
