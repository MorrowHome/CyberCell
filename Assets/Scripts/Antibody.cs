using UnityEngine;

public class Antibody : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 5f; // 最长生存时间
    [SerializeField] private float searchRange = 10f; // 自动寻找范围

    [Header("Motion Effects")]
    [SerializeField] private float jitterStrength = 0.2f;   // 抖动幅度
    [SerializeField] private float jitterFrequency = 2f;    // 抖动频率
    [SerializeField] private float snakeAmplitude = 0.2f;   // 蛇形幅度
    [SerializeField] private float snakeFrequency = 8f;     // 蛇形频率

    private Transform target;
    private float timeAlive = 0f;

    private float searchCooldown = 0.5f;
    private float searchTimer = 0f;

    private Vector3 baseDirection; // 用于平滑旋转时的参考

    public void SetTarget(Transform virusTarget)
    {
        target = virusTarget;
    }

    void Update()
    {
        timeAlive += Time.deltaTime;
        if (timeAlive > lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        if (target == null)
        {
            searchTimer -= Time.deltaTime;
            if (searchTimer <= 0f)
            {
                searchTimer = searchCooldown;
                target = FindNewTarget();
            }
            return;
        }

        // 计算基础前进方向
        Vector3 dir = (target.position - transform.position).normalized;

        // --- ?? 添加自然抖动 ---
        // Perlin 噪声使得运动略带漂浮感
        float noiseX = Mathf.PerlinNoise(Time.time * jitterFrequency, GetInstanceID() * 0.1f) - 0.5f;
        float noiseY = Mathf.PerlinNoise(GetInstanceID() * 0.1f, Time.time * jitterFrequency) - 0.5f;
        Vector3 jitter = (transform.right * noiseX + transform.up * noiseY) * jitterStrength;

        // 轻微蛇形（绕垂直方向的小摆动）
        Vector3 side = Vector3.Cross(dir, Vector3.up).normalized;
        Vector3 snake = side * Mathf.Sin(Time.time * snakeFrequency + GetInstanceID()) * snakeAmplitude;

        // 最终合成移动向量
        Vector3 move = (dir * speed + jitter + snake);
        transform.position += move * Time.deltaTime;

        // 平滑朝向目标
        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 10f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IDamageable>(out var dmg))
        {
            dmg.TakeDamage(1f);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 当目标死亡后自动寻找新的敌人
    /// </summary>
    private Transform FindNewTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRange);
        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var dmg))
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = hit.transform;
                }
            }
        }
        return nearest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, searchRange);
    }
}
