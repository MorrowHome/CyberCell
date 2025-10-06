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
    /// ע��Ѫ��
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
    /// ע��Ѫ��
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
    /// ˢ������Ѫ����ͨ��
    /// </summary>
    public void RefreshAllConnections()
    {
        foreach (var vessel in allBloodVessels)
        {
            vessel.isConnected = vessel.CheckConnectivityBFS();
            vessel.UpdateMaterial();
        }

        // ˢ�������ռ�ϸ��
        GlucoseCollectorManager.Instance.RefreshAllCollectors();
    }

}
