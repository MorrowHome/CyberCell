using UnityEngine;

public class HeartCell : MonoBehaviour
{
    [Header("��������")]
    [SerializeField] private float beatSpeed = 2f;     // ����Ƶ��
    [SerializeField] private float beatStrength = 0.2f; // ���ŷ���
    [SerializeField] private Transform heartCell;       // ����������

    private Vector3 originalScale;
    private float startTime; // ÿ��ʵ���Ķ�����ʼʱ��

    void Start()
    {
        // ���û���ֶ���ק��������ʱ�Զ�����
        if (heartCell == null)
            heartCell = transform.Find("HeartCell");

        if (heartCell == null)
        {
            Debug.LogError("û���ҵ������� HeartCell��");
            return;
        }

        // ��¼ԭʼ����
        originalScale = heartCell.localScale;

        // ��¼���������ʼʱ��
        startTime = Time.time;
    }

    void Update()
    {
        if (heartCell == null) return;

        // ʹ�����ʱ�䣨ʵ������ʼ��ʱ��
        float elapsed = Time.time - startTime;

        // ͨ�����Ҳ�ģ����������
        float scaleFactor = 1 + Mathf.Sin(elapsed * beatSpeed) * beatStrength;

        // Ӧ������
        heartCell.localScale = originalScale * scaleFactor;
    }
}
