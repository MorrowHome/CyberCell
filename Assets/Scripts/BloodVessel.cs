using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BloodVessel : MonoBehaviour, IActionPointCost, IHasHoverInfo
{
    public enum BloodVesselType { Line, Corner, T, Cross }
    public enum BloodVesselDirection
    {
        North, South, East, West,
        NorthEast, NorthWest, SouthEast, SouthWest,
        TNorth, TSouth, TEast, TWest,
        Unknown
    }

    [Header("邻居血管")]
    [HideInInspector] public BloodVessel bloodVesselForward;
    [HideInInspector] public BloodVessel bloodVesselBack;
    [HideInInspector] public BloodVessel bloodVesselLeft;
    [HideInInspector] public BloodVessel bloodVesselRight;
    public List<BloodVessel> neighborBloodVessels = new List<BloodVessel>();
    private Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
    private Dictionary<Vector3, BloodVessel> neighborCache = new Dictionary<Vector3, BloodVessel>();

    [Header("材质与显示")]
    [SerializeField] private Material connected;
    [SerializeField] private Material disconnected;
    [SerializeField] private GameObject myVisual;
    private MeshRenderer visualRenderer;
    private Color originalColor;

    [Header("血管属性")]
    [SerializeField] public int actionPointCost = 1;
    [SerializeField] private float glucoseAmount = 10f;
    [SerializeField] public int distanceFromHeart = 0;
    public float GlucoseAmount => glucoseAmount;
    public void SetGlucoseAmount(float value) => glucoseAmount = Mathf.Max(0f, value);
    public int ActionPointCost => actionPointCost;

    [Header("腐化设置")]
    [SerializeField] private bool isCorrupted = false;
    [SerializeField] private float corruptionDuration = 5f;

    [HideInInspector] public bool isConnected = false;
    [HideInInspector] public BloodVesselType vesselType = BloodVesselType.Line;
    [HideInInspector] public BloodVesselDirection vesselDirection = BloodVesselDirection.Unknown;

    private Transform parentCubeGrid;
    private Vector3 positionVector3;

    // ------------------ Unity 生命周期 ------------------
    private void Awake()
    {
        parentCubeGrid = transform.parent;
        visualRenderer = myVisual != null ? myVisual.GetComponent<MeshRenderer>() : null;
        if (visualRenderer != null) originalColor = connected.color;
    }

    private void Start()
    {
        if (!MapGenerator.Instance.Transform_Vector3_Dictionary.TryGetValue(parentCubeGrid, out positionVector3))
            Debug.LogError("BloodVessel 找不到对应的 CubeGrid 坐标！");

        Init();
    }

    private void OnDestroy()
    {
        BloodVesselManager.bloodVesselManager.UnregisterBloodVessel(this);
    }

    // ------------------ 初始化 ------------------
    public void Init()
    {
        BloodVesselManager.bloodVesselManager.RegisterBloodVessel(this);
        UpdateVesselTypeAndDirection();
        UpdateMaterial();
    }

    // ------------------ 材质刷新 ------------------
    public void UpdateMaterial()
    {
        if (visualRenderer != null)
            visualRenderer.material = isConnected ? connected : disconnected;
    }

    // ------------------ 血管类型与方向 ------------------
    public void UpdateVesselTypeAndDirection()
    {
        if (parentCubeGrid == null) return;
        if (!MapGenerator.Instance.Transform_Vector3_Dictionary.TryGetValue(parentCubeGrid, out Vector3 gridPos)) return;

        bool forward = HasNeighbor(gridPos + Vector3.forward);
        bool back = HasNeighbor(gridPos + Vector3.back);
        bool left = HasNeighbor(gridPos + Vector3.left);
        bool right = HasNeighbor(gridPos + Vector3.right);

        int connectionCount = (forward ? 1 : 0) + (back ? 1 : 0) + (left ? 1 : 0) + (right ? 1 : 0);

        // 类型判断
        switch (connectionCount)
        {
            case 2:
                vesselType = (forward && back) || (left && right) ? BloodVesselType.Line : BloodVesselType.Corner;
                break;
            case 3: vesselType = BloodVesselType.T; break;
            case 4: vesselType = BloodVesselType.Cross; break;
            default: vesselType = BloodVesselType.Line; break;
        }

        // 方向判断
        vesselDirection = GetVesselDirection(forward, back, left, right);

        RefreshNeighborBloodVessels();
    }

    private bool HasNeighbor(Vector3 pos)
    {
        if (MapGenerator.Instance.Vector3_Transform_Dictionary.TryGetValue(pos, out Transform neighborGrid))
        {
            var vessel = neighborGrid.GetComponentInChildren<BloodVessel>();
            return vessel != null;
        }
        return false;
    }

    private BloodVesselDirection GetVesselDirection(bool f, bool b, bool l, bool r)
    {
        if (vesselType == BloodVesselType.Line) return (f && b) ? BloodVesselDirection.North : BloodVesselDirection.East;
        if (vesselType == BloodVesselType.Corner)
        {
            if (f && r) return BloodVesselDirection.NorthEast;
            if (f && l) return BloodVesselDirection.NorthWest;
            if (b && r) return BloodVesselDirection.SouthEast;
            if (b && l) return BloodVesselDirection.SouthWest;
        }
        if (vesselType == BloodVesselType.T)
        {
            if (!f) return BloodVesselDirection.TSouth;
            if (!b) return BloodVesselDirection.TNorth;
            if (!l) return BloodVesselDirection.TEast;
            if (!r) return BloodVesselDirection.TWest;
        }
        return BloodVesselDirection.Unknown;
    }

    // ------------------ 邻居刷新 ------------------
    private void RefreshNeighborBloodVessels()
    {
        if (parentCubeGrid == null) return;
        if (!MapGenerator.Instance.Transform_Vector3_Dictionary.TryGetValue(parentCubeGrid, out Vector3 myPos)) return;

        neighborBloodVessels.Clear();

        foreach (var dir in directions)
        {
            Vector3 neighborPos = myPos + dir;
            if (neighborCache.TryGetValue(neighborPos, out var vessel) && vessel != null)
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

        bloodVesselForward = GetVesselInDirection(Vector3.forward, myPos);
        bloodVesselBack = GetVesselInDirection(Vector3.back, myPos);
        bloodVesselLeft = GetVesselInDirection(Vector3.left, myPos);
        bloodVesselRight = GetVesselInDirection(Vector3.right, myPos);
    }

    private BloodVessel GetVesselInDirection(Vector3 dir, Vector3 myPos)
    {
        neighborCache.TryGetValue(myPos + dir, out var vessel);
        return vessel;
    }

    // ------------------ 腐化 ------------------
    public void ApplyCorruption(float amount = 5f)
    {
        glucoseAmount = Mathf.Max(0f, glucoseAmount - amount);
        isCorrupted = true;

        if (visualRenderer != null)
            visualRenderer.material.color = Color.black;

        StartCoroutine(CorruptionRoutine());
    }

    private IEnumerator CorruptionRoutine()
    {
        float timer = corruptionDuration;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        RestoreFromCorruption();
    }

    private void RestoreFromCorruption()
    {
        isCorrupted = false;
        UpdateMaterial();
        if (visualRenderer != null)
            visualRenderer.material.color = originalColor;
    }

    // ------------------ Hover 信息 ------------------
    public string HoverInfoTitle => "BloodVessel";
    public string HoverInfoContent =>
        $"Glucose: {GlucoseAmount:F2}\nConnect: {(isConnected ? "True" : "False")}";
}
