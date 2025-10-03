using System.Collections.Generic;
using UnityEngine;

public class BloodVesselManager : MonoBehaviour
{
    public static BloodVesselManager bloodVesselManager;

    private List<BloodVessel> allBloodVessels = new List<BloodVessel>();

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
    }
}
