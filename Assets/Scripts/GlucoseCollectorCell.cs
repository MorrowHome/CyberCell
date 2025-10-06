using UnityEngine;

public class GlucoseCollectorCell : MonoBehaviour, IActionPointCost
{
    [Header("References & tags")]
    [SerializeField] private Transform parentCubeGrid;
    [SerializeField] private BreathVisual breathVisual;
    [SerializeField] private string glucoseCubeGridTag = "GlucoseCubeGrid";

    [Header("Collection rates")]
    [SerializeField] private float baseGlucoseCollectedPerSecond = 1.0f; // 固定基础速率，不在运行时修改
    [SerializeField] public int actionPointCost = 3;
    [SerializeField] private float glucoseConsumptionPerSecond = 0f; // 目前没用，保留

    [Header("Glucose concentration thresholds")]
    [SerializeField] private float minGlucoseConcentration = 0f;
    [SerializeField] private float bestGlucoseConcentration = 500f;
    [SerializeField] private float maxGlucoseConcentration = 100000f;

    

    private bool isConnected = false;   // 是否连通到心脏
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
    /// 由管理器或血管刷新时调用，更新是否连通
    /// </summary>
    public void RefreshConnection()
    {
        isConnected = CheckConnectionToBloodVessel();
        if (breathVisual != null) breathVisual.isActive = isConnected;

        // 如果断开连接，清除缓存的 grid 引用
        if (!isConnected) cachedGlucoseGrid = null;
    }

    void Update()
    {
        // 计算活力因子（只读，不修改基础速率）
        CalculateVitalityFactor();

        // 只有连通且父格属于 glucose cube grid 时才持续收集
        if (!isConnected || parentCubeGrid == null || parentCubeGrid.tag != glucoseCubeGridTag || GameManager.Instance.CurrentTurn != GameManager.TurnType.DefenseTime) return;

        // 缓存 GlucoseCubeGrid，父格可能被替换（比如资源耗尽时）
        if (cachedGlucoseGrid == null || cachedGlucoseGrid.transform != parentCubeGrid)
            cachedGlucoseGrid = parentCubeGrid.GetComponent<GlucoseCubeGrid>();

        if (cachedGlucoseGrid == null) return;

        // 计算本帧应采集量（不会修改基础速率）
        float collected = baseGlucoseCollectedPerSecond * vitalityFactor * Time.deltaTime;
        cachedGlucoseGrid.AmountDecrease(collected);

        if (GameManager.Instance != null)
            GameManager.Instance.glucoseAmount += collected;
    }

    /// <summary>
    /// 检查周围是否有血管，并且该血管连通心脏
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

        // 保持你原来的等级逻辑，只是改了函数名称并确保不会改基础速率
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
