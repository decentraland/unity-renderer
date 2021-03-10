using DCL.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SmartItemSliderParameter : SmartItemUIParameterAdapter
{
    public TextMeshProUGUI valueTxt;
    public Slider sliderParameter;

    private float amountOfSteps;
    private float minValue;
    private float maxValue;
    private float stepSum;

    public override void SetInfo()
    {
        base.SetInfo();

        minValue = float.Parse(currentParameter.min);
        maxValue = float.Parse(currentParameter.max);

        stepSum = float.Parse(currentParameter.step, CultureInfo.InvariantCulture);

        amountOfSteps = Mathf.RoundToInt((maxValue - minValue) / stepSum);

        sliderParameter.minValue = 0;
        sliderParameter.maxValue = amountOfSteps;

        int currentValue = ConvertParameterValueToSliderValue((float) GetParameterValue());

        sliderParameter.value = currentValue;

        SetSliderText(sliderParameter.value);

        sliderParameter.onValueChanged.AddListener(OnValueChange);
    }

    public void OnValueChange(float value)
    {
        SetSliderText(value);
        SetParameterValue(ConvertSliderValueToParameterValue(value));
    }

    private void SetSliderText(float value)
    {
        float convertedValue = ConvertSliderValueToParameterValue(value);
        valueTxt.text = convertedValue.ToString();     
    }

    private float ConvertSliderValueToParameterValue(float value)
    {
        return minValue + value * stepSum;
    }

    private int ConvertParameterValueToSliderValue(float value)
    {
        return (int)Mathf.Abs((value - minValue) / stepSum);
    }

    protected override object GetParameterValue()
    {
        object value = base.GetParameterValue();
        float sliderValue;

        float.TryParse(currentParameter.min, out sliderValue);     
  
        if(value is string stringValue)
        {
            if (!string.IsNullOrEmpty(stringValue))
            {
                if(float.TryParse(stringValue, out float defaultValue))
                    return defaultValue;
            }
            else
            {
                return sliderValue;
            }
        }
        else if (value is float sliderValueFloat)
        {
            return sliderValueFloat;
        }
        else
        {
            sliderValue = sliderParameter.minValue;
        }

        return sliderValue;
    }
}
