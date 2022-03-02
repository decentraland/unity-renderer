using DCL;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TexCompressDebugController : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI text;

    void Awake()
    {
        slider.onValueChanged.AddListener((float value) =>
        {
            int finalInt = (int)value;
            text.text = finalInt.ToString();
            DataStore.i.debugConfig.minimumFramesWaitBetweenTexCompression = finalInt;
        });
    }
}
