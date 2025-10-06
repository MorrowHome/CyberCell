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
        if (!isConnected) return;

        // ÏûºÄÆÏÌÑÌÇ
        GameManager.Instance.glucoseAmount -= glucoseConsumptionPerSecond * Time.deltaTime;

        // Ñ°ÕÒ¸½½ü²¡¶¾

    }

    private void GenerateAntibody(Transform virus)
    {
        if (antibodyPrefab == null) return;

        GameObject antibody = Instantiate(antibodyPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
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

                if (virus)
                {
                    GenerateAntibody(virus);
                    yield return new WaitForSeconds(1f / antibodyPerSecond);
                }
            }
            yield return null;
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
        int randomNumber = UnityEngine.Random.Range(0, hits.Length);
        return hits[randomNumber].transform;
    }


}
