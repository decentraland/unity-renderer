using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using DCL.Camera;
using UnityEngine;

public class OverrideCinemachineAxisInput : MonoBehaviour
{

    [Serializable]
    public struct AxisToMeasurableAction
    {
        public string axisName;
        public InputAction_Measurable measurableAction;
    }

    [SerializeField] private AxisToMeasurableAction[] axisToMeasurableActions;
    private Dictionary<string, InputAction_Measurable> cachedAxisToMeasurableActions;
    private InputSpikeFixer inputSpikeFixer;

    private void Awake()
    {
        cachedAxisToMeasurableActions = axisToMeasurableActions.ToDictionary(x => x.axisName, x => x.measurableAction);
        CinemachineCore.GetInputAxis = OverrideGetAxis;
        inputSpikeFixer = new InputSpikeFixer();
    }

    private float OverrideGetAxis(string axisName)
    {
        if (!cachedAxisToMeasurableActions.ContainsKey(axisName))
            return 0;
        
        float value = cachedAxisToMeasurableActions[axisName].GetValue();
        return inputSpikeFixer.GetValue(value);
    }
    
}