using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class TMPFontOnToggle : UIToggle
{
    public TextMeshProUGUI targetText;

    public TMP_FontAsset onFont;

    public TMP_FontAsset offFont;

    protected override void OnValueChanged(bool isOn)
    {
        targetText.font = isOn ? onFont : offFont;
    }
}