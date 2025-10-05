using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private GameObject virusPrefab;
    [SerializeField] private Transform EnemyContainer;
    [ContextMenu("Generate a virus")]
    public void newWaveTest()
    {
        Instantiate(virusPrefab, spwanPoint, EnemyContainer);

    }



    private int waveCount = 0;


    [SerializeField] private Transform spwanPoint;


}
