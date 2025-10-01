using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI glucoseAmountText;

    private void Update()
    {
        glucoseAmountText.text = "Glucose: " + GameManager.Instance.glucoseAmount.ToString();
    }
}
