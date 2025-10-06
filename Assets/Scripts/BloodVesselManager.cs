using System.Collections.Generic;
using UnityEngine;

public class BloodVesselManager : MonoBehaviour
{
    public static BloodVesselManager bloodVesselManager;
    

    private List<BloodVessel> allBloodVessels = new List<BloodVessel>();
    public int bloodVesselCount = 0;
    private void Awake()
    {
        bloodVesselManager = this;
    }

    /// <summary>
    /// 注册血管
    /// </summary>
    public void RegisterBloodVessel(BloodVessel vessel)
    {
        if (!allBloodVessels.Contains(vessel))
        {
            allBloodVessels.Add(vessel);
            bloodVesselCount = allBloodVessels.Count;
        }
    }

    /// <summary>
    /// 注销血管
    /// </summary>
    public void UnregisterBloodVessel(BloodVessel vessel)
    {
        if (allBloodVessels.Contains(vessel))
        {
            allBloodVessels.Remove(vessel);
            bloodVesselCount = allBloodVessels.Count;
        }
    }

    /// <summary>
    /// 刷新所有血管连通性
    /// </summary>
    public void RefreshAllConnections()
    {
        foreach (var vessel in allBloodVessels)
        {
            vessel.isConnected = vessel.CheckConnectivityBFS();
            vessel.UpdateMaterial();
        }

        // 刷新所有收集细胞
        GlucoseCollectorManager.Instance.RefreshAllCollectors();
    }

}
