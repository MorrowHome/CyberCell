using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Virus : MonoBehaviour, IDamageable, IDamaging
{
    [Header("移动参数")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private Transform target;


    [SerializeField] private float HP = 10f;

    [SerializeField] private Rigidbody rb;

    [SerializeField] private float damage = 1f;

    float IDamageable.HP => HP;

    public float Damage => damage;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        target = MapGenerator.Instance.heartCellTransform;
    }

    void FixedUpdate()
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
        transform.LookAt(target);
    }

    void Update()
    {

    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void TakeDamage(float amount)
    {
        HP -= amount;
        if(HP <= 0f)
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
        if(other.transform.parent.CompareTag("HeartCell"))
        {
            GameManager.Instance.TakeDamage(damage);
            Die();
        }
    }
}
