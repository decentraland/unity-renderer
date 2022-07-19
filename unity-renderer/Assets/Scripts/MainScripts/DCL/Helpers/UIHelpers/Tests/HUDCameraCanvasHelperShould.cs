using NUnit.Framework;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

public class HUDCameraCanvasHelperShould
{
    private Canvas canvas;
    private HUDCanvasCameraModeController hudCanvasCameraModeController;
    private BaseVariable<Camera> hudCameraVariable;
    private Camera hudCamera;

    [SetUp]
    public void SetUp()
    {
        hudCameraVariable = new BaseVariable<Camera>();

        canvas = new GameObject().AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        hudCanvasCameraModeController = new HUDCanvasCameraModeController(canvas, hudCameraVariable);

        hudCamera = new GameObject().AddComponent<Camera>();
    }

    [TearDown]
    public void TearDown() { Object.Destroy(canvas.gameObject); }

    [Test]
    public void BeInitializedOnConstruction()
    {
        Assert.AreEqual(canvas, hudCanvasCameraModeController.canvas);
        Assert.AreEqual(hudCameraVariable, hudCanvasCameraModeController.hudCameraVariable);
        Assert.AreEqual(1, hudCameraVariable.OnChangeListenersCount());
    }

    [Test]
    public void SetOverlayModeIfHUDCameraIsNull()
    {
        canvas.renderMode = RenderMode.WorldSpace;

        hudCameraVariable.Set(null, true);

        Assert.AreEqual(RenderMode.ScreenSpaceOverlay, canvas.renderMode);
    }

    [Test]
    public void SetCameraModeAndAssignCamera()
    {

        canvas.renderMode = RenderMode.WorldSpace;

        hudCameraVariable.Set(hudCamera);

        Assert.AreEqual(RenderMode.ScreenSpaceCamera, canvas.renderMode);
        Assert.AreEqual(hudCamera, canvas.worldCamera);
    }

    [Test]
    public void Dispose()
    {
        hudCanvasCameraModeController.Dispose();

        Assert.AreEqual(0, hudCameraVariable.OnChangeListenersCount());
    }

}