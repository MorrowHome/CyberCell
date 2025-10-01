using UnityEngine;
using UnityEngine.InputSystem;

public class Test : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject glucoseCollectorCellPrefab; // ������ɵ�Prefab
    private Transform currentSelected;

    void Update()
    {
        // ���߼�����λ��
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Transform hitCube = hit.transform; // �����е� Cube

            // �� Cube �������µ� Selected
            Transform selectedChild = hitCube.transform.parent.Find("Selected");
            if (selectedChild == null)
            {
                Debug.LogWarning("Cube ��û����Ϊ Selected ��������");
                return;
            }

            // ���µ�ǰѡ��
            if (currentSelected != selectedChild)
            {
                if (currentSelected != null)
                    currentSelected.gameObject.SetActive(false);

                currentSelected = selectedChild;
                currentSelected.gameObject.SetActive(true);
            }

            Debug.Log("����: " + hitCube.name);

            // ����������Prefab
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                // ������ Cube ����
                Vector3 spawnPos = hitCube.position + new Vector3(0.5f, 0.5f, 0.5f);
                Instantiate(glucoseCollectorCellPrefab, spawnPos, Quaternion.identity, hitCube.parent);
            }
        }
        else
        {
            // û����ʱ������һ��
            if (currentSelected != null)
            {
                currentSelected.gameObject.SetActive(false);
                currentSelected = null;
            }
        }
    }
}
