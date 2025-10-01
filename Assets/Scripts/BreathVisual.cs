using UnityEngine;

public class HeartCell : MonoBehaviour
{
    [Header("搏动参数")]
    [SerializeField] private float beatSpeed = 2f;     // 心跳频率
    [SerializeField] private float beatStrength = 0.2f; // 扩张幅度
    [SerializeField] private Transform heartCell;       // 子物体引用

    private Vector3 originalScale;
    private float startTime; // 每个实例的独立起始时间

    void Start()
    {
        // 如果没有手动拖拽，在运行时自动查找
        if (heartCell == null)
            heartCell = transform.Find("HeartCell");

        if (heartCell == null)
        {
            Debug.LogError("没有找到子物体 HeartCell！");
            return;
        }

        // 记录原始缩放
        originalScale = heartCell.localScale;

        // 记录本对象的起始时间
        startTime = Time.time;
    }

    void Update()
    {
        if (heartCell == null) return;

        // 使用相对时间（实例化后开始计时）
        float elapsed = Time.time - startTime;

        // 通过正弦波模拟心跳缩放
        float scaleFactor = 1 + Mathf.Sin(elapsed * beatSpeed) * beatStrength;

        // 应用缩放
        heartCell.localScale = originalScale * scaleFactor;
    }
}
