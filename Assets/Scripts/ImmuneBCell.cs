using System.Collections;
using UnityEngine;

public class ImmuneBCell : MonoBehaviour, IActionPointCost
{
    [Header("基础参数")]
    [SerializeField] private int actionPointCost = 5;
    [SerializeField] private float glucoseConsumptionPerSecond = 2f;
    [SerializeField] private float defenseRange = 5f;
    [SerializeField] private float antibodyPerSecond = 2f;
    [SerializeField] private GameObject antibodyPrefab;
    [SerializeField] private BloodVessel bloodVesselForward = null;
    [SerializeField] private BloodVessel bloodVesselBack = null;
    [SerializeField] private BloodVessel bloodVesselLeft = null;
    [SerializeField] private BloodVessel bloodVesselRight = null;

    [Header("能量系统")]
    [SerializeField] private float maxHungryTime = 5f;
    private float hungryTimer;
    private bool hasEnoughGlucose;
    private bool isConnected;
    private Coroutine attackRoutine;

    public int ActionPointCost => actionPointCost;

    private void Start()
    {
        // 注册对象池（每个Prefab只注册一次）
        ObjectPoolManager.Instance.RegisterPrefab(antibodyPrefab, 30);
        isConnected = true; // 可改为你自己的血管检查逻辑
        attackRoutine = StartCoroutine(AutoFire());
    }

    private IEnumerator AutoFire()
    {
        const float searchInterval = 0.2f;
        while (true)
        {
            if (!isConnected)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            Transform virus = FindTarget();
            if (virus != null)
            {
                FireAntibody(virus);
                yield return new WaitForSeconds(1f / antibodyPerSecond);
            }
            else
            {
                yield return new WaitForSeconds(searchInterval);
            }
        }
    }

    private void FireAntibody(Transform target)
    {
        if (antibodyPrefab == null || target == null) return;
        Vector3 spawnPos = transform.position + Vector3.up * 0.5f;

        GameObject antibody = ObjectPoolManager.Instance.Get(antibodyPrefab, spawnPos, Quaternion.identity);
        if (antibody == null) return;

        var ab = antibody.GetComponent<Antibody>();
        ab.Init(antibodyPrefab);
        ab.SetTarget(target);
    }

    private Transform FindTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, defenseRange);
        Transform nearest = null;
        float minDist = float.MaxValue;
        foreach (var h in hits)
        {
            if (!h.TryGetComponent<IDamageable>(out _)) continue;
            float dist = Vector3.Distance(transform.position, h.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = h.transform;
            }
        }
        return nearest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, defenseRange);
    }
}
