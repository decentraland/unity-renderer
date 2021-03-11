using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmartItemBooleanParameterAdapter : SmartItemUIParameterAdapter
{
    public Toggle boolParameterToggle;

    private void Start()
    {
        boolParameterToggle.onValueChanged.AddListener(OnValueChange);
    }

    public override void SetInfo()
    {
        base.SetInfo();

        boolParameterToggle.gameObject.SetActive(true);

        if(bool.TryParse(GetParameterValue().ToString(), out bool defaultParameter))
            boolParameterToggle.isOn = defaultParameter;
    }

    public void OnValueChange(bool value)
    {
        SetParameterValue(value);
    }
}
