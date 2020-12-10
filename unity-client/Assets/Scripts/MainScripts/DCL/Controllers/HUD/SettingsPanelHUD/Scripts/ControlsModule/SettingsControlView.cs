using DCL.SettingsPanelHUD.Common;
using TMPro;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    /// <summary>
    /// Base interface to implement a view for a CONTROL.
    /// </summary>
    public interface ISettingsControlView
    {
        /// <summary>
        /// All the needed base logic to initializes the CONTROL view.
        /// </summary>
        /// <param name="controlConfig">Model that will contain the configuration of the CONTROL.</param>
        /// <param name="settingsControlController">Controller associated to the CONTROL view.</param>
        void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController);

        /// <summary>
        /// This logic should update the CONTROL view with the stored value.
        /// </summary>
        void RefreshControl();
    }

    /// <summary>
    /// MonoBehaviour that represents the base of a CONTROL view.
    /// </summary>
    public class SettingsControlView : MonoBehaviour, ISettingsControlView
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private GameObject betaIndicator;
        [SerializeField] private CanvasGroup canvasGroup;

        protected SettingsControlController settingsControlController;
        protected bool skipPostApplySettings = false;

        private SettingsControlModel controlConfig;

        private void OnEnable()
        {
            if (settingsControlController == null)
                return;

            skipPostApplySettings = true;
            RefreshControl();
        }

        public virtual void Initialize(SettingsControlModel controlConfig, SettingsControlController settingsControlController)
        {
            this.controlConfig = controlConfig;
            this.settingsControlController = settingsControlController;
            this.settingsControlController.Initialize(this);
            title.text = controlConfig.title;
            betaIndicator.SetActive(controlConfig.isBeta);

            foreach (BooleanVariable flag in controlConfig.flagsThatDisableMe)
            {
                flag.OnChange += OnAnyDisableFlagChange;
                OnAnyDisableFlagChange(flag.Get(), false);
            }

            foreach (BooleanVariable flag in controlConfig.flagsThatDeactivateMe)
            {
                flag.OnChange += OnAnyDeactivationFlagChange;
                OnAnyDeactivationFlagChange(flag.Get(), false);
            }

            CommonSettingsEvents.OnRefreshAllSettings += OnRefreshAllSettings;

            skipPostApplySettings = true;
            RefreshControl();
            settingsControlController.OnControlChanged(settingsControlController.GetStoredValue());
        }

        private void OnDestroy()
        {
            if (controlConfig != null)
            {
                foreach (BooleanVariable flag in controlConfig.flagsThatDisableMe)
                {
                    flag.OnChange -= OnAnyDisableFlagChange;
                }

                foreach (BooleanVariable flag in controlConfig.flagsThatDeactivateMe)
                {
                    flag.OnChange -= OnAnyDeactivationFlagChange;
                }
            }

            CommonSettingsEvents.OnRefreshAllSettings -= OnRefreshAllSettings;
        }

        public virtual void RefreshControl()
        {
        }

        /// <summary>
        /// It will be triggered when the CONTROL state changes and will execute the main flow of the CONTROL controller: OnControlChanged(), ApplySettings() and PostApplySettings().
        /// </summary>
        /// <param name="newValue">Value of the new state. It can be a bool (for toggle controls), a float (for slider controls) or an int (for spin-box controls).</param>
        protected void ApplySetting(object newValue)
        {
            settingsControlController.OnControlChanged(newValue);
            settingsControlController.ApplySettings();

            if (!skipPostApplySettings)
                settingsControlController.PostApplySettings();
            skipPostApplySettings = false;
        }

        private void OnAnyDisableFlagChange(bool current, bool previous)
        {
            bool canApplychange = true;
            if (!current)
            {
                // Check if all the disable flags are false before enable the control
                foreach (var flag in controlConfig.flagsThatDisableMe)
                {
                    if (flag.Get() == true)
                    {
                        canApplychange = false;
                        break;
                    }
                }
            }

            if (canApplychange)
                SetEnabled(!current);
        }

        private void OnAnyDeactivationFlagChange(bool current, bool previous)
        {
            bool canApplychange = true;
            if (!current)
            {
                // Check if all the deactivation flags are false before enable the control
                foreach (var flag in controlConfig.flagsThatDeactivateMe)
                {
                    if (flag.Get() == true)
                    {
                        canApplychange = false;
                        break;
                    }
                }
            }

            if (canApplychange)
                SetControlActive(!current);
        }

        private void SetEnabled(bool enabled)
        {
            canvasGroup.alpha = enabled ? 1f : 0.5f;
            canvasGroup.interactable = enabled;
        }

        private void SetControlActive(bool actived)
        {
            gameObject.SetActive(actived);
            CommonSettingsEvents.RaiseRefreshAllWidgetsSize();
        }

        private void OnRefreshAllSettings(SettingsControlController sender)
        {
            if (sender != settingsControlController)
            {
                skipPostApplySettings = true;
                RefreshControl();
            }
        }
    }
}