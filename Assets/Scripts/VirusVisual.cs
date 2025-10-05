using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class VirusVisual : MonoBehaviour
{
    [Header("�Ӿ�Ч��")]
    [SerializeField] private Transform visual;     // ������
    [SerializeField] private float rotateSpeed = 90f; // �����ٶ�
    [SerializeField] private float wobbleAmplitude = 0.2f; // �ζ�����
    [SerializeField] private float wobbleSpeed = 2f; // �ζ��ٶ�
    [SerializeField] private float pulseScale = 0.1f; // ������������
    [SerializeField] private float pulseSpeed = 2f; // ����Ƶ��
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Vector3 visualOriginalLocalPos;
    private Vector3 visualOriginalScale;
    private Vector3 wobbleOffset;

    void Start()
    {

        if (visual == null)
            visual = transform.Find("Visual"); // �Զ�����������

        if (visual != null)
        {
            visualOriginalLocalPos = visual.localPosition;
            visualOriginalScale = visual.localScale;
            wobbleOffset = new Vector3(Random.value * 10f, Random.value * 10f, Random.value * 10f);
        }
    }
    // Update is called once per frame
    void Update()
    {
        // ?? ����
        visual.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.Self);
        visual.Rotate(Vector3.right, rotateSpeed * 0.3f * Time.deltaTime, Space.Self);

        // ?? ����ζ�����������Ư����
        float wobbleX = Mathf.Sin(Time.time * wobbleSpeed + wobbleOffset.x) * wobbleAmplitude;
        float wobbleY = Mathf.Cos(Time.time * wobbleSpeed + wobbleOffset.y) * wobbleAmplitude;
        float wobbleZ = Mathf.Sin(Time.time * wobbleSpeed * 0.5f + wobbleOffset.z) * wobbleAmplitude;
        visual.localPosition = visualOriginalLocalPos + new Vector3(wobbleX, wobbleY, wobbleZ);

        // ?? ������������΢���ţ�
        float scalePulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
        visual.localScale = visualOriginalScale * scalePulse;
    }
}
