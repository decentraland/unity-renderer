using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.BaseControllers
{
    /// <summary>
    /// This controller is in charge of manage all the logic related to a SETTING CONTROL.
    /// </summary>
    public class SettingsControlController : ScriptableObject
    {
        protected GeneralSettings currentGeneralSettings;
        protected QualitySettings currentQualitySetting;
        protected AudioSettings currentAudioSettings;

        public virtual void Initialize()
        {
            currentGeneralSettings = Settings.i.generalSettings.Data.Clone() as GeneralSettings;
            currentQualitySetting = Settings.i.qualitySettings.Data.Clone() as QualitySettings;
            currentAudioSettings = Settings.i.audioSettings.Data.Clone() as AudioSettings;

            Settings.i.generalSettings.OnChanged += OnGeneralSettingsChanged;
            Settings.i.qualitySettings.OnChanged += OnQualitySettingsChanged;
            Settings.i.audioSettings.OnChanged += OnAudioSettingsChanged;
            Settings.i.OnResetAllSettings += OnResetSettingsControl;
        }

        public virtual void OnDestroy()
        {
            Settings.i.generalSettings.OnChanged -= OnGeneralSettingsChanged;
            Settings.i.qualitySettings.OnChanged -= OnQualitySettingsChanged;
            Settings.i.audioSettings.OnChanged -= OnAudioSettingsChanged;
            Settings.i.OnResetAllSettings -= OnResetSettingsControl;
        }

        /// <summary>
        /// It should return the stored value of the control.
        /// </summary>
        /// <returns>It can be a bool (for toggle controls), a float (for slider controls) or an int (for spin-box controls).</returns>
        public virtual object GetStoredValue() { return null; }

        /// <summary>
        /// All the needed logic to applying the setting and storing the current value.
        /// </summary>
        /// <param name="newValue">Value of the new state. It can be a bool (for toggle controls), a float (for slider controls) or an int (for spin-box controls).</param>
        public virtual void UpdateSetting(object newValue) { }

        /// <summary>
        /// Applies the current control state into the Settings class.
        /// </summary>
        public virtual void ApplySettings()
        {
            Settings.i.generalSettings.Apply(currentGeneralSettings);
            Settings.i.qualitySettings.Apply(currentQualitySetting);
            Settings.i.audioSettings.Apply(currentAudioSettings);
        }

        private void OnGeneralSettingsChanged(GeneralSettings newGeneralSettings) { currentGeneralSettings = (GeneralSettings)newGeneralSettings.Clone(); }

        private void OnQualitySettingsChanged(QualitySettings newQualitySettings) { currentQualitySetting = (QualitySettings)newQualitySettings.Clone(); }

        private void OnAudioSettingsChanged(AudioSettings newAudioSettings) { currentAudioSettings = (AudioSettings)newAudioSettings.Clone(); }

        protected virtual void OnResetSettingsControl()
        {
            currentGeneralSettings = Settings.i.generalSettings.Data.Clone() as GeneralSettings;
            currentQualitySetting = Settings.i.qualitySettings.Data.Clone() as QualitySettings;
            currentAudioSettings = Settings.i.audioSettings.Data.Clone() as AudioSettings;
        }
    }
}