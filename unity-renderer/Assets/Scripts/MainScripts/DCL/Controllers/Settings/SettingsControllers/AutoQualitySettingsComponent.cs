﻿using System.Collections;
using DCL.Helpers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers
{
    /// <summary>
    /// Controller to change the quality settings automatically based on performance
    /// </summary>
    public class AutoQualitySettingsComponent : MonoBehaviour
    {
        private const float LOOP_TIME_SECONDS = 1f;
        private const string LAST_QUALITY_INDEX = "LAST_QUALITY_INDEX";
        private const int TARGET_FPS = 30;

        [SerializeField] internal BooleanVariable autoQualityEnabled;
        [SerializeField] internal PerformanceMetricsDataVariable performanceMetricsData;
        internal QualitySettingsData qualitySettings => Settings.i.autoQualitySettings;

        internal int currentQualityIndex;
        private Coroutine settingsCheckerCoroutine;
        internal IAutoQualityController controller;
        internal bool fpsCapped;

        void Start()
        {
            if (autoQualityEnabled == null || qualitySettings == null || qualitySettings.Length == 0)
                return;

            currentQualityIndex = PlayerPrefsUtils.GetInt(LAST_QUALITY_INDEX, (qualitySettings.Length - 1) / 2);

            controller = new AutoQualityUncappedFPSController(currentQualityIndex, qualitySettings);

            Settings.i.qualitySettings.OnChanged += OnQualitySettingsChanged;
            autoQualityEnabled.OnChange += SetAutoSettings;

            fpsCapped = !Settings.i.qualitySettings.Data.fpsCap;
            OnQualitySettingsChanged(Settings.i.qualitySettings.Data);
            SetAutoSettings(autoQualityEnabled.Get(), !autoQualityEnabled.Get());
        }

        private void OnDestroy()
        {
            if (autoQualityEnabled != null)
                autoQualityEnabled.OnChange -= SetAutoSettings;
            Settings.i.qualitySettings.OnChanged -= OnQualitySettingsChanged;
        }

        internal void OnQualitySettingsChanged(QualitySettings settings)
        {
            if (fpsCapped == settings.fpsCap)
                return;

            fpsCapped = settings.fpsCap;
            if (settings.fpsCap)
                controller = new AutoQualityCappedFPSController(TARGET_FPS, currentQualityIndex, qualitySettings);
            else
                controller = new AutoQualityUncappedFPSController(currentQualityIndex, qualitySettings);
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
                controller.ResetEvaluation();
            }
        }

        private IEnumerator AutoSettingsLoop()
        {
            while (true)
            {
                UpdateQuality(controller.EvaluateQuality(performanceMetricsData?.Get()));
                yield return new WaitForSeconds(LOOP_TIME_SECONDS);
            }
        }

        private void UpdateQuality(int newQualityIndex)
        {
            if (newQualityIndex == currentQualityIndex)
                return;

            if (newQualityIndex <= 0 || newQualityIndex >= qualitySettings.Length)
                return;

            PlayerPrefsUtils.SetInt(LAST_QUALITY_INDEX, currentQualityIndex);
            currentQualityIndex = newQualityIndex;
            Settings.i.ApplyAutoQualitySettings(currentQualityIndex);
        }
    }
}