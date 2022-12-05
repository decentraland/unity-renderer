using DCL.SettingsCommon;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using DCL.SettingsPanelHUD.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using QualitySettings = DCL.SettingsCommon.QualitySettings;

namespace DCL.SettingsPanelHUD.Controls
{
    /// <summary>
    /// MonoBehaviour that represents the base of a CONTROL view.
    /// </summary>
    public class SettingsControlView : MonoBehaviour, ISettingsControlView
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Color titleDeactivationColor;
        [SerializeField] private GameObject betaIndicator;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private List<TextMeshProUGUI> valueLabels;
        [SerializeField] private Color valueLabelDeactivationColor;
        [SerializeField] private List<Image> handleImages;
        [SerializeField] private Color handlerDeactivationColor;
        [SerializeField] private List<CanvasGroup> controlBackgroundCanvasGroups;
        [SerializeField] private float controlBackgroundDeactivationAlpha = 0.5f;

        private SettingsControlController settingsControlController;

        private SettingsControlModel controlConfig;
        private Color originalTitleColor;
        private Color originalLabelColor;
        private Color originalHandlerColor;
        private float originalControlBackgroundAlpha;

        public virtual void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController)
        {
            this.controlConfig = controlConfig;
            this.settingsControlController = settingsControlController;
            this.settingsControlController.Initialize();

            betaIndicator.SetActive(controlConfig.isBeta);

            title.text = controlConfig.title;
            originalTitleColor = title.color;
            originalLabelColor = valueLabels.Count > 0 ? valueLabels[0].color : Color.white;
            originalHandlerColor = handleImages.Count > 0 ? handleImages[0].color : Color.white;
            originalControlBackgroundAlpha = controlBackgroundCanvasGroups.Count > 0 ? controlBackgroundCanvasGroups[0].alpha : 1f;

            foreach (BooleanVariable flag in controlConfig.flagsThatDisableMe)
            {
                flag.OnChange += OnAnyDisableFlagChange;
                OnAnyDisableFlagChange(flag.Get());
            }

            foreach (BooleanVariable flag in controlConfig.flagsThatDeactivateMe)
            {
                flag.OnChange += OnAnyDeactivationFlagChange;
                OnAnyDeactivationFlagChange(flag.Get());
            }

            RefreshControl();

            Settings.i.generalSettings.OnChanged += OnGeneralSettingsChanged;
            Settings.i.qualitySettings.OnChanged += OnQualitySettingsChanged;
            Settings.i.OnResetAllSettings += OnResetSettingsControl;
        }

        protected virtual void OnDestroy()
        {
            if (controlConfig != null)
            {
                foreach (BooleanVariable flag in controlConfig.flagsThatDisableMe)
                    flag.OnChange -= OnAnyDisableFlagChange;

                foreach (BooleanVariable flag in controlConfig.flagsThatDeactivateMe)
                    flag.OnChange -= OnAnyDeactivationFlagChange;
            }

            Settings.i.generalSettings.OnChanged -= OnGeneralSettingsChanged;
            Settings.i.qualitySettings.OnChanged -= OnQualitySettingsChanged;
            Settings.i.OnResetAllSettings -= OnResetSettingsControl;
        }

        public virtual void RefreshControl() { }

        /// <summary>
        /// It will be triggered when the CONTROL state changes and will execute the main flow of the CONTROL controller: OnControlChanged(), ApplySettings() and PostApplySettings().
        /// </summary>
        /// <param name="newValue">Value of the new state. It can be a bool (for toggle controls), a float (for slider controls) or an int (for spin-box controls).</param>
        protected void ApplySetting(object newValue)
        {
            settingsControlController.UpdateSetting(newValue);
            settingsControlController.ApplySettings();
        }

        private void OnAnyDisableFlagChange(bool disable, bool _ = false) =>
            ApplyWhenAllFlagsAreFalse(disable, controlConfig.flagsThatDisableMe, SetEnabled);

        private void OnAnyDeactivationFlagChange(bool deactivate, bool _ = false) =>
            ApplyWhenAllFlagsAreFalse(deactivate, controlConfig.flagsThatDeactivateMe, SetControlActive);

        private static void ApplyWhenAllFlagsAreFalse(bool flagOn, List<BooleanVariable> flags, Action<bool> actionToApply)
        {
            bool canApplyChange = flagOn
                                  || !flags.Any(flag => flag.Get()); // Check if all the flags are false before enable the control

            if (canApplyChange)
                actionToApply(!flagOn);
        }

        private void OnGeneralSettingsChanged(GeneralSettings _) =>
            RefreshControl();

        private void OnQualitySettingsChanged(QualitySettings _) =>
            RefreshControl();

        private void OnResetSettingsControl() =>
            RefreshControl();

        private void SetEnabled(bool isEnabled)
        {
            title.color = isEnabled ? originalTitleColor : titleDeactivationColor;

            foreach (TextMeshProUGUI text in valueLabels)
                text.color = isEnabled ? originalLabelColor : valueLabelDeactivationColor;

            foreach (Image image in handleImages)
                image.color = isEnabled ? originalHandlerColor : handlerDeactivationColor;

            foreach (CanvasGroup group in controlBackgroundCanvasGroups)
                group.alpha = isEnabled ? originalControlBackgroundAlpha : controlBackgroundDeactivationAlpha;

            canvasGroup.interactable = isEnabled;
            canvasGroup.blocksRaycasts = isEnabled;
        }

        private void SetControlActive(bool isActive)
        {
            gameObject.SetActive(isActive);
            CommonSettingsPanelEvents.RaiseRefreshAllWidgetsSize();
        }
    }
}
