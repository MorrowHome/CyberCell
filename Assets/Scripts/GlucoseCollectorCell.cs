using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 葡萄糖采集细胞
/// </summary>
public class GlucoseCollectorCell : MonoBehaviour, IActionPointCost
{
    [Header("References & Tags")]
    [SerializeField] private Transform parentCubeGrid;
    [SerializeField] private BreathVisual breathVisual;
    [SerializeField] private string glucoseCubeGridTag = "GlucoseCubeGrid";

    [Header("Collection Rates")]
    [SerializeField] private float baseGlucoseCollectedPerSecond = 1.0f;
    [SerializeField] private int actionPointCost = 3;
    [SerializeField] private float glucoseConsumptionPerSecond = 0f;

    [Header("Glucose Concentration Thresholds")]
    [SerializeField] private float minGlucoseConcentration = 0f;
    [SerializeField] private float bestGlucoseConcentration = 500f;
    [SerializeField] private float maxGlucoseConcentration = 100000f;

    [Header("Neighbor Blood Vessels")]
    [SerializeField] private BloodVessel bloodVesselForward = null;
    [SerializeField] private BloodVessel bloodVesselBack = null;
    [SerializeField] private BloodVessel bloodVesselLeft = null;
    [SerializeField] private BloodVessel bloodVesselRight = null;

    private List<BloodVessel> neighborBloodVessels = new List<BloodVessel>();
    private bool isConnected = false;
    private float vitalityFactor = 1f;
    private GlucoseCubeGrid cachedGlucoseGrid;

    private Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
    private Dictionary<Vector3, BloodVessel> neighborCache = new Dictionary<Vector3, BloodVessel>();

    public int ActionPointCost => actionPointCost;

    private void Awake()
    {
        if (parentCubeGrid == null)
            parentCubeGrid = transform.parent;

        if (breathVisual == null)
            breathVisual = GetComponentInChildren<BreathVisual>();

        neighborBloodVessels.AddRange(new[] { bloodVesselForward, bloodVesselBack, bloodVesselLeft, bloodVesselRight });
    }

    private void Start()
    {
        GlucoseCollectorManager.Instance?.RegisterCollector(this);
        RefreshConnection();
    }

    private void Update()
    {
        CalculateVitalityFactor();

        if (!isConnected || parentCubeGrid == null || parentCubeGrid.tag != glucoseCubeGridTag || GameManager.Instance.CurrentTurn != GameManager.TurnType.DefenseTime)
            return;

        CacheGlucoseGrid();

        if (cachedGlucoseGrid == null)
            return;

        CollectGlucose();
    }

    #region Connection & Neighbor Handling

    public void RefreshConnection()
    {
        bool prevConnection = isConnected;
        isConnected = CheckConnectionToBloodVessel();

        if (isConnected)
        {
            RefreshNeighborBloodVessels();
        }
        else
        {
            neighborBloodVessels.Clear();
            neighborCache.Clear();
            cachedGlucoseGrid = null;
        }

        if (breathVisual != null)
            breathVisual.isActive = isConnected;

        // 仅在连接状态变化时做额外操作（如视觉效果）
        if (prevConnection != isConnected)
        {
            // 可以在这里加额外逻辑，比如断开连接播放动画
        }
    }

    private bool CheckConnectionToBloodVessel()
    {
        if (parentCubeGrid == null || MapGenerator.Instance == null)
            return false;

        if (!MapGenerator.Instance.Transform_Vector3_Dictionary.TryGetValue(parentCubeGrid, out Vector3 myPos))
            return false;

        foreach (var dir in directions)
        {
            Vector3 neighborPos = myPos + dir;

            if (neighborCache.TryGetValue(neighborPos, out var cachedVessel) && cachedVessel != null && cachedVessel.isConnected)
                return true;

            if (MapGenerator.Instance.Vector3_Transform_Dictionary.TryGetValue(neighborPos, out Transform neighborGrid))
            {
                BloodVessel vessel = neighborGrid.GetComponentInChildren<BloodVessel>();
                neighborCache[neighborPos] = vessel;

                if (vessel != null && vessel.isConnected)
                    return true;
            }
        }

        return false;
    }

    private void RefreshNeighborBloodVessels()
    {
        if (parentCubeGrid == null || MapGenerator.Instance == null)
            return;

        if (!MapGenerator.Instance.Transform_Vector3_Dictionary.TryGetValue(parentCubeGrid, out Vector3 myPos))
            return;

        neighborBloodVessels.Clear();

        foreach (var dir in directions)
        {
            Vector3 neighborPos = myPos + dir;

            BloodVessel vessel = null;
            if (neighborCache.TryGetValue(neighborPos, out vessel) && vessel != null)
            {
                neighborBloodVessels.Add(vessel);
                continue;
            }

            if (MapGenerator.Instance.Vector3_Transform_Dictionary.TryGetValue(neighborPos, out Transform neighborGrid))
            {
                vessel = neighborGrid.GetComponentInChildren<BloodVessel>();
                if (vessel != null)
                {
                    neighborBloodVessels.Add(vessel);
                    neighborCache[neighborPos] = vessel;
                }
            }
        }

        // 更新方向引用（方便编辑器显示）
        bloodVesselForward = GetVesselInDirection(Vector3.forward, myPos);
        bloodVesselBack = GetVesselInDirection(Vector3.back, myPos);
        bloodVesselLeft = GetVesselInDirection(Vector3.left, myPos);
        bloodVesselRight = GetVesselInDirection(Vector3.right, myPos);
    }

    private BloodVessel GetVesselInDirection(Vector3 dir, Vector3 myPos)
    {
        Vector3 neighborPos = myPos + dir;
        neighborCache.TryGetValue(neighborPos, out var vessel);
        return vessel;
    }

    #endregion

    #region Glucose Collection

    private void CacheGlucoseGrid()
    {
        if (cachedGlucoseGrid == null || cachedGlucoseGrid.transform != parentCubeGrid)
        {
            cachedGlucoseGrid = parentCubeGrid.GetComponent<GlucoseCubeGrid>();
        }
    }

    private void CollectGlucose()
    {
        float collected = baseGlucoseCollectedPerSecond * vitalityFactor * Time.deltaTime;
        cachedGlucoseGrid.AmountDecrease(collected);

        if (GameManager.Instance != null)
            GameManager.Instance.glucoseAmount += collected;
    }

    private void CalculateVitalityFactor()
    {
        float concentration = GameManager.Instance?.GlucoseConcentration ?? 0f;

        if (concentration <= minGlucoseConcentration || concentration >= maxGlucoseConcentration)
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

    #endregion

    #region OnEnable/OnDisable/OnDestroy

    private void OnEnable()
    {
        GlucoseCollectorManager.Instance?.RegisterCollector(this);
    }

    private void OnDisable()
    {
        GlucoseCollectorManager.Instance?.UnregisterCollector(this);
    }

    private void OnDestroy()
    {
        GlucoseCollectorManager.Instance?.UnregisterCollector(this);
    }

    #endregion
}
