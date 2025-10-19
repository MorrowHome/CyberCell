using System.Collections.Generic;
using UnityEngine;

public class BloodVesselManager : MonoBehaviour
{
    public static BloodVesselManager bloodVesselManager;

    [Header("扩散参数")]
    [SerializeField] private float diffusionRate = 0.1f; // 扩散速率
    [SerializeField] private float updateInterval = 1f;  // 扩散刷新间隔（秒）
    [SerializeField] private bool autoSimulateDiffusion = true; // 是否自动扩散

    public List<BloodVessel> allBloodVessels = new List<BloodVessel>();
    private float timer = 0f;

    public int bloodVesselCount => allBloodVessels.Count;

    private void Awake()
    {
        bloodVesselManager = this;
    }

    private void Update()
    {
        if (!autoSimulateDiffusion) return;
        if (GameManager.Instance.CurrentTurn != GameManager.TurnType.DefenseTime) return;

        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            SimulateDiffusion();
        }
    }

    public void RegisterBloodVessel(BloodVessel vessel)
    {
        if (!allBloodVessels.Contains(vessel))
            allBloodVessels.Add(vessel);
    }

    public void UnregisterBloodVessel(BloodVessel vessel)
    {
        if (allBloodVessels.Contains(vessel))
            allBloodVessels.Remove(vessel);
    }

    /// <summary>
    /// 从心脏 BFS 更新血管连接状态
    /// </summary>
    public void RefreshAllConnections()
    {
        // 初始化
        foreach (var vessel in allBloodVessels)
        {
            vessel.isConnected = false;
            vessel.distanceFromHeart = -1; // 重置层级
        }

        if (MapGenerator.Instance.heartCellTransform == null) return;

        CubeGrid heartCube = MapGenerator.Instance.heartCellTransform.parent.GetComponent<CubeGrid>();
        if (heartCube == null) return;

        BloodVessel startVessel = heartCube.whatIsOnMe.GetComponent<BloodVessel>();
        if (startVessel == null) return;

        Queue<BloodVessel> queue = new Queue<BloodVessel>();
        HashSet<BloodVessel> visited = new HashSet<BloodVessel>();

        startVessel.distanceFromHeart = 0; // 心脏血管层级为0
        queue.Enqueue(startVessel);
        visited.Add(startVessel);

        while (queue.Count > 0)
        {
            BloodVessel current = queue.Dequeue();
            current.isConnected = true;

            Vector3 pos = MapGenerator.Instance.Transform_Vector3_Dictionary[current.transform.parent];
            Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

            foreach (var dir in directions)
            {
                Vector3 neighborPos = pos + dir;
                if (MapGenerator.Instance.Vector3_Transform_Dictionary.TryGetValue(neighborPos, out Transform neighborGrid))
                {
                    var neighborObj = neighborGrid.GetComponent<CubeGrid>().whatIsOnMe;
                    if (neighborObj != null)
                    {
                        var neighborVessel = neighborObj.GetComponent<BloodVessel>();
                        if (neighborVessel != null && !visited.Contains(neighborVessel))
                        {
                            visited.Add(neighborVessel);
                            neighborVessel.distanceFromHeart = current.distanceFromHeart + 1; // 分配层级
                            queue.Enqueue(neighborVessel);
                        }
                    }
                }
            }
        }

        // 更新材质和方向
        foreach (var vessel in allBloodVessels)
        {
            vessel.UpdateMaterial();
            vessel.UpdateVesselTypeAndDirection();
        }

        GlucoseCollectorManager.Instance.RefreshAllCollectors();
    }


    /// <summary>
    /// 模拟血管葡萄糖扩散
    /// </summary>
    private void SimulateDiffusion()
    {
        Dictionary<BloodVessel, float> nextGlucose = new Dictionary<BloodVessel, float>();

        foreach (var vessel in allBloodVessels)
        {
            float delta = 0f;
            Vector3 pos = MapGenerator.Instance.Transform_Vector3_Dictionary[vessel.transform.parent];
            Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

            foreach (var dir in directions)
            {
                Vector3 neighborPos = pos + dir;
                if (MapGenerator.Instance.Vector3_Transform_Dictionary.TryGetValue(neighborPos, out Transform neighborGrid))
                {
                    var neighborObj = neighborGrid.GetComponent<CubeGrid>().whatIsOnMe;
                    if (neighborObj != null)
                    {
                        var neighborVessel = neighborObj.GetComponent<BloodVessel>();
                        if (neighborVessel != null)
                        {
                            delta += diffusionRate * (neighborVessel.GlucoseAmount - vessel.GlucoseAmount);
                        }
                    }
                }
            }

            nextGlucose[vessel] = vessel.GlucoseAmount + delta;
        }

        foreach (var kvp in nextGlucose)
            kvp.Key.SetGlucoseAmount(kvp.Value);
    }
}
