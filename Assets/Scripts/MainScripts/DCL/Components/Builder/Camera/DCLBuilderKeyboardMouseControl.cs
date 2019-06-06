using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DCLBuilderKeyboardMouseControl : MonoBehaviour
{


    public DCLBuilderCameraInput cameraInput;

    [Header("Mouse modal mapping")]
    public int mouseOrbitButton = 0;

    public int mousePanButton = 1;

    [Header("Axis mapping")]
    public string orbitXAxisName = "Mouse X";

    public string orbitYAxisName = "Mouse Y";

    public string panXAxisName = "Mouse X";

    public string panYAxisName = "Mouse Y";

    public string zoomAxisName = "Mouse ScrollWheel";

    [Header("Key mapping")]
    public KeyCode[] resetKeys;

    public KeyCode[] zoomInKeys;

    public KeyCode[] zoomOutKeys;

    public KeyCode[] moveForwardKeys;

    public KeyCode[] moveBackwardsKeys;

    public KeyCode[] moveRightKeys;

    public KeyCode[] moveLeftKeys;


    void Update()
    {
        // One timers with AnyKeyDown
        cameraInput.SetButtonInput(DCLBuilderCameraInput.TButton.RESET, GetAnyKeyDown(resetKeys));
        cameraInput.SetButtonInput(DCLBuilderCameraInput.TButton.ZOOM_IN, GetAnyKeyDown(zoomInKeys));
        cameraInput.SetButtonInput(DCLBuilderCameraInput.TButton.ZOOM_OUT, GetAnyKeyDown(zoomOutKeys));

        // Continuous behaviour with AnyKey
        cameraInput.SetButtonInput(DCLBuilderCameraInput.TButton.MOVE_FORWARD, GetAnyKey(moveForwardKeys));
        cameraInput.SetButtonInput(DCLBuilderCameraInput.TButton.MOVE_BACKWARDS, GetAnyKey(moveBackwardsKeys));
        cameraInput.SetButtonInput(DCLBuilderCameraInput.TButton.MOVE_RIGHT, GetAnyKey(moveRightKeys));
        cameraInput.SetButtonInput(DCLBuilderCameraInput.TButton.MOVE_LEFT, GetAnyKey(moveLeftKeys));

        // Zoom is always enabled
        cameraInput.SetAxisInput(DCLBuilderCameraInput.TAxis.ZOOM, Input.GetAxis(zoomAxisName));

        if (Input.GetMouseButton(mouseOrbitButton)) // Orbit has priority over panning
        {
            cameraInput.SetAxisInput(DCLBuilderCameraInput.TAxis.ORBIT_X, Input.GetAxis(orbitXAxisName));
            cameraInput.SetAxisInput(DCLBuilderCameraInput.TAxis.ORBIT_Y, Input.GetAxis(orbitYAxisName));
            cameraInput.SetAxisInput(DCLBuilderCameraInput.TAxis.PAN_X, 0f);
            cameraInput.SetAxisInput(DCLBuilderCameraInput.TAxis.PAN_Y, 0f);
        }
        else if (Input.GetMouseButton(mousePanButton)) // Panning stops orbit input
        {
            cameraInput.SetAxisInput(DCLBuilderCameraInput.TAxis.ORBIT_X, 0f);
            cameraInput.SetAxisInput(DCLBuilderCameraInput.TAxis.ORBIT_Y, 0f);
            cameraInput.SetAxisInput(DCLBuilderCameraInput.TAxis.PAN_X, Input.GetAxis(panXAxisName));
            cameraInput.SetAxisInput(DCLBuilderCameraInput.TAxis.PAN_Y, Input.GetAxis(panYAxisName));
        }
        else // Both disabled
        {
            cameraInput.SetAxisInput(DCLBuilderCameraInput.TAxis.ORBIT_X, 0f);
            cameraInput.SetAxisInput(DCLBuilderCameraInput.TAxis.ORBIT_Y, 0f);
            cameraInput.SetAxisInput(DCLBuilderCameraInput.TAxis.PAN_X, 0f);
            cameraInput.SetAxisInput(DCLBuilderCameraInput.TAxis.PAN_Y, 0f);
        }
    }

    private bool GetAnyKeyDown(IReadOnlyList<KeyCode> keys)
    {
        for (int i = 0; i < keys.Count; ++i)
        {
            if (Input.GetKeyDown(keys[i])) return true;
        }

        return false;
    }

    private bool GetAnyKey(IReadOnlyList<KeyCode> keys)
    {
        for (int i = 0; i < keys.Count; ++i)
        {
            if (Input.GetKey(keys[i])) return true;
        }

        return false;
    }

    private bool GetAnyKeyUp(IReadOnlyList<KeyCode> keys)
    {
        for (int i = 0; i < keys.Count; ++i)
        {
            if (Input.GetKeyUp(keys[i])) return true;
        }

        return false;
    }
}
