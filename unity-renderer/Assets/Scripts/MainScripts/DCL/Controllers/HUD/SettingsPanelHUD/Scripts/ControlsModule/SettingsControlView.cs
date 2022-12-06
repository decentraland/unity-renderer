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
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private CanvasGroup controlCanvasGroup;

        [Space]
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Color titleDeactivationColor;

        [Space]
        [SerializeField] private GameObject betaIndicator;
        [SerializeField] private GameObject infoButton;

        [Space]
        [SerializeField] private List<TextMeshProUGUI> valueLabels;
        [SerializeField] private Color valueLabelDeactivationColor;

        [Space]
        [SerializeField] private List<Image> handleImages;
        [SerializeField] private Color handlerDeactivationColor;

        [Space]
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

            foreach (BooleanVariable flag in controlConfig.flagsThatOverrideMe)
            {
                flag.OnChange += OnAnyOverrideFlagChange;
                OnAnyOverrideFlagChange(flag.Get());
            }

            RefreshControl();

            Settings.i.generalSettings.OnChanged += OnGeneralSettingsChanged;
            Settings.i.qualitySettings.OnChanged += OnQualitySettingsChanged;
            Settings.i.OnResetAllSettings += RefreshControl;
        }

        protected virtual void OnDestroy()
        {
            if (controlConfig != null)
            {
                foreach (BooleanVariable flag in controlConfig.flagsThatDisableMe)
                    flag.OnChange -= OnAnyDisableFlagChange;

                foreach (BooleanVariable flag in controlConfig.flagsThatDeactivateMe)
                    flag.OnChange -= OnAnyDeactivationFlagChange;

                foreach (BooleanVariable flag in controlConfig.flagsThatOverrideMe)
                    flag.OnChange -= OnAnyOverrideFlagChange;
            }

            Settings.i.generalSettings.OnChanged -= OnGeneralSettingsChanged;
            Settings.i.qualitySettings.OnChanged -= OnQualitySettingsChanged;
            Settings.i.OnResetAllSettings -= RefreshControl;
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

        private void OnAnyDisableFlagChange(bool disableFlag, bool _ = false) =>
            SetEnabled(enable: TrueWhenAllFlagsAreOff(disableFlag, controlConfig.flagsThatDisableMe));

        private void OnAnyDeactivationFlagChange(bool deactivateFlag, bool _ = false) =>
            SetActive(activate: TrueWhenAllFlagsAreOff(deactivateFlag, controlConfig.flagsThatDeactivateMe));

        private void OnAnyOverrideFlagChange(bool overrideFlag, bool _ = false) =>
            SetOverriden(@override: TrueWhenAllFlagsAreOff(overrideFlag, controlConfig.flagsThatOverrideMe));

        private static bool TrueWhenAllFlagsAreOff(bool flagEnabled, List<BooleanVariable> flags)
        {
            if (flagEnabled)
                return false; // disable if flag was changed to be On

            if (flags.Any(flag => flag.Get())) // or any other flag is still On
                return false;

            return true; // true otherwise (when all flags are off)
        }

        private void OnGeneralSettingsChanged(GeneralSettings _) =>
            RefreshControl();

        private void OnQualitySettingsChanged(QualitySettings _) =>
            RefreshControl();

        private void SetEnabled(bool enable)
        {
            title.color = enable ? originalTitleColor : titleDeactivationColor;

            foreach (TextMeshProUGUI text in valueLabels)
                text.color = enable ? originalLabelColor : valueLabelDeactivationColor;

            foreach (Image image in handleImages)
                image.color = enable ? originalHandlerColor : handlerDeactivationColor;

            foreach (CanvasGroup group in controlBackgroundCanvasGroups)
                group.alpha = enable ? originalControlBackgroundAlpha : controlBackgroundDeactivationAlpha;

            canvasGroup.interactable = enable;
            canvasGroup.blocksRaycasts = enable;
        }

        private void SetOverriden(bool @override)
        {
            infoButton.SetActive(@override);
            //
            // foreach (TextMeshProUGUI text in valueLabels)
            //     text.color = @override ? valueLabelDeactivationColor : originalLabelColor;
            //
            // foreach (Image image in handleImages)
            //     image.color = @override ? handlerDeactivationColor : originalHandlerColor;
            //
            // foreach (CanvasGroup group in controlBackgroundCanvasGroups)
            //     group.alpha = @override ? controlBackgroundDeactivationAlpha : originalControlBackgroundAlpha;

            // controlCanvasGroup.interactable = !@override;
            // controlCanvasGroup.blocksRaycasts = @override;
        }

        private void SetActive(bool activate)
        {
            gameObject.SetActive(activate);
            CommonSettingsPanelEvents.RaiseRefreshAllWidgetsSize();
        }
    }
}
