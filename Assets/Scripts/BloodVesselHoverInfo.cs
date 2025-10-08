using UnityEngine;

/// <summary>
/// 当鼠标悬浮在血管上时，显示血管的葡萄糖浓度等信息
/// </summary>
public class BloodVesselHoverInfo : HoverInfoProvider
{
    private BloodVessel vessel;

    [Header("显示设置")]
    [SerializeField] private string title = "血管格子";
    [SerializeField] private string unit = "mg/dL";  // 单位
    [SerializeField] private Color lowColor = Color.gray;
    [SerializeField] private Color highColor = Color.red;

    private void Awake()
    {
        vessel = GetComponent<BloodVessel>();
    }

    public override string GetHoverInfo()
    {
        if (vessel == null)
            return $"{title}\n<color=#888888>无数据</color>";

        float glucose = vessel.GlucoseAmount;
        Color c = Color.Lerp(lowColor, highColor, Mathf.Clamp01(glucose / 50f));

        string hexColor = ColorUtility.ToHtmlStringRGB(c);
        return $"{title}\n<color=#{hexColor}>葡萄糖浓度: {glucose:F2} {unit}</color>";
    }
}
