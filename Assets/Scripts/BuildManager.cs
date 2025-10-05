using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;


    [SerializeField] private Camera cam;

    [Header("建造 Prefabs")]
    [SerializeField] private GameObject glucoseCollectorCellPrefab; // 1
    [SerializeField] private GameObject bloodVesselPrefab;          // 2
    [SerializeField] private GameObject wallPrefab;                 // 3
    [SerializeField] private GameObject immuneBCellPrefab;                // 4

    [SerializeField] private LayerMask cubeGridLayer;

    public EventHandler OnPlaceSomething;

    private Transform lastHoveredCube;

    private PlayerInputActions.BuildActions buildActions;

    private enum WhatToBuild { Collector = 1, BloodVessel = 2, Wall = 3, Tower = 4 }

    [SerializeField] private WhatToBuild currentBuild = WhatToBuild.Collector;

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
    }

    private void OnDisable()
    {
        buildActions.Place.performed -= OnPlace;
    }

    private void Update()
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Transform hitTransform = hit.transform.parent;
            if (hitTransform != lastHoveredCube)
            {
                if (lastHoveredCube != null)
                    lastHoveredCube.GetComponent<SelectedVisual>().SetHighlight(false);

                lastHoveredCube = hitTransform;
                lastHoveredCube.GetComponent<SelectedVisual>().SetHighlight(true);
            }
        }
        else
        {
            if (lastHoveredCube != null)
            {
                lastHoveredCube.GetComponent<SelectedVisual>().SetHighlight(false);
                lastHoveredCube = null;
            }
        }
    }

    private void OnPlace(InputAction.CallbackContext ctx)
    {
        if (lastHoveredCube == null) return;

        CubeGrid cubeGrid = lastHoveredCube.GetComponentInParent<CubeGrid>();
        if (cubeGrid == null || cubeGrid.isOccupied) return;
        

        Vector3 spawnPos = lastHoveredCube.position + new Vector3(0.5f, 0.5f, 0.5f);

        IActionPointCost iActionPointCost = GetPrefabForBuild(currentBuild).GetComponent<IActionPointCost>();
        if(iActionPointCost!=null)
        {
            if (!GameManager.Instance.HasEnoughPoints(iActionPointCost.ActionPointCost)) return;
            OnPlaceSomething?.Invoke(this, EventArgs.Empty);
            GameManager.Instance.SpendPoints(iActionPointCost.ActionPointCost);
            GameObject ins = Instantiate(GetPrefabForBuild(currentBuild), spawnPos, Quaternion.identity, cubeGrid.transform);
            cubeGrid.whatIsOnMe = ins.transform;
            cubeGrid.isOccupied = true;
            // 如果是血管，立即初始化注册并刷新 BFS
            BloodVessel vessel = ins.GetComponent<BloodVessel>();
            if (vessel != null)
            {
                vessel.Init();
                BloodVesselManager.bloodVesselManager.RefreshAllConnections();
            }
        }
        
        
        

        
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
        Debug.Log("当前选择建造: " + currentBuild);
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
