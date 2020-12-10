using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    /// <summary>
    /// This controller is in charge of manage all the base logic related to a CONTROL.
    /// </summary>
    public class SettingsControlController : ScriptableObject
    {
        protected SettingsData.GeneralSettings currentGeneralSettings;
        protected SettingsData.QualitySettings currentQualitySetting;
        protected ISettingsControlView view;

        public virtual void Initialize(ISettingsControlView settingsControlView)
        {
            view = settingsControlView;

            currentGeneralSettings = Settings.i.generalSettings;
            currentQualitySetting = Settings.i.qualitySettings;

            Settings.i.OnGeneralSettingsChanged += OnGeneralSettingsChanged;
            Settings.i.OnQualitySettingsChanged += OnQualitySettingsChanged;
        }

        public virtual void OnDestroy()
        {
            Settings.i.OnGeneralSettingsChanged -= OnGeneralSettingsChanged;
            Settings.i.OnQualitySettingsChanged -= OnQualitySettingsChanged;
        }

        /// <summary>
        /// It should return the stored value of the control.
        /// </summary>
        /// <returns>It can be a bool (for toggle controls), a float (for slider controls) or an int (for spin-box controls).</returns>
        public virtual object GetStoredValue()
        {
            return null;
        }

        /// <summary>
        /// It should contain the specific logic that will be triggered when the control state changes.
        /// </summary>
        /// <param name="newValue">Value of the new state. It can be a bool (for toggle controls), a float (for slider controls) or an int (for spin-box controls).</param>
        public virtual void OnControlChanged(object newValue)
        {
        }

        /// <summary>
        /// Applies the current control state into the Settings class.
        /// </summary>
        public virtual void ApplySettings()
        {
            Settings.i.ApplyGeneralSettings(currentGeneralSettings);
            Settings.i.ApplyQualitySettings(currentQualitySetting);
        }

        /// <summary>
        /// The logic put here will be triggered AFTER ApplySettings().
        /// </summary>
        public virtual void PostApplySettings()
        {
        }

        private void OnGeneralSettingsChanged(SettingsData.GeneralSettings newGeneralSettings)
        {
            currentGeneralSettings = newGeneralSettings;
        }

        private void OnQualitySettingsChanged(SettingsData.QualitySettings newQualitySettings)
        {
            currentQualitySetting = newQualitySettings;
        }
    }
}