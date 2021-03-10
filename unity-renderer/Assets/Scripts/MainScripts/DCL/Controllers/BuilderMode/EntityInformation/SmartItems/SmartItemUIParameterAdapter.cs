using DCL.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SmartItemUIParameterAdapter : MonoBehaviour
{

    public TextMeshProUGUI labelTxt;
    public Action<SmartItemParameter> OnParameterChange;

    protected SmartItemParameter currentParameter;

    protected string KEY_NAME;

    protected Dictionary<object, object> currentValues;

    public virtual void SetParameter(SmartItemParameter parameter, Dictionary<object, object> values)
    {
        currentParameter = parameter;
        labelTxt.text = parameter.label;
        KEY_NAME = parameter.label;
        currentValues = values;
        SetInfo();
    }

    public virtual void SetInfo()
    {

    }

    public virtual void ChangeParameter()
    {
        OnParameterChange?.Invoke(currentParameter);
    }

    protected virtual void SetParameterValue(object value)
    {
        if (currentValues.ContainsKey(KEY_NAME))        
            currentValues[KEY_NAME] = value;
        
        else       
            currentValues.Add(KEY_NAME, value);
        
    }

    protected virtual object GetParameterValue()
    {
        if (currentValues.ContainsKey(KEY_NAME))
            return currentValues[KEY_NAME];

        if (!string.IsNullOrEmpty(currentParameter.@default))
            return currentParameter.@default;
        
        return null;
    }
}
