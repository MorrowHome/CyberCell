using System.Collections.Generic;
using UnityEngine;

public class BloodVesselManager : MonoBehaviour
{
    public static BloodVesselManager bloodVesselManager;

    [Header("扩散参数")]
    [SerializeField] private float diffusionRate = 0.1f; // 扩散速率
    [SerializeField] private float updateInterval = 1f;  // 扩散刷新间隔（秒）

    private List<BloodVessel> allBloodVessels = new List<BloodVessel>();
    private float timer = 0f;

    public int bloodVesselCount = 0;

    private void Awake()
    {
        bloodVesselManager = this;
    }

    private void Update()
    {
        if(GameManager.Instance.CurrentTurn == GameManager.TurnType.DefenseTime)
        {
            timer += Time.deltaTime;
            if (timer >= updateInterval)
            {
                timer = 0f;
                SimulateDiffusion();
            }
        }
        
    }

    public void RegisterBloodVessel(BloodVessel vessel)
    {
        if (!allBloodVessels.Contains(vessel))
        {
            allBloodVessels.Add(vessel);
            bloodVesselCount = allBloodVessels.Count;
        }
    }

    public void UnregisterBloodVessel(BloodVessel vessel)
    {
        if (allBloodVessels.Contains(vessel))
        {
            allBloodVessels.Remove(vessel);
            bloodVesselCount = allBloodVessels.Count;
        }
    }

    public void RefreshAllConnections()
    {
        foreach (var vessel in allBloodVessels)
        {
            vessel.isConnected = vessel.CheckConnectivityBFS();
            vessel.UpdateMaterial();
        }

        GlucoseCollectorManager.Instance.RefreshAllCollectors();
    }

    /// <summary>
    /// 模拟血管之间的葡萄糖扩散
    /// </summary>
    private void SimulateDiffusion()
    {
        // 先创建一个字典保存下一帧的浓度变化
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
                            // 扩散 = 扩散速率 × (邻居浓度 - 自身浓度)
                            delta += diffusionRate * (neighborVessel.GlucoseAmount - vessel.GlucoseAmount);
                        }
                    }
                }
            }

            nextGlucose[vessel] = vessel.GlucoseAmount + delta;
        }

        // 统一更新浓度
        foreach (var kvp in nextGlucose)
        {
            kvp.Key.SetGlucoseAmount(kvp.Value);
        }
    }
}
