using System.Collections;
using DCL;
using DCL.SettingsData;
using UnityEngine;
using QualitySettings = DCL.SettingsData.QualitySettings;

/// <summary>
/// Controller to change the quality settings automatically based on performance
/// </summary>
public class AutoQualitySettingsController : MonoBehaviour
{
    private const float LOOP_TIME_SECONDS = 1f;
    private const string LAST_QUALITY_INDEX = "LAST_QUALITY_INDEX";

    [SerializeField] internal BooleanVariable autoQualityEnabled;
    [SerializeField] internal PerformanceMetricsDataVariable performanceMetricsData;
    internal QualitySettingsData qualitySettings => Settings.i.autoqualitySettings;

    internal int currentQualityIndex;
    internal IAutoQualitySettingsEvaluator evaluator = new AutoQualitySettingsEvaluator();
    private Coroutine settingsCheckerCoroutine;

    void Awake()
    {
        if (autoQualityEnabled == null || qualitySettings == null || qualitySettings.Length == 0)
            return;

        currentQualityIndex = PlayerPrefs.GetInt(LAST_QUALITY_INDEX,(qualitySettings.Length - 1) / 2);
        autoQualityEnabled.OnChange += SetAutoSettings;
        SetAutoSettings(autoQualityEnabled.Get(), !autoQualityEnabled.Get());
    }

    private void OnDestroy()
    {
        if (autoQualityEnabled == null)
            return;
        autoQualityEnabled.OnChange -= SetAutoSettings;
    }

    private void SetAutoSettings(bool newValue, bool oldValue)
    {
        if (settingsCheckerCoroutine != null)
        {
            StopCoroutine(settingsCheckerCoroutine);
            settingsCheckerCoroutine = null;
        }

        if (newValue)
        {
           settingsCheckerCoroutine = StartCoroutine(AutoSettingsLoop());
        }
        else
        {
            evaluator.Reset();
        }
    }

    private IEnumerator AutoSettingsLoop()
    {
        SetQuality(currentQualityIndex);
        while (true)
        {
            EvaluateQuality();

            yield return new WaitForSeconds(LOOP_TIME_SECONDS);
        }
    }

    internal void EvaluateQuality()
    {
        int newQualityIndex = currentQualityIndex;
        switch (evaluator.Evaluate(performanceMetricsData?.Get()))
        {
            case -1:
                newQualityIndex = Mathf.Max(0, currentQualityIndex - 1);
                break;
            case 1:
                newQualityIndex = Mathf.Min(qualitySettings.Length - 1, currentQualityIndex + 1);
                break;
            default:
                return;
        }

        SetQuality(newQualityIndex);
    }

    private void SetQuality(int newQualityIndex)
    {
        if (newQualityIndex <= 0 || newQualityIndex >= qualitySettings.Length)
            return;

        PlayerPrefs.SetInt(LAST_QUALITY_INDEX, currentQualityIndex);
        currentQualityIndex = newQualityIndex;
        evaluator.Reset();
        Settings.i.ApplyAutoQualitySettings(currentQualityIndex);
    }
}
