using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    [SerializeField] private Camera cam;

    [Header("建造 Prefabs")]
    [SerializeField] private GameObject glucoseCollectorCellPrefab; // 1
    [SerializeField] private GameObject bloodVesselPrefab;          // 2
    [SerializeField] private GameObject wallPrefab;                 // 3
    [SerializeField] private GameObject immuneBCellPrefab;          // 4

    [SerializeField] private LayerMask cubeGridLayer;

    public EventHandler OnPlaceSomething;

    private Transform lastHoveredCube;
    private PlayerInputActions.BuildActions buildActions;

    public enum WhatToBuild { Collector = 1, BloodVessel = 2, Wall = 3, Tower = 4 }
    [SerializeField] private WhatToBuild currentBuild = WhatToBuild.Collector;

    // === 新增：引用 Nanobot（场景中应始终存在） ===
    [Header("建造机器人")]
    [SerializeField] private Nanobot nanobot;

    // === 新增：删除模式 ===
    private enum BuildMode { Build, Delete }
    private BuildMode currentMode = BuildMode.Build;

    private void Awake()
    {
        Instance = this;
        if (InputManager.Instance == null)
        {
            Debug.LogError("InputManager not found in scene.");
            return;
        }
        buildActions = InputManager.Instance.inputActions.Build;
    }

    private void OnEnable()
    {
        buildActions = InputManager.Instance.inputActions.Build;
        buildActions.Place.performed += OnPlace;

        buildActions.Select1.performed += ctx => SetCurrentBuild(WhatToBuild.Collector);
        buildActions.Select2.performed += ctx => SetCurrentBuild(WhatToBuild.BloodVessel);
        buildActions.Select3.performed += ctx => SetCurrentBuild(WhatToBuild.Wall);
        buildActions.Select4.performed += ctx => SetCurrentBuild(WhatToBuild.Tower);

        // 新增按键绑定
        buildActions.BuildMode.performed += ctx => SetMode(BuildMode.Build);   // B 键切建造模式
        buildActions.DeleteMode.performed += ctx => SetMode(BuildMode.Delete); // X 键切删除模式
    }

    private void OnDisable()
    {
        buildActions.Place.performed -= OnPlace;
        buildActions.BuildMode.performed -= ctx => SetMode(BuildMode.Build);
        buildActions.DeleteMode.performed -= ctx => SetMode(BuildMode.Delete);
    }

    private void Update()
    {
        // 如果鼠标在 UI 上，就不做高亮逻辑
        if (EventSystem.current.IsPointerOverGameObject())
        {
            if (lastHoveredCube != null)
            {
                lastHoveredCube.GetComponent<SelectedVisual>()?.SetHighlight(false);
                lastHoveredCube = null;
            }
            return;
        }

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Transform hitTransform = hit.transform.parent;
            if (hitTransform != lastHoveredCube)
            {
                if (lastHoveredCube != null)
                    lastHoveredCube.GetComponent<SelectedVisual>()?.SetHighlight(false);

                lastHoveredCube = hitTransform;
                lastHoveredCube.GetComponent<SelectedVisual>()?.SetHighlight(true);
            }
        }
        else
        {
            if (lastHoveredCube != null)
            {
                lastHoveredCube.GetComponent<SelectedVisual>()?.SetHighlight(false);
                lastHoveredCube = null;
            }
        }
    }

    private void OnPlace(InputAction.CallbackContext ctx)
    {


        // 如果鼠标在 UI 上，就不执行建造/删除
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (lastHoveredCube == null) return;

        CubeGrid cubeGrid = lastHoveredCube.GetComponentInParent<CubeGrid>();
        if (cubeGrid == null) return;

        if (currentMode == BuildMode.Build)
        {
            if (cubeGrid.isOccupied) return;

            Vector3 spawnPos = lastHoveredCube.position + new Vector3(0.5f, 0.5f, 0.5f);
            GameObject prefab = GetPrefabForBuild(currentBuild);
            IActionPointCost iActionPointCost = prefab.GetComponent<IActionPointCost>();
            if (iActionPointCost == null) return;

            if (nanobot != null)
            {
                nanobot.AssignBuildTask(spawnPos, prefab, cubeGrid, iActionPointCost.ActionPointCost);
            }
            else
            {
                Debug.LogWarning("Nanobot 未绑定，直接建造。");
                GameObject ins = Instantiate(prefab, spawnPos, Quaternion.identity, cubeGrid.transform);
                cubeGrid.whatIsOnMe = ins.transform;
                cubeGrid.isOccupied = true;

                BloodVessel vessel = ins.GetComponent<BloodVessel>();
                if (vessel != null)
                    vessel.Init();

                BloodVesselManager.bloodVesselManager.RefreshAllConnections();
                OnPlaceSomething?.Invoke(this, EventArgs.Empty);
            }
        }
        else if (currentMode == BuildMode.Delete)
        {
            if (!cubeGrid.isOccupied) return;
            RemoveObject(cubeGrid);
        }
    }

    private void RemoveObject(CubeGrid cubeGrid)
    {
        if (cubeGrid.whatIsOnMe == null) return;

        BloodVessel vessel = cubeGrid.whatIsOnMe.GetComponent<BloodVessel>();
        if (vessel != null)
        {
            Destroy(vessel.gameObject);
        }
        else
        {
            Destroy(cubeGrid.whatIsOnMe.gameObject);
        }

        cubeGrid.whatIsOnMe = null;
        cubeGrid.isOccupied = false;
        BloodVesselManager.bloodVesselManager.RefreshAllConnections();
        OnPlaceSomething?.Invoke(this, EventArgs.Empty);
    }

    public void OnBuildFinished()
    {
        BloodVesselManager.bloodVesselManager.RefreshAllConnections();
        OnPlaceSomething?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveBloodVessel(CubeGrid cubeGrid)
    {
        if (cubeGrid.whatIsOnMe == null) return;

        BloodVessel vessel = cubeGrid.whatIsOnMe.GetComponent<BloodVessel>();
        if (vessel != null)
        {
            Destroy(vessel.gameObject);
            cubeGrid.whatIsOnMe = null;
            cubeGrid.isOccupied = false;

            BloodVesselManager.bloodVesselManager.RefreshAllConnections();
        }
    }

    private void SetCurrentBuild(WhatToBuild newBuild)
    {
        currentBuild = newBuild;
        currentMode = BuildMode.Build; // 自动切回建造模式
        Debug.Log("当前选择建造: " + currentBuild);
    }

    private void SetMode(BuildMode mode)
    {
        currentMode = mode;
        Debug.Log("当前模式: " + currentMode);
    }

    private GameObject GetPrefabForBuild(WhatToBuild buildType)
    {
        return buildType switch
        {
            WhatToBuild.Collector => glucoseCollectorCellPrefab,
            WhatToBuild.BloodVessel => bloodVesselPrefab,
            WhatToBuild.Wall => wallPrefab,
            WhatToBuild.Tower => immuneBCellPrefab,
            _ => glucoseCollectorCellPrefab,
        };
    }
}
