using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Controllers;
using DCL.Models;
using UnityEngine;

public class BiwSceneMetricsAnalyticsHelper
{
    private string currentExceededLimitTypes = "";

    private IParcelScene scene;

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
        SendSceneLimitExceededAnalyticsEvent();
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
    }
}