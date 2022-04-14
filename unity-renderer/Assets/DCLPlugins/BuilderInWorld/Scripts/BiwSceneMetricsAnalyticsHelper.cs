using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;

public class BiwSceneMetricsAnalyticsHelper
{
    private const float MS_BETWEEN_METRICS_EVENT = 2000f;
    private string currentExceededLimitTypes = "";

    private IParcelScene scene;
    private float lastTimeAnalitycsSent = 0;
    private SceneMetricsModel lastSceneMetrics = new SceneMetricsModel();
    
    public BiwSceneMetricsAnalyticsHelper(IParcelScene sceneOwner)
    {
        this.scene = sceneOwner;
        scene.metricsCounter.OnMetricsUpdated += OnMetricsUpdated;
    }

    public void Dispose()
    {
        scene.metricsCounter.OnMetricsUpdated -= OnMetricsUpdated;
    }

    private void OnMetricsUpdated(ISceneMetricsCounter obj)
    {
        if (Time.unscaledTime < lastTimeAnalitycsSent + MS_BETWEEN_METRICS_EVENT / 1000f)
            return;
        
        SendSceneLimitExceededAnalyticsEvent();
        lastTimeAnalitycsSent = Time.unscaledTime;
    }

    private void SendSceneLimitExceededAnalyticsEvent()
    {
        if (scene.metricsCounter.IsInsideTheLimits())
        {
            currentExceededLimitTypes = "";
            return;
        }

        var metricsModel = scene.metricsCounter.currentCount;
        var metricsLimits = scene.metricsCounter.maxCount;

        string exceededLimits = BIWAnalytics.GetLimitsPassedArray(metricsModel, metricsLimits);

        if (exceededLimits != currentExceededLimitTypes)
        {
            BIWAnalytics.SceneLimitsExceeded(metricsModel, metricsLimits);
            currentExceededLimitTypes = exceededLimits;
        }

        lastSceneMetrics = metricsModel;
    }
}