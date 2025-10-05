using UnityEngine;

public class Antibody : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 5f; // 最长生存时间
    [SerializeField] private float searchRange = 10f; // 自动寻找范围

    private Transform target;
    private float timeAlive = 0f;

    public void SetTarget(Transform virusTarget)
    {
        target = virusTarget;
    }

    private float searchCooldown = 0.5f;
    private float searchTimer = 0f;

    void Update()
    {
        timeAlive += Time.deltaTime;
        if (timeAlive > lifeTime) { Destroy(gameObject); return; }

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

        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
        transform.LookAt(target);
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
