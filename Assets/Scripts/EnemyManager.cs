using UnityEngine;
using System.Collections;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [Header("Spawn Settings")]
    [SerializeField] private GameObject virusPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform enemyContainer;
    [SerializeField] private float spawnInterval = 2f;

    private int enemiesToSpawn;
    private bool isSpawning;
    private int waveCount = 0;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 启动一波新的敌人生成
    /// </summary>
    public void StartNewWave(int count)
    {
        if (isSpawning) return; // 避免重复开启
        enemiesToSpawn = count;
        waveCount++;
        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        isSpawning = true;
        Debug.Log($"[EnemyManager] Wave {waveCount} started, spawning {enemiesToSpawn} viruses");

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            GameObject virus = Instantiate(virusPrefab, spawnPoint.position, Quaternion.identity, enemyContainer);
            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
        Debug.Log($"[EnemyManager] Wave {waveCount} complete");
    }
}
