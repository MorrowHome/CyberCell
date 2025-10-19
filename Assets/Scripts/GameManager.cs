using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Resource Collection")]
    public float glucoseAmount = 0f;

    [Header("Action Points")]
    [SerializeField] private int actionPoints = 20;
    [SerializeField] private int maxActionPoints = 40;
    [SerializeField] private int actionPointsPerTurn = 20;
    [SerializeField] private float glucoseConcentration = 500f;

    [SerializeField] public float HP = 10f;
    private int waveCounts = 0;
    
    public float GlucoseConcentration => glucoseConcentration;

    public int ActionPoints => actionPoints;
    public int MaxActionPoints => maxActionPoints;

    public enum TurnType { BuildTime, DefenseTime }
    public TurnType CurrentTurn { get; private set; } = TurnType.BuildTime;

    public event Action<TurnType> OnTurnChanged; // 用事件驱动 UI
    public event Action<int> OnActionPointsChanged;

    private void Update()
    {
        glucoseConcentration = glucoseAmount / BloodVesselManager.bloodVesselManager.bloodVesselCount;
    }

    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
    }

    // === 行动力管理 ===
    public bool HasEnoughPoints(int cost) => actionPoints >= cost;

    public void SpendPoints(int cost)
    {
        actionPoints = Mathf.Max(0, actionPoints - cost);
        OnActionPointsChanged?.Invoke(actionPoints);
    }

    public void GainPoints(int amount)
    {
        actionPoints = Mathf.Min(actionPoints + amount, maxActionPoints);
        OnActionPointsChanged?.Invoke(actionPoints);
    }

    // === 回合切换 ===
    public void SwitchTurn()
    {
        waveCounts++;
        if (CurrentTurn == TurnType.BuildTime)
        {
            CurrentTurn = TurnType.DefenseTime;
            StartDefensePhase();
        }
        else
        {
            CurrentTurn = TurnType.BuildTime;
            StartBuildPhase();
        }

        OnTurnChanged?.Invoke(CurrentTurn);
    }

    private void StartBuildPhase()
    {
        GainPoints(actionPointsPerTurn);
        foreach (var grid in MapGenerator.Instance.allGrids)
        {
            grid.transform.TryGetComponent<CubeGrid>(out var cubeGrid);
            //cubeGrid.myVisual.TryGetComponent<MeshRenderer>(out var aaa);
            //aaa.enabled = true;
        }
        ObjectPoolManager.Instance.AsleepAll();
    }

    private void StartDefensePhase()
    {
        foreach (var grid in MapGenerator.Instance.allGrids)
        {
            grid.transform.TryGetComponent<CubeGrid>(out var cubeGrid);
            cubeGrid.myVisual.TryGetComponent<MeshRenderer>(out var aaa);
            aaa.enabled = false;
        }
        EnemyManager.Instance.StartNewWave(waveCounts * 10);
    }

    public void TakeDamage(float dmg)
    {
        HP -= dmg;
        if (HP <= 0f)
        {
            UIManager.Instance.gameOverPanel.SetActive(true);
        }
    }
}
