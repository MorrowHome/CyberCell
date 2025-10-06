using UnityEngine;

public class GlucoseCollectorCell : MonoBehaviour, IActionPointCost
{
    [Header("References & tags")]
    [SerializeField] private Transform parentCubeGrid;
    [SerializeField] private BreathVisual breathVisual;
    [SerializeField] private string glucoseCubeGridTag = "GlucoseCubeGrid";

    [Header("Collection rates")]
    [SerializeField] private float baseGlucoseCollectedPerSecond = 1.0f; // �̶��������ʣ���������ʱ�޸�
    [SerializeField] public int actionPointCost = 3;
    [SerializeField] private float glucoseConsumptionPerSecond = 0f; // Ŀǰû�ã�����

    [Header("Glucose concentration thresholds")]
    [SerializeField] private float minGlucoseConcentration = 0f;
    [SerializeField] private float bestGlucoseConcentration = 500f;
    [SerializeField] private float maxGlucoseConcentration = 100000f;

    

    private bool isConnected = false;   // �Ƿ���ͨ������
    public int ActionPointCost => actionPointCost;

    private float vitalityFactor = 1f;
    private GlucoseCubeGrid cachedGlucoseGrid;

    void Start()
    {
        parentCubeGrid = transform.parent;
        if (breathVisual == null) breathVisual = GetComponentInChildren<BreathVisual>();

        if (GlucoseCollectorManager.Instance != null)
            GlucoseCollectorManager.Instance.RegisterCollector(this);

        RefreshConnection();
    }

    private void OnEnable()
    {
        if (GlucoseCollectorManager.Instance != null)
            GlucoseCollectorManager.Instance.RegisterCollector(this);
    }

    private void OnDisable()
    {
        if (GlucoseCollectorManager.Instance != null)
            GlucoseCollectorManager.Instance.UnregisterCollector(this);
    }

    private void OnDestroy()
    {
        if (GlucoseCollectorManager.Instance != null)
            GlucoseCollectorManager.Instance.UnregisterCollector(this);
    }

    /// <summary>
    /// �ɹ�������Ѫ��ˢ��ʱ���ã������Ƿ���ͨ
    /// </summary>
    public void RefreshConnection()
    {
        isConnected = CheckConnectionToBloodVessel();
        if (breathVisual != null) breathVisual.isActive = isConnected;

        // ����Ͽ����ӣ��������� grid ����
        if (!isConnected) cachedGlucoseGrid = null;
    }

    void Update()
    {
        // ����������ӣ�ֻ�������޸Ļ������ʣ�
        CalculateVitalityFactor();

        // ֻ����ͨ�Ҹ������� glucose cube grid ʱ�ų����ռ�
        if (!isConnected || parentCubeGrid == null || parentCubeGrid.tag != glucoseCubeGridTag || GameManager.Instance.CurrentTurn != GameManager.TurnType.DefenseTime) return;

        // ���� GlucoseCubeGrid��������ܱ��滻��������Դ�ľ�ʱ��
        if (cachedGlucoseGrid == null || cachedGlucoseGrid.transform != parentCubeGrid)
            cachedGlucoseGrid = parentCubeGrid.GetComponent<GlucoseCubeGrid>();

        if (cachedGlucoseGrid == null) return;

        // ���㱾֡Ӧ�ɼ����������޸Ļ������ʣ�
        float collected = baseGlucoseCollectedPerSecond * vitalityFactor * Time.deltaTime;
        cachedGlucoseGrid.AmountDecrease(collected);

        if (GameManager.Instance != null)
            GameManager.Instance.glucoseAmount += collected;
    }

    /// <summary>
    /// �����Χ�Ƿ���Ѫ�ܣ����Ҹ�Ѫ����ͨ����
    /// </summary>
    private bool CheckConnectionToBloodVessel()
    {
        if (parentCubeGrid == null) return false;
        if (MapGenerator.Instance == null) return false;

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

    private void CalculateVitalityFactor()
    {
        float concentration = GameManager.Instance != null ? GameManager.Instance.GlucoseConcentration : 0f;

        // ������ԭ���ĵȼ��߼���ֻ�Ǹ��˺������Ʋ�ȷ������Ļ�������
        if (concentration <= minGlucoseConcentration)
        {
            vitalityFactor = 1f;
        }
        else if (concentration >= maxGlucoseConcentration)
        {
            vitalityFactor = 1f;
        }
        else if (concentration >= bestGlucoseConcentration)
        {
            vitalityFactor = 10f;
        }
        else
        {
            vitalityFactor = 5f;
        }
    }
}
