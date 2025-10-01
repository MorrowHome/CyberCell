using UnityEngine;
using UnityEngine.InputSystem;

public class Test : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject glucoseCollectorCellPrefab; // 左键生成的Prefab
    private Transform currentSelected;

    void Update()
    {
        // 射线检测鼠标位置
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Transform hitCube = hit.transform; // 被射中的 Cube

            // 找 Cube 父物体下的 Selected
            Transform selectedChild = hitCube.transform.parent.Find("Selected");
            if (selectedChild == null)
            {
                Debug.LogWarning("Cube 下没有名为 Selected 的子物体");
                return;
            }

            // 更新当前选中
            if (currentSelected != selectedChild)
            {
                if (currentSelected != null)
                    currentSelected.gameObject.SetActive(false);

                currentSelected = selectedChild;
                currentSelected.gameObject.SetActive(true);
            }

            Debug.Log("射中: " + hitCube.name);

            // 鼠标左键生成Prefab
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                // 生成在 Cube 中心
                Vector3 spawnPos = hitCube.position + new Vector3(0.5f, 0.5f, 0.5f);
                Instantiate(glucoseCollectorCellPrefab, spawnPos, Quaternion.identity, hitCube.parent);
            }
        }
        else
        {
            // 没射中时隐藏上一个
            if (currentSelected != null)
            {
                currentSelected.gameObject.SetActive(false);
                currentSelected = null;
            }
        }
    }
}
