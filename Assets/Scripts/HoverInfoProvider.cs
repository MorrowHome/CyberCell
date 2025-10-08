using UnityEngine;

public class HoverInfoProvider : MonoBehaviour
{
    [TextArea]
    public string infoText; // 简单文本信息，可在Inspector中写

    // 如果是动态信息（比如血管浓度），可以重写这个函数
    public virtual string GetHoverInfo()
    {
        return infoText;
    }
}
