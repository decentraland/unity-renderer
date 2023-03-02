using DCL.SettingsCommon;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using DCL.SettingsPanelHUD.Common;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UIComponents.Scripts.Components;
using UIComponents.Scripts.Components.Tooltip;
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
        [SerializeField] private ButtonComponentView infoButton;
        [SerializeField] private TooltipComponentView tooltip;

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

        public virtual void Initialize(SettingsControlModel model, SettingsControlController controller)
        {
            controlConfig = model;
            settingsControlController = controller;
            settingsControlController.Initialize();

            betaIndicator.SetActive(model.isBeta);

            string tooltipMessage = !string.IsNullOrEmpty(model.infoTooltipMessage) ? model.infoTooltipMessage : "This setting is being controlled \n by the creator";
            tooltip.SetModel(new TooltipComponentModel(tooltipMessage));

            infoButton.onClick.AddListener(OnInfoButtonClicked);
            infoButton.gameObject.SetActive(model.infoButtonEnabled);

            title.text = model.title;
            originalTitleColor = title.color;
            originalLabelColor = valueLabels.Count > 0 ? valueLabels[0].color : Color.white;
            originalHandlerColor = handleImages.Count > 0 ? handleImages[0].color : Color.white;
            originalControlBackgroundAlpha = controlBackgroundCanvasGroups.Count > 0 ? controlBackgroundCanvasGroups[0].alpha : 1f;

            SwitchInteractibility(isInteractable: model.flagsThatDisableMe.All(flag => flag.Get() == false));
            SwitchVisibility(isVisible: model.flagsThatDeactivateMe.All(flag => flag.Get() == false));

            if(!model.infoButtonEnabled)
                SetOverriden(@override: model.flagsThatOverrideMe.Any(flag => flag.Get()));

            RefreshControl();

            foreach (BooleanVariable flag in model.flagsThatDisableMe)
                flag.OnChange += OnAnyDisableFlagChange;

            foreach (BooleanVariable flag in model.flagsThatDeactivateMe)
                flag.OnChange += OnAnyDeactivationFlagChange;

            foreach (BooleanVariable flag in model.flagsThatOverrideMe)
                flag.OnChange += OnAnyOverrideFlagChange;

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

            infoButton.onClick.RemoveListener(OnInfoButtonClicked);

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

        private void OnInfoButtonClicked()
        {
            if (!tooltip.gameObject.activeSelf)
                tooltip.Show();
        }

        private void OnGeneralSettingsChanged(GeneralSettings _) =>
            RefreshControl();

        private void OnQualitySettingsChanged(QualitySettings _) =>
            RefreshControl();

        private void OnAnyDeactivationFlagChange(bool deactivateFlag, bool _ = false) =>
            SwitchVisibility(isVisible: AllFlagsAreDisabled(deactivateFlag, controlConfig.flagsThatDeactivateMe));

        private void OnAnyDisableFlagChange(bool disableFlag, bool _ = false) =>
            SwitchInteractibility(isInteractable: AllFlagsAreDisabled(disableFlag, controlConfig.flagsThatDisableMe));

        private void OnAnyOverrideFlagChange(bool overrideFlag, bool _ = false) =>
            SetOverriden(@override: AnyFlagIsEnabled(overrideFlag, controlConfig.flagsThatOverrideMe));

        private static bool AllFlagsAreDisabled(bool flagEnabled, List<BooleanVariable> flags) =>
            !flagEnabled && flags.All(flag => flag.Get() == false);

        private static bool AnyFlagIsEnabled(bool flagEnabled, List<BooleanVariable> flags) =>
            flagEnabled || flags.Any(flag => flag.Get());

        private void SwitchVisibility(bool isVisible)
        {
            gameObject.SetActive(isVisible);
            CommonSettingsPanelEvents.RaiseRefreshAllWidgetsSize();
        }

        private void SwitchInteractibility(bool isInteractable)
        {
            title.color = isInteractable ? originalTitleColor : titleDeactivationColor;
            SwitchUIControlInteractibility(isInteractable, canvasGroup);
        }

        private void SetOverriden(bool @override)
        {
            infoButton.gameObject.SetActive(@override);
            SwitchUIControlInteractibility(isInteractable: !@override, controlCanvasGroup);
        }

        private void SwitchUIControlInteractibility(bool isInteractable, CanvasGroup group)
        {
            foreach (TextMeshProUGUI text in valueLabels)
                text.color = isInteractable ? originalLabelColor : valueLabelDeactivationColor;

            foreach (Image image in handleImages)
                image.color = isInteractable ? originalHandlerColor : handlerDeactivationColor;

            foreach (CanvasGroup bkgCanvasGroup in controlBackgroundCanvasGroups)
                bkgCanvasGroup.alpha = isInteractable ? originalControlBackgroundAlpha : controlBackgroundDeactivationAlpha;

            group.interactable = isInteractable;
            group.blocksRaycasts = isInteractable;
        }
    }
}
