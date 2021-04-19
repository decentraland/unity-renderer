using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.FPSDisplay;
using DCL.SettingsData;
using UnityEngine;

/// <summary>
/// An implementation of an AutoQualityController for uncapped FPS
/// </summary>
public class AutoQualityUncappedFPSController : IAutoQualityController
{
    private const int EVALUATIONS_SIZE = 5;

    internal int currentQualityIndex;
    internal readonly QualitySettingsData qualitySettings;

    internal readonly List<float> fpsEvaluations = new List<float>();

    public AutoQualityUncappedFPSController(int startIndex, QualitySettingsData qualitySettings)
    {
        currentQualityIndex = startIndex;
        this.qualitySettings = qualitySettings;
    }

    public int EvaluateQuality(PerformanceMetricsData metrics)
    {
        if (metrics == null) return currentQualityIndex;

        //TODO refine this evaluation
        fpsEvaluations.Add(metrics.fpsCount);
        if (fpsEvaluations.Count <= EVALUATIONS_SIZE)
            return currentQualityIndex;

        fpsEvaluations.RemoveAt(0);
        float average = fpsEvaluations.Average();

        int newCurrentQualityIndex = currentQualityIndex;
        if (average <= FPSEvaluation.WORSE)
            newCurrentQualityIndex = Mathf.Max(0, currentQualityIndex - 1);

        if (average >= FPSEvaluation.GREAT)
            newCurrentQualityIndex = Mathf.Min(qualitySettings.Length - 1, currentQualityIndex + 1);

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