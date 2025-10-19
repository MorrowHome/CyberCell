using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bacteria : MonoBehaviour, IDamageable, IDamaging
{
    [Header("移动参数")]
    [SerializeField] private float speed = 1.5f; // 比病毒慢一些
    [SerializeField] private Transform target;

    [Header("生命与伤害参数")]
    [SerializeField] private float HP = 20f; // 细菌生命更厚
    [SerializeField] private float damage = 0.5f; // 单次伤害较小，但可以持续接触伤害
    [SerializeField] private float damageInterval = 1f; // 每秒伤害间隔
    private float damageTimer = 0f;

    [SerializeField] private Rigidbody rb;

    float IDamageable.HP => HP;
    public float Damage => damage;

    private bool isTouchingHeart = false; // 标记是否正在接触心脏

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        target = MapGenerator.Instance.heartCellTransform;
    }

    void FixedUpdate()
    {
        if (target == null || isTouchingHeart) return;

        Vector3 direction = (target.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
        transform.LookAt(target);
    }

    void Update()
    {
        // 如果正在接触心脏，每隔一定时间造成一次伤害
        if (isTouchingHeart)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                GameManager.Instance.TakeDamage(damage);
                damageTimer = 0f;
            }
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void TakeDamage(float amount)
    {
        HP -= amount;
        if (HP <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null && other.transform.parent.CompareTag("HeartCell"))
        {
            isTouchingHeart = true;
            damageTimer = 0f; // 立即开始计时
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.parent != null && other.transform.parent.CompareTag("HeartCell"))
        {
            isTouchingHeart = false;
        }
    }
}
