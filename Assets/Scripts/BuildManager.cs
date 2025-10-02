// BuildManager.cs
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class BuildManager : MonoBehaviour
{
    [SerializeField] private Camera cam;

    [Header("���� Prefabs")]
    [SerializeField] private GameObject glucoseCollectorCellPrefab; // 1
    [SerializeField] private GameObject bloodVesselPrefab;          // 2
    [SerializeField] private GameObject wallPrefab;                 // 3
    [SerializeField] private GameObject towerPrefab;                // 4


    [SerializeField] private LayerMask cubeGridLayer;

    private Transform currentSelected;      // ��ʾ Selected ������
    private Transform lastHoveredCube;      // ���һ���������е� Cube (���� Place �ص�)

    private PlayerInputActions.BuildActions buildActions; // �󶨵� InputManager.instance.inputActions.Build

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

        // �õ� Build action map��ע�⣺���� generated wrapper �Ľṹ��
        buildActions = InputManager.Instance.inputActions.Build;
    }

    private void OnEnable()
    {
        // ����ȷ���õ� BuildActions�������л� / �༭ģʽ�±Ƚ��Ƚ���
        if (InputManager.Instance == null) return;
        buildActions = InputManager.Instance.inputActions.Build;

        // ���� Action �ص���ʹ�þ���ķ���������ȡ�����ģ�
        buildActions.Place.performed += OnPlace;

        buildActions.Select1.performed += OnSelect1;
        buildActions.Select2.performed += OnSelect2;
        buildActions.Select3.performed += OnSelect3;
        buildActions.Select4.performed += OnSelect4;
    }

    private void OnDisable()
    {
        if (InputManager.Instance == null) return;

        // ȡ������
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
            Transform hitTransform = hit.transform.parent; // Cube ���ڵ�
            print(hitTransform);

            if (hitTransform != lastHoveredCube)
            {
                // ȡ��֮ǰ�ĸ���
                if (lastHoveredCube != null)
                    lastHoveredCube.GetComponent<SelectedVisual>().SetHighlight(false);

                // �����µ� Hover
                lastHoveredCube = hitTransform;
                lastHoveredCube.GetComponent<SelectedVisual>().SetHighlight(true);
            }
        }
        else
        {
            // ��겻�ڸ����ϣ��������
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
        if (cubeGrid == null) return; // û��CubeGrid�����ֱ���˳�

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
        Debug.Log("��ǰѡ����: " + currentBuild);
        // TODO: ��������Դ��� UI ���£�������� HUD �ϵ�ͼ�������
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
