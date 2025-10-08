using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("🔹 Top Bars")]
    public Slider apSlider;
    public TMP_Text apText;
    public Slider glucoseSlider;
    public TMP_Text glucoseText;
    public TMP_Text phaseText;

    [Header("🔹 Panels")]
    public GameObject buildPanel;
    public GameObject defensePanel;
    public GameObject gameOverPanel;

    [Header("🔹 Buttons")]
    public Button nextTurnBtn;
    public Button btnBuildCollector;
    public Button btnBuildBloodVessel;
    public Button btnBuildWall;
    public Button btnBuildTower;

    [Header("🔹 Hover Info")]
    public GameObject hoverPanel;           // 悬浮信息 Panel
    public TMP_Text hoverTitleText;
    public TMP_Text hoverContentText;


    [Header("Offset")]
    public Vector3 hoverOffset = new Vector3(15f, -15f, 0f);

    public Image HP;
    public TextMeshProUGUI HPTMP;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
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

        UpdateActionPoints(GameManager.Instance.ActionPoints);
        UpdateGlucose(GameManager.Instance.GlucoseConcentration);
        OnTurnChanged(GameManager.Instance.CurrentTurn);

        GameManager.Instance.OnActionPointsChanged += UpdateActionPoints;
        GameManager.Instance.OnTurnChanged += OnTurnChanged;

        nextTurnBtn.onClick.AddListener(OnNextTurn);
        btnBuildCollector.onClick.AddListener(() => SetBuildType(1));
        btnBuildBloodVessel.onClick.AddListener(() => SetBuildType(2));
        btnBuildWall.onClick.AddListener(() => SetBuildType(3));
        btnBuildTower.onClick.AddListener(() => SetBuildType(4));

        if (hoverPanel != null)
            hoverPanel.SetActive(false);
    }

    private void Update()
    {
        if (GameManager.Instance != null)
            UpdateGlucose(GameManager.Instance.GlucoseConcentration);

        UpdateHoverInfo();

        HP.fillAmount = GameManager.Instance.HP / 10f;
        HPTMP.text = "HP: " + Mathf.RoundToInt(GameManager.Instance.HP).ToString();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnActionPointsChanged -= UpdateActionPoints;
            GameManager.Instance.OnTurnChanged -= OnTurnChanged;
        }
    }

    // === 悬浮检测逻辑 ===
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask bloodVesselLayer;

    private void UpdateHoverInfo()
    {
        if (hoverPanel == null || BuildManager.Instance == null) return;

        Transform hoveredCube = BuildManager.Instance.LastHoveredCube;
        if (hoveredCube == null)
        {
            HideHoverInfo();
            return;
        }

        // 检查 Cube 上有没有 BloodVessel
        BloodVessel vessel = hoveredCube.GetComponentInChildren<BloodVessel>();
        if (vessel != null)
        {
            Vector3 mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
            ShowHoverInfo(vessel, mousePos);
        }
        else
        {
            HideHoverInfo();
        }
    }

    private void ShowHoverInfo(BloodVessel vessel, Vector3 mousePos)
    {
        if (hoverPanel == null) return;

        hoverPanel.SetActive(true);
        hoverTitleText.text = "BloodVessel";
        hoverContentText.text = $"Glucose: {vessel.GlucoseAmount:F2}\n" +
                                $"Connect: {(vessel.isConnected ? "True" : "False")}";

        // 跟随鼠标
        hoverPanel.transform.position = mousePos + hoverOffset;
    }

    private void HideHoverInfo()
    {
        if (hoverPanel != null)
            hoverPanel.SetActive(false);
    }

    // === 其他原有方法保持不变 ===
    private void OnNextTurn() => GameManager.Instance.SwitchTurn();
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

    private void UpdateActionPoints(int ap)
    {
        if (apSlider != null)
            apSlider.value = ap;
        if (apText != null)
            apText.text = $"{ap} / {GameManager.Instance.MaxActionPoints}";
    }

    private void UpdateGlucose(float amount)
    {
        if (glucoseSlider != null)
            glucoseSlider.value = amount;
        if (glucoseText != null)
            glucoseText.text = $"{Mathf.RoundToInt(amount)}%";
    }

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
        }
    }
}
