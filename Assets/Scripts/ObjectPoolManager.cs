using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全局对象池管理器（多Prefab版本）
/// </summary>
public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance;

    private readonly Dictionary<GameObject, ObjectPool> pools = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 注册Prefab（初始化对应对象池）
    /// </summary>
    public void RegisterPrefab(GameObject prefab, int initialSize = 10)
    {
        if (prefab == null) return;
        if (pools.ContainsKey(prefab)) return;

        var pool = new ObjectPool(prefab, transform, initialSize);
        pools[prefab] = pool;
    }

    /// <summary>
    /// 从对应Prefab的对象池中取出实例
    /// </summary>
    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        if (!pools.TryGetValue(prefab, out var pool))
        {
            RegisterPrefab(prefab, 10);
            pool = pools[prefab];
        }

        return pool.Get(position, rotation);
    }

    /// <summary>
    /// 回收对象
    /// </summary>
    public void Return(GameObject prefab, GameObject obj)
    {
        if (prefab == null || obj == null) return;

        if (pools.TryGetValue(prefab, out var pool))
            pool.Return(obj);
        else
        {
            Debug.LogWarning($"[ObjectPoolManager] 无 {prefab.name} 对象池，销毁对象。");
            Destroy(obj);
        }
    }

    public void AsleepAll()
    {
        foreach (var pool in pools.Values)
            pool.AsleepAll();
    }

    public void DebugAllPools()
    {
        foreach (var kv in pools)
            Debug.Log($"[Pool:{kv.Key.name}] {kv.Value.PoolCountDebug()}");
    }
}