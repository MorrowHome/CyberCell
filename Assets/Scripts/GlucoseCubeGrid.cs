using UnityEngine;

public class GlucoseCubeGrid : MonoBehaviour
{
    [Header("资源参数")]
    [SerializeField] private float glucoseAmount;
    [SerializeField] private float MINAMOUNT = 50f;
    [SerializeField] private float MAXAMOUNT = 500f;

    [Header("格子引用")]
    [SerializeField] private GameObject emptyGridPrefab; // 普通格子 prefab



    private void Start()
    {
        glucoseAmount = Random.Range(MINAMOUNT, MAXAMOUNT);
    }

    private void resourceDepleted()
    {
        if (emptyGridPrefab != null)
        {
            // 在当前位置生成新的普通格子
            GameObject newGrid = Instantiate(
                emptyGridPrefab,
                transform.position,
                transform.rotation,
                transform.parent // 保持父对象一致
            );

            // 将当前格子的所有子物体迁移到新格子下
            // 使用临时列表避免迭代中修改父对象导致问题
            Transform[] children = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                if(transform.GetChild(i).name == "Visual" || transform.GetChild(i).name != "SelectedVisual") continue;
                children[i] = transform.GetChild(i);

            }
            foreach (Transform child in children)
            {
                child.SetParent(newGrid.transform, true); // true 保持世界坐标

            }
        }

        // 删除当前格子
        Destroy(gameObject);
    }

    public void AmountDecrease(float num)
    {
        glucoseAmount -= num;
        if (glucoseAmount <= 0)
        {
            resourceDepleted();
        }
    }
}
