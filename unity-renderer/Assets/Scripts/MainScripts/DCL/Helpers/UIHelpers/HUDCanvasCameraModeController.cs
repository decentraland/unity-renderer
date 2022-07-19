using System;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// This helper will prepare the canvas to work in screen space or camera space if the proper DataStore is set
/// </summary>
public class HUDCanvasCameraModeController : IDisposable
{
    internal readonly Canvas canvas;
    internal readonly BaseVariable<Camera> hudCameraVariable;

    public HUDCanvasCameraModeController(Canvas canvas, BaseVariable<Camera> hudCameraVariable)
    {
        Assert.IsNotNull(canvas);
        Assert.IsNotNull(hudCameraVariable);

        this.canvas = canvas;
        this.hudCameraVariable = hudCameraVariable;
        this.hudCameraVariable.OnChange += UpdateCanvas;
        UpdateCanvas(this.hudCameraVariable.Get(), null);
    }

    internal void UpdateCanvas(Camera newCamera, Camera oldCamera)
    {
        canvas.renderMode = newCamera == null ? RenderMode.ScreenSpaceOverlay : RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = newCamera;
    }

    public void Dispose() { hudCameraVariable.OnChange -= UpdateCanvas; }
}