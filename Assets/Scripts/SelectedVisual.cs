using UnityEngine;

public class SelectedVisual : MonoBehaviour
{
    [SerializeField] private GameObject selectedVisual;

    private void Awake()
    {
        if (selectedVisual != null)
            selectedVisual.SetActive(false);
    }

    // �ⲿ����ͳһ�����������Ǳ�¶ bool
    public void SetHighlight(bool active)
    {
        if (selectedVisual != null)
            selectedVisual.SetActive(active);
    }
}
