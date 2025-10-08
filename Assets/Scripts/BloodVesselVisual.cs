using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BloodVessel))]
public class BloodVesselVisual : MonoBehaviour
{
    [Header("颜色与材质")]
    [SerializeField] private GameObject myVisual;
    [SerializeField] private float colorMaxGlucose = 100f;
    [SerializeField] private Color lowColor = new Color(0.2f, 0f, 0f);
    [SerializeField] private Color highColor = new Color(0.7f, 0f, 0f);

    [Header("红细胞动态效果")]
    [SerializeField] private GameObject redBloodCellPrefab;
    [SerializeField] private int maxRedCells = 20;
    [SerializeField] private float redCellRadius = 0.3f;
    [SerializeField] private float rotationSpeed = 30f;

    private BloodVessel vessel;
    private List<GameObject> activeCells = new();

    private void Awake()
    {
        vessel = GetComponent<BloodVessel>();
        if (myVisual == null)
            myVisual = vessel.gameObject;
    }

    private void Update()
    {
        if (vessel == null) return;

        UpdateColor();
        UpdateRedBloodCells();
        AnimateRedBloodCells();
    }

    /// <summary>
    /// 根据血糖浓度更新颜色（越高越深）
    /// </summary>
    private void UpdateColor()
    {
        if (myVisual == null) return;
        var renderer = myVisual.GetComponent<MeshRenderer>();
        if (renderer == null) return;

        float t = Mathf.Clamp01(vessel.GlucoseAmount / colorMaxGlucose);
        Color color = Color.Lerp(lowColor, highColor, t);
        renderer.material.color = color;
    }

    /// <summary>
    /// 根据浓度控制红细胞数量
    /// </summary>
    private void UpdateRedBloodCells()
    {
        if (redBloodCellPrefab == null) return;

        int targetCount = Mathf.RoundToInt(Mathf.Clamp(vessel.GlucoseAmount / 10f, 0, maxRedCells));

        // 增加红细胞
        while (activeCells.Count < targetCount)
        {
            GameObject cell = Instantiate(redBloodCellPrefab, transform);
            cell.transform.localPosition = Random.insideUnitSphere * redCellRadius;
            activeCells.Add(cell);
        }

        // 减少红细胞
        while (activeCells.Count > targetCount)
        {
            var cell = activeCells[0];
            activeCells.RemoveAt(0);
            Destroy(cell);
        }
    }

    /// <summary>
    /// 让红细胞循环流动
    /// </summary>
    private void AnimateRedBloodCells()
    {
        if (activeCells.Count == 0) return;

        foreach (var cell in activeCells)
        {
            if (cell == null) continue;

            // 绕中心旋转，看起来像血流循环
            cell.transform.RotateAround(transform.position, Vector3.up, rotationSpeed * Time.deltaTime);

            // 加点微小漂浮扰动
            cell.transform.localPosition += Random.insideUnitSphere * 0.001f;
        }
    }
}
