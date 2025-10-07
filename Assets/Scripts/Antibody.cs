using UnityEngine;

public class Antibody : MonoBehaviour
{
    [Header("移动参数")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private float searchRange = 10f;

    [Header("运动效果")]
    [SerializeField] private float jitterStrength = 0.2f;
    [SerializeField] private float snakeAmplitude = 0.2f;
    [SerializeField] private float snakeFrequency = 8f;

    [Header("伤害参数")]
    [SerializeField] private float damage = 2f;

    private Transform target;
    private GameObject prefabRef;
    private float timeAlive;

    public void Init(GameObject prefab)
    {
        prefabRef = prefab;
        timeAlive = 0f;
        target = null;
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }

    private void Update()
    {
        timeAlive += Time.deltaTime;
        if (timeAlive >= lifeTime)
        {
            ReturnToPool();
            return;
        }

        if (target == null)
        {
            target = FindNewTarget();
            if (target == null) return;
        }

        Vector3 dir = (target.position - transform.position).normalized;
        Vector3 jitter = (Random.insideUnitSphere * jitterStrength);
        Vector3 snake = Vector3.Cross(dir, Vector3.up) * Mathf.Sin(Time.time * snakeFrequency) * snakeAmplitude;
        transform.position += (dir * speed + jitter + snake) * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(dir);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IDamageable>(out var dmg))
        {
            dmg.TakeDamage(damage);
            ReturnToPool();
        }
    }

    private Transform FindNewTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRange);
        Transform nearest = null;
        float minDist = Mathf.Infinity;
        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent<IDamageable>(out _)) continue;
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = hit.transform;
            }
        }
        return nearest;
    }

    private void ReturnToPool()
    {
        if (prefabRef == null)
        {
            Debug.LogWarning($"[Antibody] 没有 prefabRef，无法回收，直接销毁。");
            Destroy(gameObject);
            return;
        }

        ObjectPoolManager.Instance.Return(prefabRef, gameObject);
    }
}
