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

        activeObjects.Add(obj); // ��¼�ѽ������
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
    /// �����ж������ߣ���������ʹ�õģ�
    /// </summary>
    public void AsleepAll()
    {
        // 1. �ر����л�Ծ����
        foreach (var obj in activeObjects)
        {
            obj.SetActive(false);
            pool.Enqueue(obj); // ����
        }

        activeObjects.Clear();

        // 2. ȷ�����ж���Ҳ���ڹر�״̬
        foreach (var obj in pool)
        {
            obj.SetActive(false);
        }
    }
}
