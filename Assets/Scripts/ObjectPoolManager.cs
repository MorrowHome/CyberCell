using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ȫ�ֶ���ع���������Prefab�汾��
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
    /// ע��Prefab����ʼ����Ӧ����أ�
    /// </summary>
    public void RegisterPrefab(GameObject prefab, int initialSize = 10)
    {
        if (prefab == null) return;
        if (pools.ContainsKey(prefab)) return;

        var pool = new ObjectPool(prefab, transform, initialSize);
        pools[prefab] = pool;
    }

    /// <summary>
    /// �Ӷ�ӦPrefab�Ķ������ȡ��ʵ��
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
    /// ���ն���
    /// </summary>
    public void Return(GameObject prefab, GameObject obj)
    {
        if (prefab == null || obj == null) return;

        if (pools.TryGetValue(prefab, out var pool))
            pool.Return(obj);
        else
        {
            Debug.LogWarning($"[ObjectPoolManager] �� {prefab.name} ����أ����ٶ���");
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