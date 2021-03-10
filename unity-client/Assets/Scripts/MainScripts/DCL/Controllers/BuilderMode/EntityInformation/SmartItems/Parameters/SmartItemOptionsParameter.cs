using DCL.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SmartItemOptionsParameter : SmartItemUIParameterAdapter
{
    public TMP_Dropdown dropDown;

    private void Start()
    {
        dropDown.onValueChanged.AddListener(OnValueChange);
    }

    public override void SetInfo()
    {
        base.SetInfo();

        dropDown.options = new List<TMP_Dropdown.OptionData>();

        List<string> optionsLabelList = new List<string>();
        foreach(SmartItemParameter.OptionsParameter options in currentParameter.options)
        {
            optionsLabelList.Add(options.label);
        }

        dropDown.AddOptions(optionsLabelList);

        string value = (string) GetParameterValue();

        for(int i = 0; i < currentParameter.options.Length;i++)
        {
            if (currentParameter.options[i].value == value) dropDown.SetValueWithoutNotify(i);
        }
    }

    private void OnValueChange(int currentIndex)
    {
        foreach (SmartItemParameter.OptionsParameter options in currentParameter.options)
        {
            if(options.label == dropDown.options[currentIndex].text)
                SetParameterValue(options.value);
        }      
    }
}
