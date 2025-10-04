using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("ResourcesCollection")]
    public float glucoseAmount = 0f;


    public static GameManager Instance;

    [SerializeField] public int actionPoints = 20;
    [SerializeField] public int maxActionPoints = 40;
    [SerializeField] public int actionPointsPerTurn = 20;
    public enum TurnType
    {
        BuildTime = 0,
        DefenseTime = 1,

    }
    public TurnType currentTurnType = TurnType.BuildTime;


    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 30;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool HasEnoughPoints(int cost)
    {
        return actionPoints >= cost;
    }

    public void SpendPoints(int cost)
    {
        actionPoints -= cost;
        if (actionPoints < 0) actionPoints = 0;
    }

    public void GainPoints(int amount)
    {
        actionPoints = Mathf.Min(actionPoints + amount, maxActionPoints);
    }

    public void BuildTimeStart()
    {
        actionPoints += actionPointsPerTurn;
    }

    public void DefenseTimeStart()
    {

    }







    public void TurnTypeSwitch()
    {
        if (currentTurnType == TurnType.BuildTime)
        {
            currentTurnType = TurnType.DefenseTime;
            DefenseTimeStart();
        }
        else
        {
            currentTurnType = TurnType.BuildTime;
            BuildTimeStart();
        }
    }



}
