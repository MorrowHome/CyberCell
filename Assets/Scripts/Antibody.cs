using UnityEngine;

public class Antibody : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 5f; // 自动销毁时间
    private Transform target;

    public void SetTarget(Transform virusTarget)
    {
        target = virusTarget;
        Destroy(gameObject, lifeTime); // 防止无限存在
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
        transform.LookAt(target);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Virus"))
        {
            Destroy(other.gameObject); // 病毒被击中
            Destroy(gameObject);       // 自己也销毁
        }
    }
}
