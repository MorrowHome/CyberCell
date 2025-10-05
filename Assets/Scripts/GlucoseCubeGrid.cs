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
        // Cache original children more efficiently
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

        // Create new empty grid
        GameObject newGrid = Instantiate(
            emptyGridPrefab,
            transform.position,
            transform.rotation,
            transform.parent
        );

        // Transfer non-original children to new grid
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (!IsOriginalChild(child))
            {
                child.SetParent(newGrid.transform, true);
            }
        }
        MapGenerator.Instance.Transform_Vector3_Dictionary.TryGetValue(gameObject.transform, out var pos);

        MapGenerator.Instance.Transform_Vector3_Dictionary.Remove(gameObject.transform);
        MapGenerator.Instance.Vector3_Transform_Dictionary.Remove(pos);
        MapGenerator.Instance.Transform_Vector3_Dictionary.Add(newGrid.transform, pos);
        MapGenerator.Instance.Vector3_Transform_Dictionary.Add(pos, newGrid.transform);

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
            if (child == originalChild)
            {
                return true;
            }
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

        glucoseAmount = Mathf.Max(0, glucoseAmount - amount);

        if (glucoseAmount <= 0)
        {
            ResourceDepleted();
        }
    }
}