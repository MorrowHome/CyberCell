using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Top Bars")]
    public Slider apSlider;
    public TMP_Text apText;
    public Slider glucoseSlider;
    public TMP_Text glucoseText;
    public TMP_Text defenseText;

    [Header("Panels")]
    public GameObject buildPanel;
    public GameObject defensePanel;

    [Header("Buttons")]
    public Button nextTurnBtn;
    public Button btnBuildVessel;
    public Button btnBuildDefenseCell;
    public Button btnBuildProdCell;
    public Button btnEmergency;

    private void Start()
    {
        // 初始化 UI
        UpdateActionPoints(GameManager.Instance.ActionPoints);


        // 注册事件监听
        GameManager.Instance.OnTurnChanged += OnTurnChanged;
        GameManager.Instance.OnActionPointsChanged += UpdateActionPoints;

        // 按钮绑定
        nextTurnBtn.onClick.AddListener(OnNextTurn);
        btnBuildVessel.onClick.AddListener(() => SpendAP(2));
        btnBuildDefenseCell.onClick.AddListener(() => SpendAP(2));
        btnBuildProdCell.onClick.AddListener(() => SpendAP(1));
        btnEmergency.onClick.AddListener(OnEmergency);

        OnTurnChanged(GameManager.Instance.CurrentTurn);
    }

    private void Update()
    {
        UpdateGlucose(GameManager.Instance.GlucoseConcentration);
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnTurnChanged -= OnTurnChanged;
        GameManager.Instance.OnActionPointsChanged -= UpdateActionPoints;
    }

    private void OnNextTurn() => GameManager.Instance.SwitchTurn();

    private void OnTurnChanged(GameManager.TurnType turn)
    {
        bool isBuild = turn == GameManager.TurnType.BuildTime;

        buildPanel.SetActive(isBuild);
        defensePanel.SetActive(!isBuild);
        defenseText.text = isBuild ? "Build Phase" : "Defense Phase";

        nextTurnBtn.GetComponentInChildren<TMP_Text>().text =
            isBuild ? "NEXT TURN → Defense" : "NEXT TURN → Build";
    }

    private void SpendAP(int cost)
    {
        if (GameManager.Instance.HasEnoughPoints(cost))
            GameManager.Instance.SpendPoints(cost);
    }

    private void UpdateActionPoints(int ap)
    {
        apSlider.value = ap;
        apText.text = $"{ap} / {GameManager.Instance.MaxActionPoints}";
    }

    private void UpdateGlucose(float amount)
    {
        glucoseSlider.value = amount;
        glucoseText.text = $"{Mathf.RoundToInt(amount)}%";
    }

    private void OnEmergency()
    {
        GameManager.Instance.SpendPoints(GameManager.Instance.ActionPoints);
        GameManager.Instance.glucoseAmount = Mathf.Min(1f, GameManager.Instance.glucoseAmount + 0.2f);
        UpdateGlucose(GameManager.Instance.glucoseAmount);
    }
}
