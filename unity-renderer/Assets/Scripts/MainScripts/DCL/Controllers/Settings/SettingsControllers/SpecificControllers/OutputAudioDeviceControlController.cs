using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Output Audio Device", fileName = nameof(OutputAudioDeviceControlController))]
    public class OutputAudioDeviceControlController : SpinBoxSettingsControlController
    {
        public override void Initialize()
        {
            base.Initialize();
            // Environment.i.serviceLocator.
            RaiseOnOverrideIndicatorLabel(new [] { "Speaker 1", "Speaker 2", "Speaker 3" });
            UpdateSetting(GetStoredValue());
        }

        public override object GetStoredValue() { return currentAudioSettings.outputDevice; }

        public override void UpdateSetting(object newValue)
        {
            currentAudioSettings.outputDevice = (int)newValue;

            ApplySettings();
            Settings.i.ChangeAudioDevicesSettings();
        }
    }
}