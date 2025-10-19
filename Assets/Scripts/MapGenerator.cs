using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance;

    [Header("地图参数")]
    [SerializeField] public int COLS = 20;
    [SerializeField] public int ROWS = 20;
    [SerializeField] private float sizeScale = 5f;

    [Header("格子Prefab")]
    [SerializeField] private GameObject cubeGridPrefab;              // 普通格
    [SerializeField] private GameObject cubeGridWithResourcesPrefab; // 菌落格

    [Header("菌落蔓延参数")]
    [SerializeField] private int edgeStartPoints = 4;   // 边缘起点数量
    [SerializeField, Range(0f, 1f)] private float growChance = 0.85f;    // 蔓延基础概率
    [SerializeField, Range(0f, 1f)] private float branchChance = 0.25f;  // 分支概率
    [SerializeField, Range(0f, 1f)] private float inwardBias = 0.6f;     // 越大越倾向向中心扩展
    [SerializeField] private int maxTotalSteps = 2000;  // 最大蔓延步数，控制范围
    [SerializeField] private int maxIdleStepsBeforeSeedInterior = 150; // 如果长时间没有新格子，种一个内部种子

    [Header("辅助显示")]
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Transform lineContainer;
    [SerializeField] private GameObject heartCellPrefab;
    [SerializeField] private Vector3 offset = new Vector3(0.5f, 0.5f, 0.5f);

    [SerializeField] private GameObject squareGrid2DPrefab;
    [SerializeField] private Transform cubeGridContainer;
    [SerializeField] private Transform squareGrid2DContainer;
    [SerializeField] private bool drawLines = false;

    // ---------------- 数据结构 ----------------
    public Dictionary<Vector3, Transform> Vector3_Transform_Dictionary = new Dictionary<Vector3, Transform>();
    public Dictionary<Transform, Vector3> Transform_Vector3_Dictionary = new Dictionary<Transform, Vector3>();
    public List<Transform> allGrids = new List<Transform>();

    private int ID = 0;
    private bool[,] resourceMap2D;  // 菌落格标记
    public Transform heartCellTransform;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        resourceMap2D = new bool[COLS, ROWS];
        GenerateColonyMap();
        GenerateMap();

        if (drawLines)
            DrawGridLines();

        GenerateHeartCell();
    }

    // ================= 菌落式蔓延（改进版） =================
    private void GenerateColonyMap()
    {
        List<Vector2Int> frontier = new List<Vector2Int>();
        Vector2Int center = new Vector2Int(COLS / 2, ROWS / 2);

        // 1) 多边缘起点（仍以边缘为主，但会朝中心扩展）
        for (int i = 0; i < edgeStartPoints; i++)
        {
            Vector2Int start = GetRandomEdgePoint();
            if (!resourceMap2D[start.x, start.y])
            {
                resourceMap2D[start.x, start.y] = true;
                frontier.Add(start);
            }
        }

        int steps = 0;
        int idleSinceLastAdd = 0; // 连续未成功添加的步数

        while (frontier.Count > 0 && steps < maxTotalSteps)
        {
            // 随机选一个前沿点来扩展
            int idx = Random.Range(0, frontier.Count);
            Vector2Int current = frontier[idx];
            frontier.RemoveAt(idx);

            // 计算到中心的方向（用于 inward bias）
            Vector2 toCenter = new Vector2(center.x - current.x, center.y - current.y);
            if (toCenter.magnitude > 0.001f)
                toCenter.Normalize();
            else
                toCenter = Vector2.zero;

            // 收集合法邻居并计算权重
            List<Vector2Int> candidates = new List<Vector2Int>();
            List<float> weights = new List<float>();

            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var d in dirs)
            {
                Vector2Int n = current + d;
                if (n.x < 0 || n.x >= COLS || n.y < 0 || n.y >= ROWS) continue;
                if (resourceMap2D[n.x, n.y]) continue;

                // 基础权重 (基于 growChance)
                float baseWeight = growChance;

                // inward bias：如果这个方向朝向中心，增加权重
                Vector2 dirVec = new Vector2(d.x, d.y);
                float dot = Vector2.Dot(dirVec.normalized, toCenter); // -1..1
                float inwardFactor = Mathf.Clamp01((dot + 1f) / 2f); // 0..1
                baseWeight *= 1f + inwardBias * inwardFactor;

                // 周围资源惩罚：如果邻格周围已有资源多，降低权重（避免簇）
                int adjacentResources = CountAdjacentResources(n);
                float adjacencyPenalty = 0.2f * adjacentResources; // 每个已占邻格降低20%
                baseWeight *= Mathf.Max(0.05f, 1f - adjacencyPenalty);

                // 该位置距离已有资源较远时适度增加权重，鼓励填充空洞
                float distToNearestRes = DistanceToNearestResource(n);
                baseWeight *= 1f + Mathf.Clamp(distToNearestRes / Mathf.Max(COLS, ROWS), 0f, 0.8f);

                if (baseWeight > 0.001f)
                {
                    candidates.Add(n);
                    weights.Add(baseWeight);
                }
            }

            bool addedThisIteration = false;
            if (candidates.Count > 0)
            {
                // 用权重选择一个邻居
                Vector2Int chosen = PickWeighted(candidates, weights);

                // 再用一个随机门槛决定是否真正生长（增加随机性）
                if (Random.value <= 1.0f) // 这里保留 100% 的机会，因为权重包含 growChance
                {
                    resourceMap2D[chosen.x, chosen.y] = true;
                    frontier.Add(chosen);
                    addedThisIteration = true;

                    // 分支：以 branchChance 决定是否把其他高权重邻格也加入前沿，形成分叉
                    for (int i = 0; i < candidates.Count; i++)
                    {
                        if (candidates[i] == chosen) continue;
                        if (Random.value < branchChance * (weights[i] / Sum(weights))) // 权重越高更可能分支
                        {
                            Vector2Int extra = candidates[i];
                            if (!resourceMap2D[extra.x, extra.y])
                            {
                                resourceMap2D[extra.x, extra.y] = true;
                                frontier.Add(extra);
                            }
                        }
                    }
                }
            }

            // 统计
            if (addedThisIteration)
                idleSinceLastAdd = 0;
            else
                idleSinceLastAdd++;

            steps++;

            // 如果长时间没有成功添加（可能被局部围住），在地图最空旷的位置种一个内向种子
            if (idleSinceLastAdd >= maxIdleStepsBeforeSeedInterior)
            {
                Vector2Int far = FindFarthestEmptyCellFromResources();
                if (far.x >= 0)
                {
                    resourceMap2D[far.x, far.y] = true;
                    frontier.Add(far);
                }
                idleSinceLastAdd = 0;
            }

            // 若 frontier 过少但地图仍有很多空格，也主动从边缘/内部放种子（提高覆盖率）
            if (frontier.Count == 0 && !AllFilledOrReachedLimit())
            {
                Vector2Int extra = GetRandomEdgePoint();
                if (!resourceMap2D[extra.x, extra.y])
                {
                    resourceMap2D[extra.x, extra.y] = true;
                    frontier.Add(extra);
                }
            }
        }
    }

    // 计算某格周围已有资源格数量（4邻域）
    private int CountAdjacentResources(Vector2Int pos)
    {
        int cnt = 0;
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var d in dirs)
        {
            Vector2Int n = pos + d;
            if (n.x < 0 || n.x >= COLS || n.y < 0 || n.y >= ROWS) continue;
            if (resourceMap2D[n.x, n.y]) cnt++;
        }
        return cnt;
    }

    // 计算到最近资源的曼哈顿距离（用于鼓励填充空洞）
    private float DistanceToNearestResource(Vector2Int pos)
    {
        float best = float.MaxValue;
        for (int x = 0; x < COLS; x++)
        {
            for (int y = 0; y < ROWS; y++)
            {
                if (resourceMap2D[x, y])
                {
                    float d = Mathf.Abs(x - pos.x) + Mathf.Abs(y - pos.y);
                    if (d < best) best = d;
                }
            }
        }
        if (best == float.MaxValue) return Mathf.Max(COLS, ROWS); // 当前没有资源时返回远值
        return best;
    }

    // 查找对现有资源最远的空白格（作为 interior seed）
    private Vector2Int FindFarthestEmptyCellFromResources()
    {
        Vector2Int best = new Vector2Int(-1, -1);
        float bestDist = -1f;

        for (int x = 0; x < COLS; x++)
        {
            for (int y = 0; y < ROWS; y++)
            {
                if (resourceMap2D[x, y]) continue;
                float d = DistanceToNearestResource(new Vector2Int(x, y));
                if (d > bestDist)
                {
                    bestDist = d;
                    best = new Vector2Int(x, y);
                }
            }
        }
        return best;
    }

    // 如果地图几乎填满或达到步数上限，则认为完成
    private bool AllFilledOrReachedLimit()
    {
        for (int x = 0; x < COLS; x++)
            for (int y = 0; y < ROWS; y++)
                if (!resourceMap2D[x, y]) return false;
        return true;
    }

    private Vector2Int GetRandomEdgePoint()
    {
        int edge = Random.Range(0, 4); // 0=左,1=右,2=下,3=上
        switch (edge)
        {
            case 0: return new Vector2Int(0, Random.Range(0, ROWS));
            case 1: return new Vector2Int(COLS - 1, Random.Range(0, ROWS));
            case 2: return new Vector2Int(Random.Range(0, COLS), 0);
            default: return new Vector2Int(Random.Range(0, COLS), ROWS - 1);
        }
    }

    // 加权选择
    private Vector2Int PickWeighted(List<Vector2Int> items, List<float> weights)
    {
        float sum = 0f;
        for (int i = 0; i < weights.Count; i++) sum += Mathf.Max(0f, weights[i]);
        if (sum <= 0.0001f) return items[0];

        float r = Random.value * sum;
        float acc = 0f;
        for (int i = 0; i < items.Count; i++)
        {
            acc += Mathf.Max(0f, weights[i]);
            if (r <= acc) return items[i];
        }
        return items[items.Count - 1];
    }

    private float Sum(List<float> arr)
    {
        float s = 0f;
        for (int i = 0; i < arr.Count; i++) s += arr[i];
        return s;
    }

    // ---------------- 生成格子 ----------------
    private void GenerateMap()
    {
        for (int x = 0; x < COLS; x++)
        {
            for (int z = 0; z < ROWS; z++)
            {
                bool isResource = resourceMap2D[x, z];

                Vector3 position = new Vector3(x * sizeScale, 0, z * sizeScale);
                GameObject prefabToUse = isResource ? cubeGridWithResourcesPrefab : cubeGridPrefab;

                GameObject ins = Instantiate(prefabToUse, position, Quaternion.identity, cubeGridContainer);

                if (squareGrid2DPrefab != null)
                    Instantiate(squareGrid2DPrefab, position, Quaternion.identity, squareGrid2DContainer);

                allGrids.Add(ins.transform);

                Vector3 gridVector = new Vector3(x, 0, z);
                Vector3_Transform_Dictionary.Add(gridVector, ins.transform);
                Transform_Vector3_Dictionary.Add(ins.transform, gridVector);

                ins.name = isResource ? $"ColonyCube_{ID++}" : $"Cube_{ID++}";
            }
        }
    }

    // ---------------- 心脏格生成 ----------------
    private void GenerateHeartCell()
    {
        Vector3 centerPoint = new Vector3((COLS / 2) * sizeScale, 0, (ROWS / 2) * sizeScale);
        Vector3 centerVector3 = new Vector3(COLS / 2, 0, ROWS / 2);

        if (Vector3_Transform_Dictionary.TryGetValue(centerVector3, out Transform cubeGrid))
        {
            GameObject ins = Instantiate(heartCellPrefab, centerPoint, Quaternion.identity, cubeGrid);
            heartCellTransform = ins.transform;

            CubeGrid cubeScript = cubeGrid.GetComponent<CubeGrid>();
            if (cubeScript != null)
                cubeScript.whatIsOnMe = ins.transform;
        }
        else
        {
            Debug.LogWarning("中心格子未找到，心脏格生成失败。");
        }
    }

    // ---------------- 辅助线绘制 ----------------
    private void DrawGridLines()
    {
        if (lineMaterial == null || lineContainer == null) return;

        GameObject lineParent = new GameObject("GridLines");
        lineParent.transform.parent = lineContainer;

        for (int x = 0; x <= COLS; x++)
        {
            DrawLine(new Vector3(x * sizeScale, 0, 0) + offset,
                     new Vector3(x * sizeScale, 0, ROWS * sizeScale) + offset,
                     lineParent.transform);
        }
        for (int z = 0; z <= ROWS; z++)
        {
            DrawLine(new Vector3(0, 0, z * sizeScale) + offset,
                     new Vector3(COLS * sizeScale, 0, z * sizeScale) + offset,
                     lineParent.transform);
        }
    }

    private void DrawLine(Vector3 start, Vector3 end, Transform parent)
    {
        GameObject lineObj = new GameObject("Line");
        lineObj.transform.parent = parent;

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }
}
