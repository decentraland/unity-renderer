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
    private readonly KernelConfigModel kernelConfigModel = new KernelConfigModel();

    [SetUp]
    public void SetUp()
    {
        worldState = Substitute.For<IWorldState>();

        previewSceneLimitsWarning = Substitute.ForPartsOf<PreviewSceneLimitsWarning>(worldState);
        scene = Substitute.For<IParcelScene>();

        scenes = new Dictionary<string, IParcelScene>() {{SCENE_ID, scene}};

        ISceneMetricsCounter sceneMetrics = Substitute.For<ISceneMetricsCounter>();
        sceneMetrics.currentCount.Returns(metrics);
        sceneMetrics.maxCount.Returns(limit);

        scene.metricsCounter.Returns(sceneMetrics);
        worldState.GetLoadedScenes().Returns(scenes);
        worldState.TryGetScene(SCENE_ID, out Arg.Any<IParcelScene>()).Returns(param => param[1] = scene);
        
        kernelConfigModel.debugConfig.sceneLimitsWarningSceneId = SCENE_ID;
    }

    [TearDown]
    public void TearDown()
    {
        previewSceneLimitsWarning.Dispose();
    }

    [Test]
    public void ShowNotificationWhenLimitExceeded()
    {
        limit.entities = 1;
        metrics.entities = 2;

        previewSceneLimitsWarning.SetActive(true);
        previewSceneLimitsWarning.OnKernelConfigChanged(kernelConfigModel, null);
        previewSceneLimitsWarning.HandleWarningNotification();

        Assert.IsTrue(previewSceneLimitsWarning.isShowingNotification);
    }

    [Test]
    public void HideNotificationWhenGoesBelowLimit()
    {
        limit.entities = 1;
        metrics.entities = 2;

        previewSceneLimitsWarning.SetActive(true);
        previewSceneLimitsWarning.OnKernelConfigChanged(kernelConfigModel, null);
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

        previewSceneLimitsWarning.SetActive(true);
        previewSceneLimitsWarning.OnKernelConfigChanged(kernelConfigModel, null);
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

        previewSceneLimitsWarning.SetActive(true);
        previewSceneLimitsWarning.OnKernelConfigChanged(kernelConfigModel, null);
        previewSceneLimitsWarning.HandleWarningNotification();

        Assert.IsTrue(previewSceneLimitsWarning.isShowingNotification);

        previewSceneLimitsWarning.Dispose();

        Assert.IsFalse(previewSceneLimitsWarning.isShowingNotification);
    }

    [Test]
    public void HideNotificationWhenDisabledByKernelConfig()
    {
        limit.entities = 1;
        metrics.entities = 2;

        previewSceneLimitsWarning.SetActive(true);
        previewSceneLimitsWarning.OnKernelConfigChanged(kernelConfigModel, null);
        previewSceneLimitsWarning.HandleWarningNotification();

        Assert.IsTrue(previewSceneLimitsWarning.isShowingNotification);

        kernelConfigModel.debugConfig.sceneLimitsWarningSceneId = null;
        previewSceneLimitsWarning.OnKernelConfigChanged(kernelConfigModel, null);

        Assert.IsFalse(previewSceneLimitsWarning.isShowingNotification);
    }
}