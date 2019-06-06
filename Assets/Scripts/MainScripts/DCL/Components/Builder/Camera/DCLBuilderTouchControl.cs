using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DCLBuilderTouchControl : MonoBehaviour
{
#if UNITY_EDITOR
    [Expandable]
#endif
    public DCLBuilderCameraInput cameraInput;

    public float touchDeltaZoomThreshold = 10f;
    public float zoomFactor = 0.1f;

    private const int orbitTouches = 1;
    private const int zoomPanTouches = 2;

    private void Start()
    {
        if (!Input.touchSupported)
        {
            enabled = false;
            Debug.LogWarning("Touch controls are not supported in this platform.");
        }
    }

    void Update()
    {
        int touchCount = Input.touchCount;
        if (touchCount == orbitTouches)
        {
            Vector2 touchDelta = Input.GetTouch(0).deltaPosition;

            cameraInput.SetAxisInput(DCLBuilderCameraInput.TAxis.ORBIT_X, touchDelta.x);
            cameraInput.SetAxisInput(DCLBuilderCameraInput.TAxis.ORBIT_Y, touchDelta.y);

            cameraInput.SetAxisInput(DCLBuilderCameraInput.TAxis.ZOOM, 0f);

            cameraInput.SetAxisInput(DCLBuilderCameraInput.TAxis.PAN_X, 0f);
            cameraInput.SetAxisInput(DCLBuilderCameraInput.TAxis.PAN_Y, 0f);
        }
        else if (touchCount == zoomPanTouches)
        {
            Vector2 touch0Delta = Input.GetTouch(0).deltaPosition;
            Vector2 touch1Delta = Input.GetTouch(1).deltaPosition;
            Vector2 combinedDeltaVector = touch1Delta - touch0Delta;
            float combinedDeltaSqrMagnitude = combinedDeltaVector.sqrMagnitude;
            if (combinedDeltaSqrMagnitude > (touchDeltaZoomThreshold * touchDeltaZoomThreshold)) // Zooming?
            {
                // TODO: use a smoothed delta value. 
                // TODO: use magnitude instead of SQR magnitude
                cameraInput.SetAxisInput(DCLBuilderCameraInput.TAxis.ZOOM, combinedDeltaSqrMagnitude * zoomFactor);

                cameraInput.SetAxisInput(DCLBuilderCameraInput.TAxis.PAN_X, 0f);
                cameraInput.SetAxisInput(DCLBuilderCameraInput.TAxis.PAN_Y, 0f);
            }
            else // Panning
            {
                cameraInput.SetAxisInput(DCLBuilderCameraInput.TAxis.ZOOM, 0f);

                cameraInput.SetAxisInput(DCLBuilderCameraInput.TAxis.PAN_X, touch0Delta.x);
                cameraInput.SetAxisInput(DCLBuilderCameraInput.TAxis.PAN_Y, touch0Delta.y);
            }

            cameraInput.SetAxisInput(DCLBuilderCameraInput.TAxis.ORBIT_X, 0f);
            cameraInput.SetAxisInput(DCLBuilderCameraInput.TAxis.ORBIT_Y, 0f);
        }
    }
}