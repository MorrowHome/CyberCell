using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class HeartCell : MonoBehaviour
{
    [Header("��������")]
    [SerializeField] private float beatSpeed = 2f;      // �����ٶȣ���/�룩
    [SerializeField] private float beatScale = 0.1f;    // ���ŷ���
    [SerializeField] private float emissionIntensity = 5f; // ����ǿ��

    private Renderer rend;
    private Material mat;
    private Vector3 baseScale;
    private Color baseEmission;

    void Start()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material; // ע�⣺��ʵ����һ���²��ʸ���
        baseScale = transform.localScale;

        // ��������� Emission�����¼��ʼ������ɫ
        if (mat.HasProperty("_EmissionColor"))
        {
            baseEmission = mat.GetColor("_EmissionColor");
            mat.EnableKeyword("_EMISSION");
        }
    }

    void Update()
    {
        // �������� 0~1 �Ľ�����ѭ���仯
        float pulse = (Mathf.Sin(Time.time * beatSpeed * Mathf.PI * 2) + 1f) / 2f;

        // ���ű仯���������� + ��������
        transform.localScale = baseScale * (1f + beatScale * pulse);

        // ����仯������ʱ����
        if (mat.HasProperty("_EmissionColor"))
        {
            Color emission = baseEmission * (1f + pulse * emissionIntensity);
            mat.SetColor("_EmissionColor", emission);
        }
    }
}
