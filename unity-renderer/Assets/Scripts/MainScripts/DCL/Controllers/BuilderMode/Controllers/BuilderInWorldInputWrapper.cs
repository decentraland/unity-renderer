using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using UnityEngine;
using UnityEngine.EventSystems;

public class BIWInputWrapper : BIWController, IBIWInputWrapper
{
    private const float MS_CLICK_THRESHOLD = 500;
    private const float MOVEMENT_CLICK_THRESHOLD = 50;
    private const float MOUSE_WHEEL_THROTTLE = 0.1f;

    private const string MOUSE_X_AXIS = "Mouse X";
    private const string MOUSE_Y_AXIS = "Mouse Y";
    private const string MOUSE_SCROLLWHEEL = "Mouse ScrollWheel";

    public static event Action<int, Vector3> OnMouseClick;
    public static event Action<int, Vector3> OnMouseClickOnUI;
    public static event Action<int, Vector3> OnMouseDown;
    public static event Action<int, Vector3> OnMouseUp;
    public static event Action<int, Vector3> OnMouseUpOnUI;

    public static event Action<float> OnMouseWheel;

    public static event OnMouseDragDelegate OnMouseDrag;
    public static event OnMouseDragDelegateRaw OnMouseDragRaw;

    public delegate void OnMouseDragDelegate(int buttonId, Vector3 position, float axisX, float axisY);

    public delegate void OnMouseDragDelegateRaw(int buttonId, Vector3 position, float axisX, float axisY);

    internal float lastTimeMouseDown = 0;
    internal Vector3 lastMousePosition;
    internal bool canInputBeMade = true;
    internal bool currentClickIsOnUi = false;
    internal int lastMouseWheelAxisDirection = 0;
    internal float lastMouseWheelTime = 0;

    public override void Update()
    {
        base.Update();
        for (int i = 0; i <= 2; i++)
        {
            if (HasMouseButtonInput(i))
                break;
        }

        UpdateMouseWheelInput();
    }

    public override void Dispose()
    {
        base.Dispose();

        currentClickIsOnUi = false;
    }

    internal bool HasMouseButtonInput(int button)
    {
        if (Input.GetMouseButtonDown(button))
        {
            MouseDown(button, Input.mousePosition);
            return true;
        }

        if (Input.GetMouseButton(button))
        {
            MouseDrag(button, Input.mousePosition, Input.GetAxis(MOUSE_X_AXIS), Input.GetAxis(MOUSE_Y_AXIS));
            MouseRawDrag(button, Input.mousePosition, Input.GetAxis(MOUSE_X_AXIS), Input.GetAxis(MOUSE_Y_AXIS));
            return true;
        }

        if (Input.GetMouseButtonUp(button))
        {
            MouseUp(button, Input.mousePosition);
            return true;
        }

        return false;
    }

    internal void OnMouseWheelInput(float axisValue)
    {
        int axisDirection = (int)Mathf.Sign(axisValue);
        if (lastMouseWheelAxisDirection == axisDirection)
        {
            if (Time.unscaledTime - lastMouseWheelTime >= MOUSE_WHEEL_THROTTLE)
                SetMouseWheelDelta(axisValue, axisDirection);
        }
        else
        {
            SetMouseWheelDelta(axisValue, axisDirection);
        }
    }

    internal void SetMouseWheelDelta(float axisValue, int axisDirection)
    {
        MouseWheel(axisValue);
        lastMouseWheelTime = Time.unscaledTime;
        lastMouseWheelAxisDirection = axisDirection;
    }

    internal void UpdateMouseWheelInput()
    {
        float axisValue = Input.GetAxis(MOUSE_SCROLLWHEEL);
        if (axisValue != 0)
            OnMouseWheelInput(axisValue);
    }

    public void StopInput() { canInputBeMade = false; }

    public void ResumeInput() { canInputBeMade = true; }

    internal void MouseUp(int buttonId, Vector3 mousePosition)
    {
        if (!isEditModeActive)
            return;

        if (currentClickIsOnUi)
        {
            OnMouseClickOnUI?.Invoke(buttonId, mousePosition);
            currentClickIsOnUi = false;
            return;
        }

        if (!canInputBeMade)
            return;

        if (!BIWUtils.IsPointerOverUIElement())
            MouseUpInvoke(buttonId, mousePosition);
        else
            MouseUpOnGUI(buttonId, mousePosition);
    }

    internal void MouseUpInvoke(int buttonId, Vector3 mousePosition)
    {
        OnMouseUp?.Invoke(buttonId, mousePosition);
        if (Vector3.Distance(mousePosition, lastMousePosition) >= MOVEMENT_CLICK_THRESHOLD)
            return;
        if (Time.unscaledTime >= lastTimeMouseDown + MS_CLICK_THRESHOLD / 1000)
            return;
        OnMouseClick?.Invoke(buttonId, mousePosition);
    }

    internal void MouseUpOnGUI(int buttonId, Vector3 mousePosition) { OnMouseUpOnUI?.Invoke(buttonId, mousePosition); }

    internal void MouseDown(int buttonId, Vector3 mousePosition)
    {
        if (!isEditModeActive)
            return;

        lastTimeMouseDown = Time.unscaledTime;
        lastMousePosition = mousePosition;
        currentClickIsOnUi = BIWUtils.IsPointerOverUIElement();

        if (!canInputBeMade)
            return;
        if (!currentClickIsOnUi)
            OnMouseDown?.Invoke(buttonId, mousePosition);
    }

    internal void MouseWheel(float axisValue)
    {
        if (!isEditModeActive)
            return;

        if (!canInputBeMade)
            return;
        if (!BIWUtils.IsPointerOverUIElement())
            OnMouseWheel?.Invoke(axisValue);
    }

    internal void MouseDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
    {
        if (!isEditModeActive)
            return;

        if (!CanDrag())
            return;

        OnMouseDrag?.Invoke(buttonId, mousePosition, axisX, axisY);
    }

    internal void MouseRawDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
    {
        if (!isEditModeActive)
            return;

        if (!CanDrag())
            return;

        OnMouseDragRaw?.Invoke(buttonId, mousePosition, axisX, axisY);
    }

    private bool CanDrag()
    {
        if (!canInputBeMade ||
            currentClickIsOnUi ||
            BIWUtils.IsPointerOverUIElement())
            return false;

        return true;
    }
}