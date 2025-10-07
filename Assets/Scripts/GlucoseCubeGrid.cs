using System.Collections.Generic;
using UnityEngine;

public class GlucoseCubeGrid : MonoBehaviour
{
    [Header("Resource Parameters")]
    [Tooltip("Current amount of glucose in this cube")]
    [SerializeField] private float glucoseAmount;
    [Tooltip("Minimum possible glucose amount when spawned")]
    [SerializeField] private float minAmount = 50f;
    [Tooltip("Maximum possible glucose amount when spawned")]
    [SerializeField] private float maxAmount = 500f;

    [Header("Grid References")]
    [Tooltip("Prefab for the empty grid that replaces this when depleted")]
    [SerializeField] private GameObject emptyGridPrefab;
    [Tooltip("Original children that shouldn't be transferred to new grid")]
    [SerializeField] private Transform[] originalChildren;

    private void Start()
    {
        // Cache original children
        originalChildren = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            originalChildren[i] = transform.GetChild(i);
        }

        // Initialize with random glucose amount
        glucoseAmount = Random.Range(minAmount, maxAmount);
    }

    /// <summary>
    /// Called when the glucose resource is fully depleted
    /// </summary>
    private void ResourceDepleted()
    {
        if (emptyGridPrefab == null)
        {
            Debug.LogError("Empty grid prefab is not assigned!");
            return;
        }

        // Instantiate new empty grid (as replacement)
        GameObject newGrid = Instantiate(
            emptyGridPrefab,
            transform.position,
            transform.rotation,
            transform.parent
        );
        newGrid.transform.TryGetComponent<CubeGrid>(out var aaa);
        aaa.isOccupied = true;

        // Collect non-original children first (避免在遍历时修改 transform.childCount 导致跳过)
        List<Transform> childrenToMove = new List<Transform>();
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (!IsOriginalChild(child))
            {
                childrenToMove.Add(child);
            }
        }

        // Move them to new grid
        foreach (var child in childrenToMove)
        {
            child.SetParent(newGrid.transform, true);
        }

        // 安全更新 MapGenerator 的字典（先检查存在与否）
        if (MapGenerator.Instance != null)
        {
            if (MapGenerator.Instance.Transform_Vector3_Dictionary.TryGetValue(transform, out var pos))
            {
                MapGenerator.Instance.Transform_Vector3_Dictionary.Remove(transform);
                MapGenerator.Instance.Vector3_Transform_Dictionary.Remove(pos);

                MapGenerator.Instance.Transform_Vector3_Dictionary.Add(newGrid.transform, pos);
                MapGenerator.Instance.Vector3_Transform_Dictionary.Add(pos, newGrid.transform);
                MapGenerator.Instance.allGrids.Remove(transform);
                MapGenerator.Instance.allGrids.Add(newGrid.transform);
            }
            else
            {
                Debug.LogWarning("MapGenerator dictionaries didn't contain this transform when ResourceDepleted ran.");
            }
        }
        else
        {
            Debug.LogWarning("MapGenerator.Instance is null when ResourceDepleted ran.");
        }

        if (BloodVesselManager.bloodVesselManager != null)
        {
            BloodVesselManager.bloodVesselManager.RefreshAllConnections();
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// Checks if a child is one of the original children
    /// </summary>
    private bool IsOriginalChild(Transform child)
    {
        if (originalChildren == null) return false;

        foreach (Transform originalChild in originalChildren)
        {
            if (originalChild == child) return true;
        }
        return false;
    }

    /// <summary>
    /// Reduces the glucose amount by specified value
    /// </summary>
    /// <param name="amount">Amount to decrease</param>
    public void AmountDecrease(float amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"Attempted to decrease glucose by invalid amount: {amount}");
            return;
        }

        glucoseAmount = Mathf.Max(0f, glucoseAmount - amount);

        if (glucoseAmount <= 0f)
        {
            ResourceDepleted();
        }
    }
}
