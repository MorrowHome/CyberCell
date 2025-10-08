using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImmuneBCell : MonoBehaviour, IActionPointCost
{
    [Header("基础参数")]
    [SerializeField] private int actionPointCost = 5;
    [SerializeField] private float glucoseConsumptionPerSecond = 2f;
    [SerializeField] private float defenseRange = 5f;
    [SerializeField] private float antibodyPerSecond = 2f;
    [SerializeField] private GameObject antibodyPrefab;

    [Header("邻近血管（四向）")]
    [SerializeField] private BloodVessel bloodVesselForward = null;
    [SerializeField] private BloodVessel bloodVesselBack = null;
    [SerializeField] private BloodVessel bloodVesselLeft = null;
    [SerializeField] private BloodVessel bloodVesselRight = null;

    [Header("能量系统")]
    [SerializeField] private float maxHungryTime = 5f;
    private float hungryTimer = 0f;
    private bool hasEnoughGlucose = true;
    private bool isConnected = false;

    private Coroutine attackRoutine;
    private List<BloodVessel> neighborBloodVessels = new List<BloodVessel>();
    private Dictionary<Vector3, BloodVessel> neighborCache = new();
    private Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
    private Transform parentCubeGrid;

    public int ActionPointCost => actionPointCost;

    private void Start()
    {
        ObjectPoolManager.Instance.RegisterPrefab(antibodyPrefab, 30);
        parentCubeGrid = transform.parent;
        RefreshNeighborBloodVessels();
        isConnected = neighborBloodVessels.Count > 0;

        attackRoutine = StartCoroutine(AutoFire());

        BuildManager.Instance.OnPlaceSomething += (s, e) =>
        {
            RefreshNeighborBloodVessels();
            isConnected = CheckConnectionToBloodVessel();
        };
    }

    private void Update()
    {
        if(GameManager.Instance.CurrentTurn == GameManager.TurnType.DefenseTime)
        {
            ConsumeGlucoseFromVessels();

            // 饥饿状态检测
            if (!hasEnoughGlucose)
            {
                hungryTimer += Time.deltaTime;
                if (hungryTimer >= maxHungryTime)
                {
                    Debug.Log($"{name} 因缺乏葡萄糖而死亡！");
                    Destroy(gameObject);
                }
            }
            else
            {
                hungryTimer = 0f;
            }
        }

    }

    #region 攻击逻辑
    private IEnumerator AutoFire()
    {
        const float searchInterval = 0.2f;

        while (true)
        {
            if (!isConnected || !hasEnoughGlucose)
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
    #endregion

    #region 葡萄糖消耗逻辑

    private void ConsumeGlucoseFromVessels()
    {
        if (neighborBloodVessels.Count == 0) return;

        float totalNeeded = glucoseConsumptionPerSecond * Time.deltaTime;
        float perVessel = totalNeeded / neighborBloodVessels.Count;
        float totalAbsorbed = 0f;

        foreach (var vessel in neighborBloodVessels)
        {
            if (vessel == null) continue;

            float available = vessel.GlucoseAmount;
            float consumed = Mathf.Min(available, perVessel);
            vessel.SetGlucoseAmount(available - consumed);
            totalAbsorbed += consumed;
        }

        hasEnoughGlucose = totalAbsorbed >= totalNeeded * 0.8f; // 允许轻微缺失
    }

    private void RefreshNeighborBloodVessels()
    {
        if (parentCubeGrid == null || MapGenerator.Instance == null) return;

        if (!MapGenerator.Instance.Transform_Vector3_Dictionary.TryGetValue(parentCubeGrid, out Vector3 myPos))
            return;

        neighborBloodVessels.Clear();

        foreach (var dir in directions)
        {
            Vector3 neighborPos = myPos + dir;
            BloodVessel vessel = null;

            if (neighborCache.TryGetValue(neighborPos, out vessel) && vessel != null)
            {
                neighborBloodVessels.Add(vessel);
                continue;
            }

            if (MapGenerator.Instance.Vector3_Transform_Dictionary.TryGetValue(neighborPos, out Transform neighborGrid))
            {
                vessel = neighborGrid.GetComponentInChildren<BloodVessel>();
                if (vessel != null)
                {
                    neighborBloodVessels.Add(vessel);
                    neighborCache[neighborPos] = vessel;
                }
            }
        }

        bloodVesselForward = GetVesselInDirection(Vector3.forward, myPos);
        bloodVesselBack = GetVesselInDirection(Vector3.back, myPos);
        bloodVesselLeft = GetVesselInDirection(Vector3.left, myPos);
        bloodVesselRight = GetVesselInDirection(Vector3.right, myPos);
    }

    private bool CheckConnectionToBloodVessel()
    {
        if (parentCubeGrid == null || MapGenerator.Instance == null)
            return false;

        if (!MapGenerator.Instance.Transform_Vector3_Dictionary.TryGetValue(parentCubeGrid, out Vector3 myPos))
            return false;

        foreach (var dir in directions)
        {
            Vector3 neighborPos = myPos + dir;

            if (neighborCache.TryGetValue(neighborPos, out var cachedVessel) && cachedVessel != null && cachedVessel.isConnected)
                return true;

            if (MapGenerator.Instance.Vector3_Transform_Dictionary.TryGetValue(neighborPos, out Transform neighborGrid))
            {
                BloodVessel vessel = neighborGrid.GetComponentInChildren<BloodVessel>();
                neighborCache[neighborPos] = vessel;

                if (vessel != null && vessel.isConnected)
                    return true;
            }
        }

        return false;
    }



    private BloodVessel GetVesselInDirection(Vector3 dir, Vector3 myPos)
    {
        Vector3 neighborPos = myPos + dir;
        neighborCache.TryGetValue(neighborPos, out var vessel);
        return vessel;
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = hasEnoughGlucose ? Color.cyan : Color.red;
        Gizmos.DrawWireSphere(transform.position, defenseRange);
    }
}
