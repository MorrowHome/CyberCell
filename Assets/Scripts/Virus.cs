using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Virus : MonoBehaviour
{
    [Header("移动参数")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private Transform target;

    [Header("视觉效果")]
    [SerializeField] private Transform visual;     // 子物体
    [SerializeField] private float rotateSpeed = 90f; // 自旋速度
    [SerializeField] private float wobbleAmplitude = 0.2f; // 晃动幅度
    [SerializeField] private float wobbleSpeed = 2f; // 晃动速度
    [SerializeField] private float pulseScale = 0.1f; // 呼吸脉动幅度
    [SerializeField] private float pulseSpeed = 2f; // 呼吸频率

    private Rigidbody rb;
    private Vector3 visualOriginalLocalPos;
    private Vector3 visualOriginalScale;
    private Vector3 wobbleOffset;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        target = MapGenerator.Instance.heartCellTransform;

        if (visual == null)
            visual = transform.Find("Visual"); // 自动查找子物体

        if (visual != null)
        {
            visualOriginalLocalPos = visual.localPosition;
            visualOriginalScale = visual.localScale;
            wobbleOffset = new Vector3(Random.value * 10f, Random.value * 10f, Random.value * 10f);
        }
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
        if (visual == null) return;

        // ?? 自旋
        visual.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.Self);
        visual.Rotate(Vector3.right, rotateSpeed * 0.3f * Time.deltaTime, Space.Self);

        // ?? 随机晃动（左右上下漂浮）
        float wobbleX = Mathf.Sin(Time.time * wobbleSpeed + wobbleOffset.x) * wobbleAmplitude;
        float wobbleY = Mathf.Cos(Time.time * wobbleSpeed + wobbleOffset.y) * wobbleAmplitude;
        float wobbleZ = Mathf.Sin(Time.time * wobbleSpeed * 0.5f + wobbleOffset.z) * wobbleAmplitude;
        visual.localPosition = visualOriginalLocalPos + new Vector3(wobbleX, wobbleY, wobbleZ);

        // ?? 呼吸脉动（轻微缩放）
        float scalePulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
        visual.localScale = visualOriginalScale * scalePulse;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
