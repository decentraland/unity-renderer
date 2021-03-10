using UnityEngine;

namespace DCL.SettingsControls
{
    /// <summary>
    /// This controller is in charge of manage all the logic related to a SETTING CONTROL.
    /// </summary>
    public class SettingsControlController : ScriptableObject
    {
        protected SettingsData.GeneralSettings currentGeneralSettings;
        protected SettingsData.QualitySettings currentQualitySetting;

        public virtual void Initialize()
        {
            currentGeneralSettings = Settings.i.generalSettings;
            currentQualitySetting = Settings.i.qualitySettings;

            Settings.i.OnGeneralSettingsChanged += OnGeneralSettingsChanged;
            Settings.i.OnQualitySettingsChanged += OnQualitySettingsChanged;
            Settings.i.OnResetAllSettings += OnResetSettingsControl;
        }

        public virtual void OnDestroy()
        {
            Settings.i.OnGeneralSettingsChanged -= OnGeneralSettingsChanged;
            Settings.i.OnQualitySettingsChanged -= OnQualitySettingsChanged;
            Settings.i.OnResetAllSettings -= OnResetSettingsControl;
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
        /// All the needed logic to applying the setting and storing the current value.
        /// </summary>
        /// <param name="newValue">Value of the new state. It can be a bool (for toggle controls), a float (for slider controls) or an int (for spin-box controls).</param>
        public virtual void UpdateSetting(object newValue)
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

        private void OnGeneralSettingsChanged(SettingsData.GeneralSettings newGeneralSettings)
        {
            currentGeneralSettings = newGeneralSettings;
        }

        private void OnQualitySettingsChanged(SettingsData.QualitySettings newQualitySettings)
        {
            currentQualitySetting = newQualitySettings;
        }

        private void OnResetSettingsControl()
        {
            currentGeneralSettings = Settings.i.generalSettings;
            currentQualitySetting = Settings.i.qualitySettings;
        }
    }
}