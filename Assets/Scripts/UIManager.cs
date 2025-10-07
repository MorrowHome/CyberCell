using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("🔹 Top Bars")]
    public Slider apSlider;
    public TMP_Text apText;
    public Slider glucoseSlider;
    public TMP_Text glucoseText;
    public TMP_Text phaseText;

    [Header("🔹 Panels")]
    public GameObject buildPanel;
    public GameObject defensePanel;

    [Header("🔹 Buttons")]
    public Button nextTurnBtn;
    public Button btnBuildCollector;
    public Button btnBuildBloodVessel;
    public Button btnBuildWall;
    public Button btnBuildTower;

    private void Start()
    {
        // === 检查组件 ===
        if (GameManager.Instance == null)
        {
            Debug.LogError("[UIManager] GameManager not found in scene!");
            return;
        }

        if (BuildManager.Instance == null)
        {
            Debug.LogError("[UIManager] BuildManager not found in scene!");
            return;
        }

        // === 初始化 UI ===
        UpdateActionPoints(GameManager.Instance.ActionPoints);
        UpdateGlucose(GameManager.Instance.GlucoseConcentration);
        OnTurnChanged(GameManager.Instance.CurrentTurn);

        // === 注册事件 ===
        GameManager.Instance.OnActionPointsChanged += UpdateActionPoints;
        GameManager.Instance.OnTurnChanged += OnTurnChanged;

        // === 按钮事件 ===
        nextTurnBtn.onClick.AddListener(OnNextTurn);
        btnBuildCollector.onClick.AddListener(() => SetBuildType(1));
        btnBuildBloodVessel.onClick.AddListener(() => SetBuildType(2));
        btnBuildWall.onClick.AddListener(() => SetBuildType(3));
        btnBuildTower.onClick.AddListener(() => SetBuildType(4));
    }

    private void Update()
    {
        // 实时更新 Glucose 显示
        if (GameManager.Instance != null)
            UpdateGlucose(GameManager.Instance.GlucoseConcentration);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnActionPointsChanged -= UpdateActionPoints;
            GameManager.Instance.OnTurnChanged -= OnTurnChanged;
        }
    }

    // === 切换回合按钮 ===
    private void OnNextTurn()
    {
        GameManager.Instance.SwitchTurn();
    }

    // === 更新当前阶段（Build / Defense） ===
    private void OnTurnChanged(GameManager.TurnType turn)
    {
        bool isBuild = turn == GameManager.TurnType.BuildTime;

        if (buildPanel != null) buildPanel.SetActive(isBuild);
        if (defensePanel != null) defensePanel.SetActive(!isBuild);
        if (phaseText != null)
            phaseText.text = isBuild ? "Build Phase" : "Defense Phase";

        if (nextTurnBtn != null)
        {
            var label = nextTurnBtn.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = isBuild ? "NEXT TURN → Defense" : "NEXT TURN → Build";
        }
    }

    // === 更新 AP 显示 ===
    private void UpdateActionPoints(int ap)
    {
        if (apSlider != null)
            apSlider.value = ap;
        if (apText != null)
            apText.text = $"{ap} / {GameManager.Instance.MaxActionPoints}";
    }

    // === 更新 Glucose 显示 ===
    private void UpdateGlucose(float amount)
    {
        if (glucoseSlider != null)
            glucoseSlider.value = amount;
        if (glucoseText != null)
            glucoseText.text = $"{Mathf.RoundToInt(amount)}%";
    }

    // === 建造按钮功能（连接 BuildManager） ===
    private void SetBuildType(int id)
    {
        if (BuildManager.Instance == null) return;

        string typeName = id switch
        {
            1 => "Collector",
            2 => "BloodVessel",
            3 => "Wall",
            4 => "Tower",
            _ => null
        };

        if (typeName != null)
        {
            BuildManager.Instance.SendMessage("SetCurrentBuild",
                System.Enum.Parse(typeof(BuildManager.WhatToBuild), typeName));
            Debug.Log($"[UIManager] Selected build type: {typeName}");
        }
    }
}
