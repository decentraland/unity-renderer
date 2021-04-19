using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SmartItemTextParameter : SmartItemUIParameterAdapter
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

        textParameterInputField.contentType = TMP_InputField.ContentType.Standard;

        textParameterInputField.SetTextWithoutNotify((string)GetParameterValue());
    }

    public void OnValueChange(string text)
    {
        SetParameterValue(text);
    }
}