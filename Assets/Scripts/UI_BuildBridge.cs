using UnityEngine;
using UnityEngine.UI;

public class UI_BuildBridge : MonoBehaviour
{
    [Header("Build Buttons")]
    public Button createCollectorButton;
    public Button createBloodVesselButton;
    public Button createWallButton;
    public Button createTowerButton;

    private void Start()
    {
        if (BuildManager.Instance == null)
        {
            Debug.LogError("[UI_BuildBridge] BuildManager not found in scene!");
            return;
        }

        // 绑定 UI 按钮事件
        createCollectorButton.onClick.AddListener(() => SetBuildType(1));
        createBloodVesselButton.onClick.AddListener(() => SetBuildType(2));
        createWallButton.onClick.AddListener(() => SetBuildType(3));
        createTowerButton.onClick.AddListener(() => SetBuildType(4));
    }

    private void SetBuildType(int buildID)
    {
        if (BuildManager.Instance == null) return;

        switch (buildID)
        {
            case 1:
                BuildManager.Instance.SendMessage("SetCurrentBuild", 
                    System.Enum.Parse(typeof(BuildManager.WhatToBuild), "Collector"));
                break;
            case 2:
                BuildManager.Instance.SendMessage("SetCurrentBuild", 
                    System.Enum.Parse(typeof(BuildManager.WhatToBuild), "BloodVessel"));
                break;
            case 3:
                BuildManager.Instance.SendMessage("SetCurrentBuild", 
                    System.Enum.Parse(typeof(BuildManager.WhatToBuild), "Wall"));
                break;
            case 4:
                BuildManager.Instance.SendMessage("SetCurrentBuild", 
                    System.Enum.Parse(typeof(BuildManager.WhatToBuild), "Tower"));
                break;
        }

        Debug.Log($"[UI_BuildBridge] Set build type to ID={buildID}");
    }
}
