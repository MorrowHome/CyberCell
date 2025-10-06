using UnityEngine;

public class CubeGrid : MonoBehaviour
{
    public bool isOccupied = false;
    [SerializeField] private BoxCollider boxCollider;
    public Transform myVisual;


    public Transform whatIsOnMe;

    private void Awake()
    {
        myVisual = transform.Find("Visual");
        boxCollider = myVisual.GetComponent<BoxCollider>();
    }

    private void Start()
    {

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
