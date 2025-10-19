using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BloodVessel))]
public class BloodVesselVisual : MonoBehaviour
{
    [Header("材质与颜色")]
    [SerializeField] private GameObject myVisual;
    [SerializeField] private float colorMaxGlucose = 100f;
    [SerializeField] private Color lowColor = new Color(0.2f, 0f, 0f);
    [SerializeField] private Color highColor = new Color(0.7f, 0f, 0f);

    [Header("红细胞设置")]
    [SerializeField] private GameObject redBloodCellPrefab;
    [SerializeField] private int maxCellsPerBranch = 20;
    [SerializeField] private float redCellRadius = 0.3f;
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private float flowSpeed = 2f;
    [SerializeField] private float jitterRange = 0.01f;

    [Header("红细胞运动")]
    [SerializeField, Range(0.1f, 10f)] private float branchLengthScale = 1f; // 血管长度缩放，用于红细胞运动


    [Header("分流比例")]
    [SerializeField, Range(0f, 1f)] private float branchSplitRatio = 0.5f; // T型分支分流比例

    [Header("可调偏移（本地）")]
    [SerializeField] private Vector3 startOffset = Vector3.zero;

    private BloodVessel vessel;

    private class Branch
    {
        public Vector3 localStart;
        public Vector3 localEnd;
        public Vector3 direction;
        public float length;
        public List<GameObject> cells = new List<GameObject>();
        public List<float> positions = new List<float>();
        public List<Vector3> offsets = new List<Vector3>();
    }

    private List<Branch> branches = new List<Branch>();

    private void Awake()
    {
        vessel = GetComponent<BloodVessel>();
        if (myVisual == null && vessel != null)
            myVisual = vessel.gameObject;
    }

    private void Start()
    {
        if (BuildManager.Instance != null)
        {
            BuildManager.Instance.OnPlaceSomething -= RefreshFlow;
            BuildManager.Instance.OnPlaceSomething += RefreshFlow;
        }

        InitBranches();
        InitRedBloodCells();
    }

    private void RefreshFlow(object sender, System.EventArgs e)
    {
        RefreshFlow();
    }

    private void Update()
    {
        if (vessel == null) return;
        UpdateColor();
        UpdateRedBloodCells();
        AnimateRedBloodCells();
    }

    private void UpdateColor()
    {
        if (myVisual == null) return;
        var renderer = myVisual.GetComponent<MeshRenderer>();
        if (renderer == null || renderer.material == null) return;

        float t = Mathf.Clamp01(vessel.GlucoseAmount / Mathf.Max(0.0001f, colorMaxGlucose));
        renderer.material.color = Color.Lerp(lowColor, highColor, t);
    }

    private void InitBranches()
    {
        branches.Clear();
        Vector3 center = transform.position;
        List<BloodVessel> neighbors = vessel?.neighborBloodVessels ?? new List<BloodVessel>();
        neighbors = neighbors.FindAll(n => n != null && n.distanceFromHeart > vessel.distanceFromHeart);

        if (neighbors.Count == 0)
        {
            neighbors.Add(vessel); // 末端支路自己
        }

        switch (vessel.vesselType)
        {
            case BloodVessel.BloodVesselType.Line:
                AddBranch(neighbors[0]);
                break;
            case BloodVessel.BloodVesselType.T:
                if (neighbors.Count > 0) AddBranch(neighbors[0]); // 直行
                if (neighbors.Count > 1) AddBranch(neighbors[1], true); // 转弯
                break;
            case BloodVessel.BloodVesselType.Cross:
                if (neighbors.Count > 0) AddBranch(neighbors[0]);
                if (neighbors.Count > 1) AddBranch(neighbors[1], true);
                if (neighbors.Count > 2) AddBranch(neighbors[2], true);
                break;
            default:
                foreach (var n in neighbors) AddBranch(n);
                break;
        }
    }

    private void AddBranch(BloodVessel neighbor, bool isTurn = false)
    {

        Vector3 start = Vector3.zero;
        Vector3 worldDir = neighbor != null ? (neighbor.transform.position - transform.position) : transform.forward;
        Vector3 localDir = transform.InverseTransformDirection(worldDir).normalized;



        if (isTurn)
        {
            // 可以根据 neighbor 的相对位置调整方向
            // 如果本地方向几乎沿 X 轴，旋转 90° 可能不合适
            Vector3 projected = Vector3.ProjectOnPlane(localDir, Vector3.up);
            localDir = Quaternion.AngleAxis(-90f, Vector3.up) * projected;
        }

        Branch b = new Branch
        {
            localStart = start,
            localEnd = start + localDir,
            direction = localDir.normalized,
            length = Mathf.Max(Vector3.Distance(start, start + localDir) * branchLengthScale, 0.0001f)
        };

        branches.Add(b);
    }


