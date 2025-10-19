using System.Collections;
using UnityEngine;

public class Nanobot : MonoBehaviour
{
    public static Nanobot Instance { get; private set; }

    private PlayerInputActions input;

    [Header("建造参数")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float buildTime = 2f;
    [SerializeField] private AnimationCurve buildCurve;

    [Header("建造特效")]
    [SerializeField] private ParticleSystem buildParticles;
    [SerializeField] private LineRenderer buildBeam;
    [SerializeField] private Transform beamStartPoint;


    private bool isBusy = false;
    private Vector3 targetPos;
    private GameObject buildPrefab;
    private CubeGrid targetGrid;
    private int actionPointCost;

    [Header("攻击参数")]
    [SerializeField] private float defenseRange = 6f;
    [SerializeField] private float attackRate = 1.5f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private LineRenderer laserLine;
    [SerializeField] private Transform firePoint;

    private Coroutine attackRoutine;

    private void Awake()
    {
        input = InputManager.Instance.inputActions;
    }
    private void Start()
    {
        attackRoutine = StartCoroutine(AutoAttack());
        input.Player.StopBuilding.performed += ctx =>
        {
            if (isBusy)
            {
                StopAllCoroutines();
                isBusy = false;
                if (buildParticles != null) buildParticles.Stop();
                if (buildBeam != null) buildBeam.enabled = false;
                Debug.Log("建造被取消。");
                attackRoutine = StartCoroutine(AutoAttack());
            }
        };
    }

    public void AssignBuildTask(Vector3 target, GameObject prefab, CubeGrid grid, int cost)
    {
        if (isBusy) return;

        targetPos = target;
        buildPrefab = prefab;
        targetGrid = grid;
        actionPointCost = cost;

        StartCoroutine(BuildRoutine());
    }

    private IEnumerator BuildRoutine()
    {
        isBusy = true;

        // 1. 直线移动到目标
        // 记录初始高度
        float fixedY = transform.position.y;

        // 移动到目标（只在X-Z平面移动）
        while (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                new Vector3(targetPos.x, 0, targetPos.z)) > 0.05f)
        {
            Vector3 nextPos = Vector3.MoveTowards(
                new Vector3(transform.position.x, 0, transform.position.z),
                new Vector3(targetPos.x, 0, targetPos.z),
                moveSpeed * Time.deltaTime
            );

            // 应用固定高度
            transform.position = new Vector3(nextPos.x, fixedY, nextPos.z);
            yield return null;
        }

        // 2. 检查行动点
        if (!GameManager.Instance.HasEnoughPoints(actionPointCost))
        {
            Debug.Log("行动点不足，建造失败。");
            isBusy = false;
            yield break;
        }
        GameManager.Instance.SpendPoints(actionPointCost);

        // 3. 实例化建筑
        GameObject ins = Instantiate(buildPrefab, targetPos, Quaternion.identity, targetGrid.transform);
        ins.transform.localScale = Vector3.zero;

        // 4. 启动粒子特效和建造光束
        if (buildParticles != null)
        {
            // 停止并清空之前的粒子
            buildParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            // 移动到目标位置
            buildParticles.transform.position = targetPos;

            // 从头播放
            buildParticles.Play();
        }


        if (buildBeam != null && beamStartPoint != null)
        {
            buildBeam.enabled = true;
            buildBeam.SetPosition(0, beamStartPoint.position);
            buildBeam.SetPosition(1, targetPos);
        }

        // 5. 建造渐显动画
        float t = 0f;
        while (t < buildTime)
        {
            t += Time.deltaTime;
            float factor = buildCurve != null ? buildCurve.Evaluate(t / buildTime) : t / buildTime;
            ins.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, factor);

            if (buildBeam != null && beamStartPoint != null)
                buildBeam.SetPosition(0, beamStartPoint.position);

            yield return null;
        }
        ins.transform.localScale = Vector3.one;

        // 6. 停止特效
        if (buildParticles != null) buildParticles.Stop();
        if (buildBeam != null) buildBeam.enabled = false;

        targetGrid.whatIsOnMe = ins.transform;
        targetGrid.isOccupied = true;

        BloodVessel vessel = ins.GetComponent<BloodVessel>();
        if (vessel != null) vessel.Init();

        BuildManager.Instance.OnBuildFinished();
        isBusy = false;
                attackRoutine = StartCoroutine(AutoAttack());
    }





    private void OnDisable()
    {
        if (attackRoutine != null)
            StopCoroutine(attackRoutine);
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

        // 显示激光
        if (laserLine)
        {
            laserLine.enabled = true;
            laserLine.SetPosition(0, firePoint ? firePoint.position : transform.position);
            laserLine.SetPosition(1, target.position);
        }

        // 造成伤害
        if (target.TryGetComponent<IDamageable>(out var dmg))
        {
            dmg.TakeDamage(attackDamage);
        }

        yield return new WaitForSeconds(0.1f);

        // 隐藏激光
        if (laserLine)
            laserLine.enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, defenseRange);
    }

}


