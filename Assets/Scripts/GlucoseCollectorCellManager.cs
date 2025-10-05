using System.Collections.Generic;
using UnityEngine;

public class GlucoseCollectorManager : MonoBehaviour
{
    public static GlucoseCollectorManager Instance;

    private List<GlucoseCollectorCell> allCollectors = new List<GlucoseCollectorCell>();

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterCollector(GlucoseCollectorCell collector)
    {
        if (!allCollectors.Contains(collector))
        {
            allCollectors.Add(collector);
        }
    }

    public void UnregisterCollector(GlucoseCollectorCell collector)
    {
        if (allCollectors.Contains(collector))
        {
            allCollectors.Remove(collector);
        }
    }

    public void RefreshAllCollectors()
    {
        foreach (var collector in allCollectors)
        {
            collector.RefreshConnection();
        }
    }
}