    private void InitRedBloodCells()
    {
        if (redBloodCellPrefab == null) return;

        for (int i = 0; i < branches.Count; i++)
        {
            Branch branch = branches[i];
            int count = Mathf.RoundToInt(Mathf.Clamp(vessel != null ? vessel.GlucoseAmount * 0.5f : 5f, 5f, maxCellsPerBranch));

            for (int j = 0; j < count; j++)
            {
                float pos = Random.Range(0f, branch.length);
                CreateCell(branch, pos);
            }
        }
    }

    private void CreateCell(Branch branch, float startPos)
    {
        GameObject cell = Instantiate(redBloodCellPrefab, transform, false);
        branch.cells.Add(cell);
        branch.positions.Add(Mathf.Clamp(startPos, 0f, branch.length));

        Vector3 dir = branch.direction;
        Vector3 right = Vector3.right;
        Vector3 up = Vector3.up;
        Vector3.OrthoNormalize(ref dir, ref right, ref up);

        float angle = Random.Range(0f, Mathf.PI * 2f);
        float radius = Random.Range(0f, redCellRadius);
        Vector3 offset = right * Mathf.Cos(angle) * radius + up * Mathf.Sin(angle) * radius;
        branch.offsets.Add(offset);

        Vector3 localPos = startOffset + branch.localStart + branch.direction * branch.positions[branch.positions.Count - 1] + offset;
        cell.transform.localPosition = localPos;
        cell.transform.rotation = Quaternion.LookRotation(transform.TransformDirection(branch.direction));
    }

    private void UpdateRedBloodCells()
    {
        foreach (var branch in branches)
        {
            int targetCount = Mathf.RoundToInt(Mathf.Clamp(vessel != null ? vessel.GlucoseAmount * 0.5f : 5f, 5f, maxCellsPerBranch));
            while (branch.cells.Count < targetCount)
            {
                float pos = Random.Range(0f, branch.length);
                CreateCell(branch, pos);
            }
            while (branch.cells.Count > targetCount)
            {
                int last = branch.cells.Count - 1;
                if (branch.cells[last] != null) Destroy(branch.cells[last]);
                branch.cells.RemoveAt(last);
                branch.positions.RemoveAt(last);
                branch.offsets.RemoveAt(last);
            }
        }
    }

    private void AnimateRedBloodCells()
    {
        for (int i = 0; i < branches.Count; i++)
        {
            Branch branch = branches[i];

            for (int j = 0; j < branch.cells.Count; j++)
            {
                GameObject cell = branch.cells[j];
                if (cell == null) continue;

                // 红细胞分流逻辑
                float flow = flowSpeed * Time.deltaTime;
                if (vessel.vesselType == BloodVessel.BloodVesselType.T && i > 0)
                {
                    // 转弯支路分流
                    flow *= branchSplitRatio;
                }

                branch.positions[j] += flow;

                if (branch.positions[j] > branch.length)
                    branch.positions[j] -= branch.length;

                Vector3 jitter = new Vector3(Random.Range(-jitterRange, jitterRange),
                                             Random.Range(-jitterRange, jitterRange),
                                             Random.Range(-jitterRange, jitterRange));

                Vector3 localPos = startOffset + branch.localStart + branch.direction * branch.positions[j] + branch.offsets[j] + jitter;
                cell.transform.localPosition = localPos;

                cell.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
            }
        }
    }

    public void RefreshFlow()
    {
        foreach (var branch in branches)
        {
            foreach (var cell in branch.cells)
                if (cell != null) Destroy(cell);
        }
        branches.Clear();
        InitBranches();
        InitRedBloodCells();
    }

    private void OnValidate()
    {
        if (maxCellsPerBranch < 1) maxCellsPerBranch = 1;
        if (redCellRadius < 0f) redCellRadius = 0f;
        if (colorMaxGlucose <= 0f) colorMaxGlucose = 0.0001f;
        if (flowSpeed < 0f) flowSpeed = 0f;
        if (rotationSpeed < 0f) rotationSpeed = 0f;
        if (jitterRange < 0f) jitterRange = 0f;
    }
}
