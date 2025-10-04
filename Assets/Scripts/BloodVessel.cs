using System.Collections.Generic;
using UnityEngine;

public class BloodVessel : MonoBehaviour, IActionPointCost
{
    [SerializeField] private Material connected;
    [SerializeField] private Material disconnected;
    [SerializeField] private GameObject myVisual;
    [SerializeField] public int actionPointCost = 1;
    public int ActionPointCost => actionPointCost;

    private Transform parentCubeGrid;
    private Vector3 positionVector3;

    public bool isConnected = false;

    private void Awake()
    {
        // 确定自己属于哪个 CubeGrid
        if (parentCubeGrid == null)
            parentCubeGrid = transform.parent;

        // 拿到自己在地图中的坐标
        if (!MapGenerator.Instance.Transform_Vector3_Dictionary.TryGetValue(parentCubeGrid, out positionVector3))
        {
            Debug.LogError("BloodVessel 找不到对应的 CubeGrid 坐标！");
        }
    }

    /// <summary>
    /// 初始化血管（放置时调用）
    /// </summary>
    public void Init()
    {
        BloodVesselManager.bloodVesselManager.RegisterBloodVessel(this);
        isConnected = CheckConnectivityBFS();
        UpdateMaterial();
    }

    private void OnDestroy()
    {
        BloodVesselManager.bloodVesselManager.UnregisterBloodVessel(this);
        BloodVesselManager.bloodVesselManager.RefreshAllConnections();
    }

    public void UpdateMaterial()
    {
        if (myVisual != null)
        {
            MeshRenderer renderer = myVisual.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = isConnected ? connected : disconnected;
            }
        }
    }

    /// <summary>
    /// BFS 判断是否连通心脏
    /// </summary>
    public bool CheckConnectivityBFS()
    {
        Queue<Transform> queue = new Queue<Transform>();
        HashSet<Transform> visited = new HashSet<Transform>();

        queue.Enqueue(parentCubeGrid);
        visited.Add(parentCubeGrid);

        while (queue.Count > 0)
        {
            Transform currentGrid = queue.Dequeue();
            CubeGrid cubeGrid = currentGrid.GetComponent<CubeGrid>();
            if (cubeGrid == null) continue;

            Transform objOnMe = cubeGrid.whatIsOnMe;
            if (objOnMe == null) continue;

            if (objOnMe.GetComponent<HeartCell>())
                return true;

            BloodVessel vessel = objOnMe.GetComponent<BloodVessel>();
            if (vessel != null)
            {
                Vector3 gridPos = MapGenerator.Instance.Transform_Vector3_Dictionary[currentGrid];
                Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
                foreach (Vector3 dir in directions)
                {
                    Vector3 neighborPos = gridPos + dir;
                    if (MapGenerator.Instance.Vector3_Transform_Dictionary.TryGetValue(neighborPos, out Transform neighborGrid))
                    {
                        if (!visited.Contains(neighborGrid))
                        {
                            visited.Add(neighborGrid);
                            queue.Enqueue(neighborGrid);
                        }
                    }
                }
            }
        }

        return false;
    }
}
