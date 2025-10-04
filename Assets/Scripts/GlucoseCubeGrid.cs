using UnityEngine;

public class GlucoseCubeGrid : MonoBehaviour
{
    [Header("��Դ����")]
    [SerializeField] private float glucoseAmount;
    [SerializeField] private float MINAMOUNT = 50f;
    [SerializeField] private float MAXAMOUNT = 500f;

    [Header("��������")]
    [SerializeField] private GameObject emptyGridPrefab; // ��ͨ���� prefab



    private void Start()
    {
        glucoseAmount = Random.Range(MINAMOUNT, MAXAMOUNT);
    }

    private void resourceDepleted()
    {
        if (emptyGridPrefab != null)
        {
            // �ڵ�ǰλ�������µ���ͨ����
            GameObject newGrid = Instantiate(
                emptyGridPrefab,
                transform.position,
                transform.rotation,
                transform.parent // ���ָ�����һ��
            );

            // ����ǰ���ӵ�����������Ǩ�Ƶ��¸�����
            // ʹ����ʱ�б����������޸ĸ�����������
            Transform[] children = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                if(transform.GetChild(i).name == "Visual" || transform.GetChild(i).name != "SelectedVisual") continue;
                children[i] = transform.GetChild(i);

            }
            foreach (Transform child in children)
            {
                child.SetParent(newGrid.transform, true); // true ������������

            }
        }

        // ɾ����ǰ����
        Destroy(gameObject);
    }

    public void AmountDecrease(float num)
    {
        glucoseAmount -= num;
        if (glucoseAmount <= 0)
        {
            resourceDepleted();
        }
    }
}
