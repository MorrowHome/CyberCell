using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int initialSize = 20;

    private readonly Queue<GameObject> pool = new Queue<GameObject>();
    private readonly List<GameObject> activeObjects = new List<GameObject>();

    public static ObjectPool Instance;

    private void Awake()
    {
        Instance = this;
        FillPool();
    }

    private void FillPool()
    {
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject GetFromPool(Vector3 position, Quaternion rotation)
    {
        if (pool.Count == 0)
            ExpandPool(5);

        GameObject obj = pool.Dequeue();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);

        activeObjects.Add(obj); // 记录已借出对象
        return obj;
    }

    public void ReturnToPool(GameObject obj)
    {
        if (activeObjects.Contains(obj))
            activeObjects.Remove(obj);

        obj.SetActive(false);
        pool.Enqueue(obj);
    }

    private void ExpandPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    /// <summary>
    /// 让所有对象都休眠（包括正在使用的）
    /// </summary>
    public void AsleepAll()
    {
        // 1. 关闭所有活跃对象
        foreach (var obj in activeObjects)
        {
            obj.SetActive(false);
            pool.Enqueue(obj); // 回收
        }

        activeObjects.Clear();

        // 2. 确保池中对象也处于关闭状态
        foreach (var obj in pool)
        {
            obj.SetActive(false);
        }
    }
}
