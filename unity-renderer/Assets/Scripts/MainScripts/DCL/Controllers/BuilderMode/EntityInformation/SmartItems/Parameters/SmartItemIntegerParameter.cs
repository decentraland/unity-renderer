using DCL.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SmartItemIntegerParameter : SmartItemUIParameterAdapter
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

        textParameterInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        textParameterInputField.text = GetParameterValue().ToString();
    }

    public void OnValueChange(string text)
    {
        SetParameterValue(Convert.ToInt32(text));
    }
}