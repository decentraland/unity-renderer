using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class FontOnToggle : UIToggle
{
    public Text targetText;

    public Font onFont;

    public Font offFont;

    protected override void OnValueChanged(bool isOn)
    {
        targetText.font = isOn ? onFont : offFont;
    }
}