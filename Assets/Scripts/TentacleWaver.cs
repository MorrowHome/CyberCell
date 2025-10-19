using UnityEngine;

public class TentacleWaver : MonoBehaviour
{
    [Header("Tentacles")]
    public Transform[] tentacles; // 将 Tentacle_0 ~ Tentacle_9 拖到这里

    [Header("摆动参数")]
    public float swingAmplitude = 15f; // 摆动角度幅度
    public float swingSpeed = 2f;      // 摆动速度
    public float randomPhaseRange = 2f; // 随机相位偏移范围，让每根触手不同步

    private float[] initialRotations;
    private float[] phaseOffsets;

    void Start()
    {
        int count = tentacles.Length;
        initialRotations = new float[count];
        phaseOffsets = new float[count];

        // 保存每根触手的初始旋转角度
        for (int i = 0; i < count; i++)
        {
            if (tentacles[i] != null)
            {
                initialRotations[i] = tentacles[i].localEulerAngles.z;
                phaseOffsets[i] = Random.Range(0f, randomPhaseRange);
            }
        }
    }

    void Update()
    {
        for (int i = 0; i < tentacles.Length; i++)
        {
            if (tentacles[i] != null)
            {
                float swing = Mathf.Sin(Time.time * swingSpeed + phaseOffsets[i]) * swingAmplitude;
                Vector3 newRotation = tentacles[i].localEulerAngles;
                newRotation.z = initialRotations[i] + swing;
                tentacles[i].localEulerAngles = newRotation;
            }
        }
    }
}
