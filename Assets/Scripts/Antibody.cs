using UnityEngine;

public class Antibody : MonoBehaviour
{
    [Header("移动参数")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private float searchInterval = 0.3f; // 寻敌间隔时间

    [Header("运动效果")]
    [SerializeField] private float jitterStrength = 0.2f;
    [SerializeField] private float snakeAmplitude = 0.2f;
    [SerializeField] private float snakeFrequency = 8f;

    [Header("喷射参数")]
    [SerializeField] private float sprayDuration = 0.5f;

    [Header("伤害参数")]
    [SerializeField] private float damage = 2f;

    private Transform target;
    private GameObject prefabRef;
    private float timeAlive;
    private bool isSpraying = false;
    private Vector3 sprayDirection;
    private float nextSearchTime; // 下次寻敌的时间戳

    public void Init(GameObject prefab)
    {
        prefabRef = prefab;
        timeAlive = 0f;
        target = null;
        isSpraying = false;
        nextSearchTime = 0f;
    }

    public void SetSprayDirection(Vector3 dir)
    {
        isSpraying = true;
        sprayDirection = dir.normalized;
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }

    public void SetSpeed(float s)
    {
        speed = s;
    }

    private void Update()
    {
        if (EnemyManager.Instance.enemyCount == 0) return;

        timeAlive += Time.deltaTime;
        if (timeAlive >= lifeTime)
        {
            ReturnToPool();
            return;
        }

        Vector3 dir;

        // ===== 喷射阶段 =====
        if (isSpraying)
        {
            dir = sprayDirection;
            if (timeAlive >= sprayDuration)
                isSpraying = false;
        }
        else
        {
            // ===== 智能寻敌阶段 =====
            if (target == null || !target.gameObject.activeSelf)
            {
                if (Time.time >= nextSearchTime)
                {
                    target = EnemyManager.Instance.GetNearestEnemy(transform.position);
                    nextSearchTime = Time.time + searchInterval;
                }
                if (target == null) return;
            }

            dir = (target.position - transform.position).normalized;
        }

        // ===== 抖动 + 蛇形轨迹 =====
        Vector3 jitter = Random.insideUnitSphere * jitterStrength;
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

    private void ReturnToPool()
    {
        if (prefabRef == null)
        {
            Destroy(gameObject);
            return;
        }
        ObjectPoolManager.Instance.Return(prefabRef, gameObject);
    }
}
