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
    public Button nextTurnBtn;  // NEXT TURN 按钮
    public Button btnBuildVessel;
    public Button btnBuildDefenseCell;
    public Button btnBuildProdCell;
    public Button btnEmergency;

    [Header("Game Values")]
    public int ap = 7;
    public int maxAp = 10;
    public float glucose = 0.5f; // 0 ~ 1 (百分比)
    public int currentWave = 1;

    private bool isBuildTurn = true; // 默认从建造回合开始

    void Start()
    {
        if (apSlider) { apSlider.maxValue = maxAp; apSlider.wholeNumbers = true; }
        if (glucoseSlider) glucoseSlider.minValue = 0f;
        if (glucoseSlider) glucoseSlider.maxValue = 1f;

        // 绑定 NEXT TURN 按钮
        if (nextTurnBtn) nextTurnBtn.onClick.AddListener(OnNextTurn);

        // 建造按钮
        if (btnBuildVessel) btnBuildVessel.onClick.AddListener(OnBuildVessel);
        if (btnBuildDefenseCell) btnBuildDefenseCell.onClick.AddListener(OnBuildDefenseCell);
        if (btnBuildProdCell) btnBuildProdCell.onClick.AddListener(OnBuildProdCell);

        // 紧急按钮
        if (btnEmergency) btnEmergency.onClick.AddListener(OnEmergency);

        // 初始进入建造回合
        ShowBuild();
        UpdateUI();
    }

    void UpdateUI()
    {
        if (apSlider) apSlider.value = GameManager.Instance.actionPoints;
        if (apText) apText.text = $"{GameManager.Instance.actionPoints} / {GameManager.Instance.maxActionPoints}";

        if (glucoseSlider) glucoseSlider.value = glucose;
        if (glucoseText) glucoseText.text = $"{Mathf.RoundToInt(GameManager.Instance.glucoseAmount * 100)}%";

        if (defenseText) defenseText.text = $"Defense: Wave {currentWave}";
    }

    // 点击 NEXT TURN 按钮
    public void OnNextTurn()
    {
        if (isBuildTurn)
        {
            ShowDefense();
            GameManager.Instance.TurnTypeSwitch();
        }
        else
        {
            ShowBuild();
            GameManager.Instance.TurnTypeSwitch();
        }
    }

    // 切换到建造回合
    public void ShowBuild()
    {
        isBuildTurn = true;

        if (buildPanel) buildPanel.SetActive(true);
        if (defensePanel) defensePanel.SetActive(false);

        // 回合开始 → AP 恢复满
        ap = maxAp;

        if (nextTurnBtn) nextTurnBtn.GetComponentInChildren<TMP_Text>().text = "NEXT TURN → Defense";
        UpdateUI();
    }

    // 切换到防御回合
    public void ShowDefense()
    {
        isBuildTurn = false;

        if (buildPanel) buildPanel.SetActive(false);
        if (defensePanel) defensePanel.SetActive(true);

        // 新一波敌人来袭
        currentWave++;

        if (nextTurnBtn) nextTurnBtn.GetComponentInChildren<TMP_Text>().text = "NEXT TURN → Build";
        UpdateUI();
    }

    // 建造血管
    public void OnBuildVessel()
    {
        if (ap <= 0) return;
        ap -= 2;
        glucose -= 0.05f;
        if (glucose < 0) glucose = 0;
        UpdateUI();
    }

    // 建造防御细胞
    public void OnBuildDefenseCell()
    {
        if (ap <= 0) return;
        ap -= 2;
        glucose -= 0.08f;
        if (glucose < 0) glucose = 0;
        UpdateUI();
    }

    // 建造生产细胞
    public void OnBuildProdCell()
    {
        if (ap <= 0) return;
        ap -= 1;
        glucose += 0.1f;
        if (glucose > 1f) glucose = 1f;
        UpdateUI();
    }

    // 紧急操作（例如释放一次防御技能，不切回合）
    public void OnEmergency()
    {
        // 消耗所有 AP
        ap = 0;

        // 示例：紧急技能可以恢复一点血糖/触发特效
        glucose += 0.2f;
        if (glucose > 1f) glucose = 1f;

        UpdateUI();
    }

    // 测试：按空格恢复 AP（调试用，可删）
    void Update()
    {
            UpdateUI();
    }
}
