using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmartItemBooleanParameterAdapter : SmartItemUIParameterAdapter
{
    public Toggle boolParameterToggle;

    const string parameterType = "boolean";

    public override void SetParameter(SmartItemParameter parameter)
    {
        base.SetParameter(parameter);

        if (parameter.type != parameterType)
            return;

        boolParameterToggle.gameObject.SetActive(true);

        bool defaultParameter = false;
        bool.TryParse(parameter.@default, out defaultParameter);

        boolParameterToggle.isOn = defaultParameter;
    }
}
