using UnityEngine;

public class CubeGrid : MonoBehaviour
{
    public bool isOccupied = false;
    [SerializeField] private BoxCollider boxCollider;

    private void Awake()
    {
        Transform myVisual = transform.Find("Cube");
        boxCollider = myVisual.GetComponent<BoxCollider>();
    }

    private void Update()
    {
        if (isOccupied)
        {
            boxCollider.enabled = false;
        }
        else
        {
            boxCollider.enabled = true;
        }
    }
}
