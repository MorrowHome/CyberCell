using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全局对象池管理器（多Prefab版本）
/// </summary>


public class ObjectPool
{
    private readonly GameObject prefab;
    private readonly Transform parent;
    private readonly Queue<GameObject> available = new();
    private readonly HashSet<GameObject> active = new();
    private int totalCreated;
    private const int ExpandStep = 5;

    public ObjectPool(GameObject prefab, Transform parent, int initialSize)
    {
        this.prefab = prefab;
        this.parent = parent;
        Expand(initialSize);
    }

    private void Expand(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var obj = Object.Instantiate(prefab, parent);
            obj.SetActive(false);
            available.Enqueue(obj);
            totalCreated++;
        }
    }

    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        if (available.Count == 0)
            Expand(ExpandStep);

        var obj = available.Dequeue();
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        active.Add(obj);
        return obj;
    }

    public void Return(GameObject obj)
    {
        if (obj == null) return;
        if (!active.Contains(obj)) return;

        active.Remove(obj);
        obj.SetActive(false);
        available.Enqueue(obj);
    }

    public void AsleepAll()
    {
        foreach (var obj in active)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                available.Enqueue(obj);
            }
        }
        active.Clear();
    }

    public string PoolCountDebug()
    {
        return $"Active: {active.Count}, Available: {available.Count}, TotalCreated: {totalCreated}";
    }
}
