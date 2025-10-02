using UnityEngine;

public class GlucoseCollectorCell : MonoBehaviour
{
    [SerializeField] private Transform parentCubeGrid;
    [SerializeField] private BreathVisual breathVisual;
    [SerializeField] private string glucoseCubeGridTag = "GlucoseCubeGrid";
    [SerializeField] private float glucoseCollectedPerSecond = 1.0f;
    


    void Start()
    {
        parentCubeGrid = transform.parent;
        breathVisual = GetComponent<BreathVisual>();
    }

    void Update()
    {
        if (parentCubeGrid != null && parentCubeGrid.tag == glucoseCubeGridTag)
        {
            GlucoseCubeGrid glucoseCubeGrid = parentCubeGrid.GetComponent<GlucoseCubeGrid>();
            if (glucoseCubeGrid != null)
            {
                breathVisual.isActive = true;
                float collected = glucoseCollectedPerSecond * Time.deltaTime;
                glucoseCubeGrid.AmountDecrease(collected);
                GameManager.Instance.glucoseAmount += collected;
            }
        }
    }
}
