using UnityEngine;
using System.Collections;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [Header("Spawn Settings")]
    [SerializeField] private GameObject virusPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform enemyContainer;
    [SerializeField] private float baseSpawnInterval = 2f; // 基础间隔
    [SerializeField] private float minSpawnInterval = 0.2f; // 最小间隔限制

    private int enemiesToSpawn;
    private bool isSpawning;
    private int waveCount = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 启动一波新的敌人生成
    /// </summary>
    public void StartNewWave(int count)
    {
        waveCount++;
        enemiesToSpawn += count; // 累加敌人数量
        if (!isSpawning) StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        isSpawning = true;

#if UNITY_EDITOR
        Debug.Log($"[EnemyManager] Wave {waveCount} started, spawning {enemiesToSpawn} viruses");
#endif

        float spawnInterval = Mathf.Max(minSpawnInterval, baseSpawnInterval / waveCount);

        while (enemiesToSpawn > 0)
        {
            GameObject virus = Instantiate(virusPrefab, spawnPoint.position, Quaternion.identity, enemyContainer);
            enemiesToSpawn--;
            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;

#if UNITY_EDITOR
        Debug.Log($"[EnemyManager] Wave {waveCount} complete");
#endif
    }
}
