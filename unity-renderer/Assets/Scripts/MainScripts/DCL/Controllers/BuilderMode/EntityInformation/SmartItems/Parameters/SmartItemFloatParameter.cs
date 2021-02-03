using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SmartItemFloatParameter : SmartItemUIParameterAdapter
{
    public TMP_InputField textParameterInputField;

    const string parameterType = "float";

    public override void SetParameter(SmartItemParameter parameter)
    {
        base.SetParameter(parameter);

        if (parameter.type != parameterType)
            return;

        textParameterInputField.gameObject.SetActive(true);
        
        textParameterInputField.contentType = TMP_InputField.ContentType.DecimalNumber;
        textParameterInputField.text = (string) parameter.@default;
    }
}
