using System.Collections;
using UnityEngine;

public class Nanobot : MonoBehaviour
{
    [Header("�������")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float buildTime = 2f;

    private bool isBusy = false;
    private Vector3 targetPos;
    private GameObject buildPrefab;
    private CubeGrid targetGrid;
    private int actionPointCost;

    [Header("��������")]
    [SerializeField] private float defenseRange = 6f;
    [SerializeField] private float attackRate = 1.5f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private LineRenderer laserLine;
    [SerializeField] private Transform firePoint;

    private Coroutine attackRoutine;

    private void Start()
    {
        attackRoutine = StartCoroutine(AutoAttack());
    }

    private void OnDisable()
    {
        if (attackRoutine != null)
            StopCoroutine(attackRoutine);
    }

    public void AssignBuildTask(Vector3 target, GameObject prefab, CubeGrid grid, int cost)
    {
        if (isBusy)
        {
            Debug.Log("Nanobot ��æ����ȴ���ǰ������ɡ�");
            return;
        }

        targetPos = target;
        buildPrefab = prefab;
        targetGrid = grid;
        actionPointCost = cost;

        StartCoroutine(BuildRoutine());
    }

    private IEnumerator BuildRoutine()
    {
        isBusy = true;

        // ֹͣ����
        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(buildTime);

        if (!GameManager.Instance.HasEnoughPoints(actionPointCost))
        {
            Debug.Log("�ж��㲻�㣬����ʧ�ܡ�");
            isBusy = false;
            attackRoutine = StartCoroutine(AutoAttack());
            yield break;
        }

        GameManager.Instance.SpendPoints(actionPointCost);

        GameObject ins = Instantiate(buildPrefab, targetPos, Quaternion.identity, targetGrid.transform);
        targetGrid.whatIsOnMe = ins.transform;
        targetGrid.isOccupied = true;

        BloodVessel vessel = ins.GetComponent<BloodVessel>();
        if (vessel != null)
        {
            vessel.Init();
        }

        BuildManager.Instance.OnBuildFinished();
        isBusy = false;

        // �ָ�����
        attackRoutine = StartCoroutine(AutoAttack());
    }

    private IEnumerator AutoAttack()
    {
        while (true)
        {
            if (!isBusy)
            {
                Transform enemy = FindNearestEnemy();
                if (enemy != null)
                {
                    yield return StartCoroutine(FireLaser(enemy));
                    yield return new WaitForSeconds(1f / attackRate);
                }
            }
            yield return null;
        }
    }

    private Transform FindNearestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, defenseRange);
        Transform nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var dmg))
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = hit.transform;
                }
            }
        }

        return nearest;
    }

    private IEnumerator FireLaser(Transform target)
    {
        if (target == null) yield break;

        // ��ʾ����
        if (laserLine)
        {
            laserLine.enabled = true;
            laserLine.SetPosition(0, firePoint ? firePoint.position : transform.position);
            laserLine.SetPosition(1, target.position);
        }

        // ����˺�
        if (target.TryGetComponent<IDamageable>(out var dmg))
        {
            dmg.TakeDamage(attackDamage);
        }

        yield return new WaitForSeconds(0.1f);

        // ���ؼ���
        if (laserLine)
            laserLine.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, defenseRange);
    }
}
