using UnityEngine;

public class BreathVisual : MonoBehaviour
{
    [Header("��������")]
    [SerializeField] private float beatSpeed = 2f;     // ����Ƶ��
    [SerializeField] private float beatStrength = 0.2f; // ���ŷ���
    [SerializeField] private Transform heartCell;       // ����������
    [SerializeField] private Vector3 minScale = new Vector3(0.7f,0.7f,0.7f);

    private Vector3 originalScale;
    private float startTime; // ÿ��ʵ���Ķ�����ʼʱ��
    public bool isActive = false;

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
        float scaleFactor = 0.7f + Mathf.Sin(elapsed * beatSpeed) * beatStrength;

        // Ӧ������
        if(isActive) heartCell.localScale = originalScale * scaleFactor;
    }
}
