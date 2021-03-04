using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.SettingsData;
using UnityEngine;

/// <summary>
/// An implementation of an AutoQualityController for capped FPS
/// </summary>
public class AutoQualityCappedFPSController : IAutoQualityController
{
    internal const int EVALUATIONS_SIZE = 5;
    internal const float INCREASE_MARGIN = 0.9f;
    internal const float STAY_MARGIN = 0.8f;

    internal int targetFPS;
    internal int currentQualityIndex;
    internal readonly QualitySettingsData qualitySettings;

    internal readonly List<float> fpsEvaluations = new List<float>();

    public AutoQualityCappedFPSController(int targetFPS, int startIndex, QualitySettingsData qualitySettings)
    {
        this.targetFPS = targetFPS;
        currentQualityIndex = startIndex;
        this.qualitySettings = qualitySettings;
    }

    public int EvaluateQuality(PerformanceMetricsData metrics)
    {
        if (metrics == null) return currentQualityIndex;

        fpsEvaluations.Add(metrics.fpsCount);
        if (fpsEvaluations.Count <= EVALUATIONS_SIZE)
            return currentQualityIndex;

        fpsEvaluations.RemoveAt(0);
        float performance = fpsEvaluations.Average() / targetFPS;

        int newCurrentQualityIndex = currentQualityIndex;
        if (performance < STAY_MARGIN)
            newCurrentQualityIndex = Mathf.Max(0, currentQualityIndex - 1);

        if (performance >= INCREASE_MARGIN)
            newCurrentQualityIndex = Mathf.Min(qualitySettings.Length - 1, currentQualityIndex + 2); //We increase quality more aggressively than we reduce

        if (newCurrentQualityIndex != currentQualityIndex)
            ResetEvaluation();

        currentQualityIndex = newCurrentQualityIndex;
        return currentQualityIndex;
    }

    public void ResetEvaluation()
    {
        fpsEvaluations.Clear();
    }
}