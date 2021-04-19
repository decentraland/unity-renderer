using DCL;

/// <summary>
/// The AutoQualityController will provide an autoquality index based on the metrics provided
/// </summary>
public interface IAutoQualityController
{

    /// <summary>
    /// Returns a quality index based on a performance metrics
    /// </summary>
    /// <param name="metrics">Performance metrics to evaluate</param>
    /// <returns></returns>
    int EvaluateQuality(PerformanceMetricsData metrics);

    /// <summary>
    /// Reset any ongoing and cached data of previous evaluations.
    /// </summary>
    void ResetEvaluation();
}