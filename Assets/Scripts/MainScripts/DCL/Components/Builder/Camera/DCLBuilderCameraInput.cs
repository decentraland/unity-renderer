using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DCLBuilderCameraInput : ScriptableObject
{
    public enum TAxis
    {
        ORBIT_X,
        ORBIT_Y,
        PAN_X,
        PAN_Y,
        ZOOM
    }

    public enum TButton
    {
        RESET,
        ZOOM_IN,
        ZOOM_OUT,
        MOVE_FORWARD,
        MOVE_BACKWARDS,
        MOVE_RIGHT,
        MOVE_LEFT
    }

    [Header("Current input")]
    [SerializeField]
    private float[] axisInput = new float[Enum.GetValues(typeof(TAxis)).Length];

    [SerializeField]
    private bool[] buttonInput = new bool[Enum.GetValues(typeof(TButton)).Length];

    public event Action<TAxis, float> OnAxisInputChanged;
    public event Action<TButton, bool> OnButtonInputChanged;

    public float GetAxis(TAxis axis)
    {
        return axisInput[(int)axis];
    }

    public bool GetButton(TButton button)
    {
        return buttonInput[(int)button];
    }

    public void SetAxisInput(TAxis axis, float input)
    {
        int index = (int)axis;
        if (Mathf.Approximately(input, axisInput[index])) return;

        axisInput[index] = input;
        OnAxisInputChanged?.Invoke(axis, input);
    }

    public void SetButtonInput(TButton button, bool input)
    {
        int index = (int)button;
        if (input == buttonInput[index]) return;

        buttonInput[index] = input;
        OnButtonInputChanged?.Invoke(button, input);
    }
}