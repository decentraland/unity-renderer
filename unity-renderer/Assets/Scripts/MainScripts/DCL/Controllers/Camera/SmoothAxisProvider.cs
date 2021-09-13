using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Cinemachine.Utility;
using DCL.Camera;
using UnityEngine;

public class SmoothAxisProvider : MonoBehaviour, AxisState.IInputAxisProvider
{
    public Vector3 dampTime;

    private Vector3 axis = new Vector3();
    private Vector3 axisTarget = new Vector3();

    public InputAction_Measurable axisX;
    public InputAction_Measurable axisY;
    private InputSpikeFixer[] inputSpikeFixer;

    private void Awake()
    {
        inputSpikeFixer = new []
        {
            new InputSpikeFixer(() => Cursor.lockState),
            new InputSpikeFixer(() => Cursor.lockState)
        };
    }
    void Update()
    {
        axisTarget[0] = axisX.GetValue();
        axisTarget[1] = axisY.GetValue();
        axis += Damper.Damp(axisTarget - axis, dampTime, Time.deltaTime);
    }

    public float GetAxisValue(int axis)
    {
        return inputSpikeFixer[axis].GetValue(this.axis[axis]);
    }
}