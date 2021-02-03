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

    float amountOfSteps;
    float minValue;
    float maxValue;
    float stepSum;

    const string parameterType = "slider";

    public override void SetParameter(SmartItemParameter parameter)
    {
        base.SetParameter(parameter);

        if (parameter.type != parameterType)
            return;

        minValue = float.Parse(parameter.min);
        maxValue = float.Parse(parameter.max);

        stepSum = float.Parse(parameter.step, CultureInfo.InvariantCulture);


        amountOfSteps = Mathf.RoundToInt((maxValue - minValue) / stepSum);

        sliderParameter.minValue = 0;
        sliderParameter.maxValue = amountOfSteps;

        if(!string.IsNullOrEmpty(parameter.@default))
        {
            float defaultValue = sliderParameter.minValue;
            float.TryParse((string) parameter.@default, out defaultValue);

            int defaultValueConverted = (int) Mathf.Abs(defaultValue + minValue - Mathf.CeilToInt(maxValue / amountOfSteps));

            sliderParameter.value = defaultValueConverted;

        }
        else
        {
            sliderParameter.value = sliderParameter.minValue;
        }


        SetSliderText(sliderParameter.value);

        sliderParameter.onValueChanged.AddListener(OnValueChange);
    }


    public void OnValueChange(float value)
    {
        SetSliderText(value);
    }

    void SetSliderText(float value)
    {
        float convertedValue = minValue + value * stepSum;
        valueTxt.text = convertedValue.ToString();
    }
}
