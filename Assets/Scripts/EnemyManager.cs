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
    /// ����һ���µĵ�������
    /// </summary>
    public void StartNewWave(int count)
    {
        if (isSpawning) return; // �����ظ�����
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
