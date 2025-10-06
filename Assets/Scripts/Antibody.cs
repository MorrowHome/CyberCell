using UnityEngine;

public class Antibody : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 5f; // �����ʱ��
    [SerializeField] private float searchRange = 10f; // �Զ�Ѱ�ҷ�Χ

    [Header("Motion Effects")]
    [SerializeField] private float jitterStrength = 0.2f;   // ��������
    [SerializeField] private float jitterFrequency = 2f;    // ����Ƶ��
    [SerializeField] private float snakeAmplitude = 0.2f;   // ���η���
    [SerializeField] private float snakeFrequency = 8f;     // ����Ƶ��

    private Transform target;
    private float timeAlive = 0f;

    private float searchCooldown = 0.5f;
    private float searchTimer = 0f;

    private Vector3 baseDirection; // ����ƽ����תʱ�Ĳο�

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

        // �������ǰ������
        Vector3 dir = (target.position - transform.position).normalized;

        // --- ?? �����Ȼ���� ---
        // Perlin ����ʹ���˶��Դ�Ư����
        float noiseX = Mathf.PerlinNoise(Time.time * jitterFrequency, GetInstanceID() * 0.1f) - 0.5f;
        float noiseY = Mathf.PerlinNoise(GetInstanceID() * 0.1f, Time.time * jitterFrequency) - 0.5f;
        Vector3 jitter = (transform.right * noiseX + transform.up * noiseY) * jitterStrength;

        // ��΢���Σ��ƴ�ֱ�����С�ڶ���
        Vector3 side = Vector3.Cross(dir, Vector3.up).normalized;
        Vector3 snake = side * Mathf.Sin(Time.time * snakeFrequency + GetInstanceID()) * snakeAmplitude;

        // ���պϳ��ƶ�����
        Vector3 move = (dir * speed + jitter + snake);
        transform.position += move * Time.deltaTime;

        // ƽ������Ŀ��
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
    /// ��Ŀ���������Զ�Ѱ���µĵ���
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
