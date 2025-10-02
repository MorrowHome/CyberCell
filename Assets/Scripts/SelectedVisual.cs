using UnityEngine;

public class SelectedVisual : MonoBehaviour
{
    [SerializeField] private GameObject selectedVisual;

    private void Awake()
    {
        if (selectedVisual != null)
            selectedVisual.SetActive(false);
    }

    // 外部调用统一方法，而不是暴露 bool
    public void SetHighlight(bool active)
    {
        if (selectedVisual != null)
            selectedVisual.SetActive(active);
    }
}
