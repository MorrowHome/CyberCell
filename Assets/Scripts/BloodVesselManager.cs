using UnityEngine;

public class BloodVesselManager : MonoBehaviour
{
    public static BloodVesselManager bloodVesselManager;

    public int totalCountOfBloodVessels = 0;

    public float floatSpeed = 1.0f;

    private void Awake()
    {
        bloodVesselManager = this;
    }




}
