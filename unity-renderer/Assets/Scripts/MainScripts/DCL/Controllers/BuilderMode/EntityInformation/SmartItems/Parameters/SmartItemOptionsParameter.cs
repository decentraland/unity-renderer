using DCL.Components;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SmartItemOptionsParameter : SmartItemUIParameterAdapter
{
    public TMP_Dropdown dropDown;

    const string parameterType = "options";

    public override void SetParameter(SmartItemParameter parameter)
    {
        base.SetParameter(parameter);

        if (parameter.type != parameterType)
            return;


        dropDown.options = new List<TMP_Dropdown.OptionData>();

        List<string> optionsLabelList = new List<string>();
        foreach(SmartItemParameter.OptionsParameter options in parameter.options)
        {
            optionsLabelList.Add(options.label);
        }

        dropDown.AddOptions(optionsLabelList); 
    }
}
