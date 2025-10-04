using UnityEngine;

public class GlucoseCollectorCell : MonoBehaviour, IActionPointCost
{
    [SerializeField] private Transform parentCubeGrid;
    [SerializeField] private BreathVisual breathVisual;
    [SerializeField] private string glucoseCubeGridTag = "GlucoseCubeGrid";
    [SerializeField] private float glucoseCollectedPerSecond = 1.0f;
    [SerializeField] public int actionPointCost = 3;

    private bool isConnected = false;   // �Ƿ���ͨ������
    public int ActionPointCost => actionPointCost;

    void Start()
    {
        parentCubeGrid = transform.parent;
        breathVisual = GetComponent<BreathVisual>();

        // ע���Լ�
        GlucoseCollectorManager.Instance.RegisterCollector(this);

        // ��ʼˢ��һ��
        RefreshConnection();
    }

    private void OnDestroy()
    {
        GlucoseCollectorManager.Instance.UnregisterCollector(this);
    }

    /// <summary>
    /// �ɹ�������Ѫ��ˢ��ʱ���ã������Ƿ���ͨ
    /// </summary>
    public void RefreshConnection()
    {
        isConnected = CheckConnectionToBloodVessel();
        breathVisual.isActive = isConnected;
    }

    void Update()
    {
        // ֻ����ͨʱ�ų����ռ�
        if (isConnected && parentCubeGrid != null && parentCubeGrid.tag == glucoseCubeGridTag)
        {
            GlucoseCubeGrid glucoseCubeGrid = parentCubeGrid.GetComponent<GlucoseCubeGrid>();
            if (glucoseCubeGrid != null)
            {
                float collected = glucoseCollectedPerSecond * Time.deltaTime;
                glucoseCubeGrid.AmountDecrease(collected);
                GameManager.Instance.glucoseAmount += collected;
            }
        }
    }

    /// <summary>
    /// �����Χ�Ƿ���Ѫ�ܣ����Ҹ�Ѫ����ͨ����
    /// </summary>
    private bool CheckConnectionToBloodVessel()
    {
        if (parentCubeGrid == null) return false;

        if (!MapGenerator.Instance.Transform_Vector3_Dictionary.TryGetValue(parentCubeGrid, out Vector3 myPos))
            return false;

        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

        foreach (var dir in directions)
        {
            Vector3 neighborPos = myPos + dir;

            if (MapGenerator.Instance.Vector3_Transform_Dictionary.TryGetValue(neighborPos, out Transform neighborGrid))
            {
                CubeGrid cubeGrid = neighborGrid.GetComponent<CubeGrid>();
                if (cubeGrid == null || cubeGrid.whatIsOnMe == null) continue;

                BloodVessel vessel = cubeGrid.whatIsOnMe.GetComponent<BloodVessel>();
                if (vessel != null && vessel.isConnected)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
