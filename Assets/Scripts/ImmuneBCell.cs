using System;
using System.Collections;
using UnityEngine;
using static ImmuneBCell;

public class ImmuneBCell : MonoBehaviour, IActionPointCost
{
    [SerializeField] private int actionPointCost = 5;
    [SerializeField] private float glucoseConsumptionPerSecond = 2;
    [SerializeField] private float defenseRange = 5;
    [SerializeField] private float antibodyPerSecond = 2;
    [SerializeField] private GameObject antibodyPrefab;
    [SerializeField] private Transform objectPool;
    [SerializeField] private float maxHungryTime = 5f;
    [SerializeField] private float hungryTimer = 0f;

    private bool hasEnoughGlucose;


    private Transform parentCubeGrid;
    [SerializeField] private bool isConnected = false;
    private float antibodyTimer = 0f;

    public int ActionPointCost => actionPointCost;


    public enum TargetingMode { Nearest, Strongest, Weakest, Random }
    private TargetingMode targetingMode;

    private void Awake()
    {
        parentCubeGrid = transform.parent;
    }

    void Start()
    {
        BuildManager.Instance.OnPlaceSomething += BuildManager_OnOnPlaceSomething;
        isConnected = CheckConnectionToBloodVessel();
    }

    private void BuildManager_OnOnPlaceSomething(object sender, EventArgs e)
    {
        isConnected = CheckConnectionToBloodVessel();
    }

    void Update()
    {
        if (GameManager.Instance.CurrentTurn != GameManager.TurnType.DefenseTime) return;
        if (!isConnected) return;

        // 消耗葡萄糖
        hasEnoughGlucose = (GameManager.Instance.glucoseAmount > glucoseConsumptionPerSecond * Time.deltaTime);
        if (hasEnoughGlucose)
        {
            GameManager.Instance.glucoseAmount -= glucoseConsumptionPerSecond * Time.deltaTime;
            hungryTimer = 0;
        }
        else
        {
            hungryTimer += Time.deltaTime;
            if(hungryTimer >= maxHungryTime)
            {
                Destroy(gameObject);
            }
        }
        

        // 寻找附近病毒

    }

    private void GenerateAntibody(Transform virus)
    {
        if (antibodyPrefab == null || virus == null) return;

        GameObject antibody = ObjectPool.Instance.GetFromPool(
                    transform.position + Vector3.up * 0.5f,
                    Quaternion.identity
                );

        if (antibody != null)
            antibody.GetComponent<Antibody>().SetTarget(virus);
    }


    private bool CheckConnectionToBloodVessel()
    {
        if (parentCubeGrid == null) return false;

        if (!MapGenerator.Instance.Transform_Vector3_Dictionary.TryGetValue(parentCubeGrid, out Vector3 myPos))
            return false;

        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
        foreach (var dir in directions)
        {
            Vector3 neighborPos = myPos + dir;

            if (MapGenerator.Instance.Vector3_Transform_Dictionary.TryGetValue(neighborPos, out Transform neighborGrid))
            {
                CubeGrid cubeGrid = neighborGrid.GetComponent<CubeGrid>();
                if (cubeGrid == null || cubeGrid.whatIsOnMe == null) continue;

                BloodVessel vessel = cubeGrid.whatIsOnMe.GetComponent<BloodVessel>();
                if (vessel != null && vessel.isConnected)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, defenseRange);
    }



    private Coroutine attackRoutine;

    private void OnEnable()
    {
        attackRoutine = StartCoroutine(AutoFire());
    }
    private void OnDisable()
    {
        if (attackRoutine != null) StopCoroutine(attackRoutine);
    }

    private IEnumerator AutoFire()
    {
        while (true)
        {
            if (isConnected)
            {
                Transform virus = null;
                switch (targetingMode)
                {
                    case TargetingMode.Nearest:
                        virus = FindNearestEnemy();
                        break;
                    case TargetingMode.Strongest:
                        virus = FindStrongestEnemy();
                        break;
                    case TargetingMode.Weakest:
                        virus = FindWeakestEnemy();
                        break;
                    case TargetingMode.Random:
                        virus = FindRandomEnemy();
                        break;
                }

                if (virus != null)
                {
                    GenerateAntibody(virus);
                    yield return new WaitForSeconds(1f / antibodyPerSecond);
                }
                else
                {
                    yield return null; // 没有敌人时等待下一帧
                }
            }
            else
            {
                yield return null; // 不连通时等待
            }
        }
    }


    private Transform FindNearestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, defenseRange);
        Transform nearestVirus = null;
        float minDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var dmg))
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestVirus = hit.transform;
                }
            }
        }
        return nearestVirus;
    }
    private Transform FindStrongestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, defenseRange);
        Transform nearestVirus = null;
        float maxHP = 0f;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var dmg))
            {
                float HP = dmg.HP;
                if (HP > maxHP)
                {
                    maxHP = HP;
                    nearestVirus = hit.transform;
                }
            }
        }
        return nearestVirus; 
    }
    private Transform FindWeakestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, defenseRange);
        Transform nearestVirus = null;
        float minHP = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var dmg))
            {
                float HP = dmg.HP;
                if (HP < minHP)
                {
                    minHP = HP;
                    nearestVirus = hit.transform;
                }
            }
        }
        return nearestVirus;
    }
    private Transform FindRandomEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, defenseRange);
        if (hits.Length == 0) return null;

        // 只返回有 IDamageable 的敌人
        var validHits = new System.Collections.Generic.List<Collider>();
        foreach (var hit in hits)
            if (hit.TryGetComponent<IDamageable>(out var _))
                validHits.Add(hit);

        if (validHits.Count == 0) return null;

        int randomNumber = UnityEngine.Random.Range(0, validHits.Count);
        return validHits[randomNumber].transform;
    }



}
