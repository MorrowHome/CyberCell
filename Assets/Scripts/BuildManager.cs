// BuildManager.cs
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class BuildManager : MonoBehaviour
{
    [SerializeField] private Camera cam;

    [Header("建造 Prefabs")]
    [SerializeField] private GameObject glucoseCollectorCellPrefab; // 1
    [SerializeField] private GameObject bloodVesselPrefab;          // 2
    [SerializeField] private GameObject wallPrefab;                 // 3
    [SerializeField] private GameObject towerPrefab;                // 4


    [SerializeField] private LayerMask cubeGridLayer;

    private Transform currentSelected;      // 显示 Selected 子物体
    private Transform lastHoveredCube;      // 最近一次射线命中的 Cube (用于 Place 回调)

    private PlayerInputActions.BuildActions buildActions; // 绑定到 InputManager.instance.inputActions.Build

    private enum WhatToBuild
    {
        Glucose = 1,
        BloodVessel = 2,
        Wall = 3,
        Tower = 4,
    }

    [SerializeField] private WhatToBuild currentBuild = WhatToBuild.Glucose;

    private void Awake()
    {
        if (InputManager.Instance == null)
        {
            Debug.LogError("InputManager not found in scene. Please add InputManager before BuildManager.");
            return;
        }

        // 拿到 Build action map（注意：这是 generated wrapper 的结构）
        buildActions = InputManager.Instance.inputActions.Build;
    }

    private void OnEnable()
    {
        // 重新确保拿到 BuildActions（场景切换 / 编辑模式下比较稳健）
        if (InputManager.Instance == null) return;
        buildActions = InputManager.Instance.inputActions.Build;

        // 订阅 Action 回调（使用具体的方法，方便取消订阅）
        buildActions.Place.performed += OnPlace;

        buildActions.Select1.performed += OnSelect1;
        buildActions.Select2.performed += OnSelect2;
        buildActions.Select3.performed += OnSelect3;
        buildActions.Select4.performed += OnSelect4;
    }

    private void OnDisable()
    {
        if (InputManager.Instance == null) return;

        // 取消订阅
        buildActions.Place.performed -= OnPlace;

        buildActions.Select1.performed -= OnSelect1;
        buildActions.Select2.performed -= OnSelect2;
        buildActions.Select3.performed -= OnSelect3;
        buildActions.Select4.performed -= OnSelect4;
    }

    void Update()
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Transform hitTransform = hit.transform.parent; // Cube 根节点
            print(hitTransform);

            if (hitTransform != lastHoveredCube)
            {
                // 取消之前的高亮
                if (lastHoveredCube != null)
                    lastHoveredCube.GetComponent<SelectedVisual>().SetHighlight(false);

                // 更新新的 Hover
                lastHoveredCube = hitTransform;
                lastHoveredCube.GetComponent<SelectedVisual>().SetHighlight(true);
            }
        }
        else
        {
            // 鼠标不在格子上，清除高亮
            if (lastHoveredCube != null)
            {
                lastHoveredCube.GetComponent<SelectedVisual>().SetHighlight(false);
                lastHoveredCube = null;
            }
        }
    }


    // ------- Input Action callbacks -------
    private void OnPlace(InputAction.CallbackContext ctx)
    {
        if (lastHoveredCube == null) return;

        CubeGrid cubeGrid = lastHoveredCube.GetComponentInParent<CubeGrid>();
        if (cubeGrid == null) return; // 没有CubeGrid组件就直接退出

        if (cubeGrid.isOccupied)
        {
            Debug.Log(cubeGrid.name + " is occupied!");
            return;
        }

        Vector3 spawnPos = lastHoveredCube.position + new Vector3(0.5f, 0.5f, 0.5f);
        Instantiate(GetPrefabForBuild(currentBuild), spawnPos, Quaternion.identity, cubeGrid.transform);

        cubeGrid.isOccupied = true;
    }


    private void OnSelect1(InputAction.CallbackContext ctx) => SetCurrentBuild(WhatToBuild.Glucose);
    private void OnSelect2(InputAction.CallbackContext ctx) => SetCurrentBuild(WhatToBuild.BloodVessel);
    private void OnSelect3(InputAction.CallbackContext ctx) => SetCurrentBuild(WhatToBuild.Wall);
    private void OnSelect4(InputAction.CallbackContext ctx) => SetCurrentBuild(WhatToBuild.Tower);

    private void SetCurrentBuild(WhatToBuild newBuild)
    {
        currentBuild = newBuild;
        Debug.Log("当前选择建造: " + currentBuild);
        // TODO: 在这里可以触发 UI 更新，例如更新 HUD 上的图标或文字
    }

    GameObject GetPrefabForBuild(WhatToBuild buildType)
    {
        switch (buildType)
        {
            case WhatToBuild.Glucose: return glucoseCollectorCellPrefab;
            case WhatToBuild.BloodVessel: return bloodVesselPrefab;
            case WhatToBuild.Wall: return wallPrefab;
            case WhatToBuild.Tower: return towerPrefab;
            default: return glucoseCollectorCellPrefab;
        }
    }
}
