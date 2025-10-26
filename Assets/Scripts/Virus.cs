using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class Virus : MonoBehaviour, IDamageable, IDamaging
{
    [Header("移动参数")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private BloodVessel currentVessel; // 当前所在血管
    [SerializeField] private float HP = 10f;
    [SerializeField] private float damage = 3f;

    private Rigidbody rb;
    private Queue<BloodVessel> pathToHeart = new Queue<BloodVessel>(); // 寻路路径
    private CubeGrid currentGrid;

    float IDamageable.HP => HP;
    public float Damage => damage;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (currentVessel == null)
            currentVessel = FindNearestBloodVessel();

        BuildPathToHeart();
    }

    void FixedUpdate()
    {
        if (pathToHeart.Count == 0)
        {
            Vector3 d = (MapGenerator.Instance.heartCellTransform.position - transform.position).normalized;
            rb.MovePosition(rb.position + d * speed * Time.fixedDeltaTime);
            transform.LookAt(MapGenerator.Instance.heartCellTransform.position);
            return;
        }

        BloodVessel targetVessel = pathToHeart.Peek();
        Vector3 targetPos = targetVessel.transform.position + Vector3.up * 0.5f;

        Vector3 direction = (targetPos - transform.position).normalized;
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
        transform.LookAt(targetPos);

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            pathToHeart.Dequeue();

            // ---- 腐化血管 ----
            targetVessel.ApplyCorruption();

            currentVessel = targetVessel;
            UpdateCurrentGrid();
        }
    }

    private void UpdateCurrentGrid()
    {
        CubeGrid cube = GetCurrentCubeGrid();
        if (cube != null && cube != currentGrid)
        {
            if (currentGrid != null)
            {
                currentGrid.isOccupied = false;
                currentGrid.whatIsOnMe = null;
            }
            currentGrid = cube;
            currentGrid.isOccupied = true;
            currentGrid.whatIsOnMe = transform;
        }
    }

    private CubeGrid GetCurrentCubeGrid()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider.GetComponentInParent<CubeGrid>();
        }
        return null;
    }

    private BloodVessel FindNearestBloodVessel()
    {
        BloodVessel nearest = null;
        float minDist = float.MaxValue;
        foreach (var vessel in BloodVesselManager.bloodVesselManager.allBloodVessels)
        {
            float dist = Vector3.Distance(transform.position, vessel.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = vessel;
            }
        }
        return nearest;
    }

    private void BuildPathToHeart()
    {
        pathToHeart.Clear();
        if (currentVessel == null)
        {
            Debug.LogError("Virus 找不到当前血管，无法寻路到心脏！");
            return;
        }

        Queue<BloodVessel> queue = new Queue<BloodVessel>();
        Dictionary<BloodVessel, BloodVessel> parentMap = new Dictionary<BloodVessel, BloodVessel>();
        HashSet<BloodVessel> visited = new HashSet<BloodVessel>();

        queue.Enqueue(currentVessel);
        visited.Add(currentVessel);

        BloodVessel heartVessel = MapGenerator.Instance.heartCellTransform.GetComponentInParent<BloodVessel>();
        if (heartVessel == null) return;

        bool found = false;

        while (queue.Count > 0)
        {
            BloodVessel v = queue.Dequeue();
            if (v == heartVessel)
            {
                found = true;
                break;
            }

            foreach (var neighbor in v.neighborBloodVessels)
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    parentMap[neighbor] = v;
                    queue.Enqueue(neighbor);
                }
            }
        }

        if (!found) return;

        BloodVessel step = heartVessel;
        Stack<BloodVessel> stack = new Stack<BloodVessel>();
        while (step != currentVessel)
        {
            stack.Push(step);
            step = parentMap[step];
        }

        while (stack.Count > 0)
        {
            pathToHeart.Enqueue(stack.Pop());
        }
    }

    public void TakeDamage(float amount)
    {
        HP -= amount;
        if (HP <= 0f)
            Die();
    }

    private void Die()
    {
        if (currentGrid != null)
        {
            currentGrid.isOccupied = false;
            currentGrid.whatIsOnMe = null;
        }
        EnemyManager.Instance.enemiesAlive--;
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null && other.transform.parent.CompareTag("HeartCell"))
        {
            GameManager.Instance.TakeDamage(damage);
            Die();
        }
    }
}
