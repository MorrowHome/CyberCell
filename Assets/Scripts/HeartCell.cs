using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class HeartCell : MonoBehaviour
{
    [Header("搏动参数")]
    [SerializeField] private float beatSpeed = 2f;      // 心跳速度（次/秒）
    [SerializeField] private float beatScale = 0.1f;    // 扩张幅度
    [SerializeField] private float emissionIntensity = 5f; // 发光强度

    private Renderer rend;
    private Material mat;
    private Vector3 baseScale;
    private Color baseEmission;

    void Start()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material; // 注意：会实例化一个新材质副本
        baseScale = transform.localScale;

        // 如果材质有 Emission，则记录初始发光颜色
        if (mat.HasProperty("_EmissionColor"))
        {
            baseEmission = mat.GetColor("_EmissionColor");
            mat.EnableKeyword("_EMISSION");
        }
    }

    void Update()
    {
        // 让心跳在 0~1 的节奏中循环变化
        float pulse = (Mathf.Sin(Time.time * beatSpeed * Mathf.PI * 2) + 1f) / 2f;

        // 缩放变化：基础缩放 + 心跳脉动
        transform.localScale = baseScale * (1f + beatScale * pulse);

        // 发光变化：心跳时变亮
        if (mat.HasProperty("_EmissionColor"))
        {
            Color emission = baseEmission * (1f + pulse * emissionIntensity);
            mat.SetColor("_EmissionColor", emission);
        }
    }
}
