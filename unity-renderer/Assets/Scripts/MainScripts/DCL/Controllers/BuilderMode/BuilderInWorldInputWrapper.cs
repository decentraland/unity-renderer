using Builder;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuilderInWorldInputWrapper : MonoBehaviour
{
    public float msClickThreshold = 200;
    public float movementClickThreshold = 50;

    public static event Action<int, Vector3> OnMouseClick;
    public static event Action<int, Vector3> OnMouseDown;
    public static event Action<int, Vector3> OnMouseUp;

    public static event Action<float> OnMouseWheel;

    public static event OnMouseDragDelegate OnMouseDrag;
    public static event OnMouseDragDelegateRaw OnMouseDragRaw;

    public delegate void OnMouseDragDelegate(int buttonId, Vector3 position, float axisX, float axisY);
    public delegate void OnMouseDragDelegateRaw(int buttonId, Vector3 position, float axisX, float axisY);

    private float lastTimeMouseDown = 0;
    private Vector3 lastMousePosition;
    private bool canInputBeMade = true;
    private bool currentClickIsOnUi = false;

    private void Awake()
    {
        DCLBuilderInput.OnMouseDrag += MouseDrag;
        DCLBuilderInput.OnMouseRawDrag += MouseRawDrag;
        DCLBuilderInput.OnMouseWheel += MouseWheel;
        DCLBuilderInput.OnMouseDown += MouseDown;
        DCLBuilderInput.OnMouseUp += MouseUp;
    }

    private void OnDestroy()
    {
        DCLBuilderInput.OnMouseDrag -= MouseDrag;
        DCLBuilderInput.OnMouseRawDrag -= MouseRawDrag;
        DCLBuilderInput.OnMouseWheel -= MouseWheel;
        DCLBuilderInput.OnMouseDown -= MouseDown;
        DCLBuilderInput.OnMouseUp -= MouseUp;
    }

    public void StopInput() { canInputBeMade = false; }

    public void ResumeInput() { canInputBeMade = true; }

    private void MouseUp(int buttonId, Vector3 mousePosition)
    {
        currentClickIsOnUi = false;

        if (!canInputBeMade)
            return;

        if (!BuilderInWorldUtils.IsPointerOverUIElement())
        {
            OnMouseUp?.Invoke(buttonId, mousePosition);
            if (Vector3.Distance(mousePosition, lastMousePosition) >= movementClickThreshold)
                return;
            if (Time.unscaledTime >= lastTimeMouseDown + msClickThreshold / 1000)
                return;
            OnMouseClick?.Invoke(buttonId, mousePosition);
        }
    }

    private void MouseDown(int buttonId, Vector3 mousePosition)
    {
        lastTimeMouseDown = Time.unscaledTime;
        lastMousePosition = mousePosition;
        currentClickIsOnUi = BuilderInWorldUtils.IsPointerOverUIElement();

        if (!canInputBeMade)
            return;
        if (!currentClickIsOnUi)
            OnMouseDown?.Invoke(buttonId, mousePosition);
    }

    private void MouseWheel(float axisValue)
    {
        if (!canInputBeMade)
            return;
        if (!BuilderInWorldUtils.IsPointerOverUIElement())
            OnMouseWheel?.Invoke(axisValue);
    }

    private void MouseDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
    {
        if (!CanDrag())
            return;

        OnMouseDrag?.Invoke(buttonId, mousePosition, axisX, axisY);
    }

    private void MouseRawDrag(int buttonId, Vector3 mousePosition, float axisX, float axisY)
    {
        if (!CanDrag())
            return;

        OnMouseDragRaw?.Invoke(buttonId, mousePosition, axisX, axisY);
    }

    private bool CanDrag()
    {
        if (!canInputBeMade ||
            currentClickIsOnUi ||
            BuilderInWorldUtils.IsPointerOverUIElement())
            return false;

        return true;
    }
}