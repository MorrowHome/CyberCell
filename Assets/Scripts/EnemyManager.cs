using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [Header("Spawn Settings")]
    [SerializeField] private GameObject virusPrefab;
    [SerializeField] private GameObject bacteriaPrefab;
    [SerializeField] private List<Transform> spawnPoints; // 多个生成点
    [SerializeField] private float spawnRadius = 2f;       // 每个点生成随机偏移
    [SerializeField] public Transform enemyContainer;
    [SerializeField] public float baseSpawnInterval = 2f;
    [SerializeField] public float minSpawnInterval = 0.02f;
    public int enemiesAlive;

    private int enemiesToSpawn;
    private bool isSpawning;
    public int waveCount = 0;

    private readonly List<Transform> enemyList = new List<Transform>();
    public int enemyCount => enemyList.Count;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (enemyContainer == null)
            enemyContainer = new GameObject("EnemyContainer").transform;

        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            // 如果没有指定生成点，则使用自身位置作为默认生成点
            spawnPoints = new List<Transform> { transform };
        }
    }

    public void StartNewWave(int count)
    {
        waveCount++;
        enemiesToSpawn += count;
        enemiesAlive += count;
        if (!isSpawning) StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        isSpawning = true;

#if UNITY_EDITOR
        Debug.Log($"[EnemyManager] Wave {waveCount} started, spawning {enemiesToSpawn} viruses");
#endif

        float spawnInterval = Mathf.Max(minSpawnInterval, baseSpawnInterval - waveCount * 0.2f);

        while (enemiesToSpawn > 0)
        {
            // 随机选择一个生成点
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            GameObject prefab = (Random.value < 0.8f) ? virusPrefab : bacteriaPrefab;

            // 随机偏移
            Vector3 offset = new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                0f,
                Random.Range(-spawnRadius, spawnRadius)
            );

            GameObject virus = Instantiate(prefab, spawnPoint.position + offset, Quaternion.identity, enemyContainer);

            // 注册敌人
            RegisterEnemy(virus.transform);

            enemiesToSpawn--;
            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;

#if UNITY_EDITOR
        Debug.Log($"[EnemyManager] Wave {waveCount} complete");
#endif
    }

    public void RegisterEnemy(Transform enemy)
    {
        if (!enemyList.Contains(enemy))
            enemyList.Add(enemy);
    }

    public void UnregisterEnemy(Transform enemy)
    {
        if (enemyList.Contains(enemy))
        {
            enemyList.Remove(enemy);
            enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        }
    }

    public Transform GetNearestEnemy(Vector3 position)
    {
        Transform nearest = null;
        float minDist = Mathf.Infinity;

        for (int i = 0; i < enemyList.Count; i++)
        {
            Transform e = enemyList[i];
            if (e == null || !e.gameObject.activeSelf) continue;

            float dist = (position - e.position).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                nearest = e;
            }
        }

        return nearest;
    }
}
