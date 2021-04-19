using DCL.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SmartItemFloatParameter : SmartItemUIParameterAdapter
{
    public TMP_InputField textParameterInputField;

    private void Start()
    {
        textParameterInputField.onEndEdit.AddListener(OnValueChange);
    }

    public override void SetInfo()
    {
        base.SetInfo();

        textParameterInputField.gameObject.SetActive(true);
        
        textParameterInputField.contentType = TMP_InputField.ContentType.DecimalNumber;
        textParameterInputField.text = GetParameterValue().ToString();
    }

    public void OnValueChange(string text)
    {
        if(float.TryParse(text,out float result))
            SetParameterValue(result);
    }
}
