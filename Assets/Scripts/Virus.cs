using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Virus : MonoBehaviour, IDamageable
{
    [Header("ÒÆ¶¯²ÎÊý")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private Transform target;


    [SerializeField] private float HP = 10f;

    [SerializeField] private Rigidbody rb;

    float IDamageable.HP => HP;

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
}
