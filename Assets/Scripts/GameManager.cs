using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("ResourcesCollection")]
    public float glucoseAmount = 0f;


    public static GameManager Instance;

    public int currentMode;
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


}
