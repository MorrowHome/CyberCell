using UnityEditor.Rendering;
using UnityEngine;

public class BloodVessel : MonoBehaviour
{

    [SerializeField] private Material connected;
    [SerializeField] private Material disconnected;
    [SerializeField] private GameObject myVisual;





    [SerializeField] private bool isConnected = false;

    private BloodVessel bloodVessel;

    private float glucoseAmount = 0;


    private void Start()
    {
            
    }

    private void Update()
    {
        if (isConnected)
        {
            myVisual.GetComponent<MeshRenderer>().material = connected;
        }
        else
        {
            myVisual.GetComponent<MeshRenderer>().material = disconnected;
        }
    }
}
