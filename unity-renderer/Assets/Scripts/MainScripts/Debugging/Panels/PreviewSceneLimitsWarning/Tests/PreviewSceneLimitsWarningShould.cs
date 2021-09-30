using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using NSubstitute;
using NUnit.Framework;

public class PreviewSceneLimitsWarningShould
{
    private const string SCENE_ID = "Temptation";

    private PreviewSceneLimitsWarning previewSceneLimitsWarning;
    private IParcelScene scene;
    private IWorldState worldState;
    private SceneMetricsModel metrics = new SceneMetricsModel();
    private SceneMetricsModel limit = new SceneMetricsModel();
    private Dictionary<string, IParcelScene> scenes;

    [SetUp]
    public void SetUp()
    {
        worldState = Substitute.For<IWorldState>();

        previewSceneLimitsWarning = Substitute.ForPartsOf<PreviewSceneLimitsWarning>(worldState);
        scene = Substitute.For<IParcelScene>();

        scenes = new Dictionary<string, IParcelScene>() { { SCENE_ID, scene } };

        ISceneMetricsController sceneMetrics = Substitute.For<ISceneMetricsController>();
        sceneMetrics.GetModel().Returns(metrics);
        sceneMetrics.GetLimits().Returns(limit);

        scene.metricsController.Returns(sceneMetrics);
        worldState.loadedScenes.Returns(scenes);
    }

    [TearDown]
    public void TearDown()
    {
        KernelConfig.i.ClearPromises();
    }

    [Test]
    public void ShowNotificationWhenLimitExceeded()
    {
        limit.entities = 1;
        metrics.entities = 2;

        var kernelConfig = KernelConfig.i.Get().Clone();
        kernelConfig.debugConfig.sceneLimitsWarningSceneId = SCENE_ID;
        KernelConfig.i.Set(kernelConfig);

        previewSceneLimitsWarning.SetActive(true);
        previewSceneLimitsWarning.HandleWarningNotification();

        Assert.IsTrue(previewSceneLimitsWarning.isShowingNotification);
    }

    [Test]
    public void HideNotificationWhenGoesBelowLimit()
    {
        limit.entities = 1;
        metrics.entities = 2;

        var kernelConfig = KernelConfig.i.Get().Clone();
        kernelConfig.debugConfig.sceneLimitsWarningSceneId = SCENE_ID;
        KernelConfig.i.Set(kernelConfig);

        previewSceneLimitsWarning.SetActive(true);
        previewSceneLimitsWarning.HandleWarningNotification();

        Assert.IsTrue(previewSceneLimitsWarning.isShowingNotification);

        limit.entities = metrics.entities;
        previewSceneLimitsWarning.HandleWarningNotification();

        Assert.IsFalse(previewSceneLimitsWarning.isShowingNotification);
    }

    [Test]
    public void HideNotificationWhenDisabled()
    {
        limit.entities = 1;
        metrics.entities = 2;

        var kernelConfig = KernelConfig.i.Get().Clone();
        kernelConfig.debugConfig.sceneLimitsWarningSceneId = SCENE_ID;
        KernelConfig.i.Set(kernelConfig);

        previewSceneLimitsWarning.SetActive(true);
        previewSceneLimitsWarning.HandleWarningNotification();

        Assert.IsTrue(previewSceneLimitsWarning.isShowingNotification);

        previewSceneLimitsWarning.SetActive(false);

        Assert.IsFalse(previewSceneLimitsWarning.isShowingNotification);
    }

    [Test]
    public void HideNotificationWhenDisposed()
    {
        limit.entities = 1;
        metrics.entities = 2;

        var kernelConfig = KernelConfig.i.Get().Clone();
        kernelConfig.debugConfig.sceneLimitsWarningSceneId = SCENE_ID;
        KernelConfig.i.Set(kernelConfig);

        previewSceneLimitsWarning.SetActive(true);
        previewSceneLimitsWarning.HandleWarningNotification();

        Assert.IsTrue(previewSceneLimitsWarning.isShowingNotification);

        previewSceneLimitsWarning.Dispose();

        Assert.IsFalse(previewSceneLimitsWarning.isShowingNotification);
    }
}