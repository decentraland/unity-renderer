using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Input Audio Device", fileName = nameof(InputAudioDeviceControlController))]
    public class InputAudioDeviceControlController : SpinBoxSettingsControlController
    {
        public override void Initialize()
        {
            base.Initialize();
            RaiseOnOverrideIndicatorLabel(new [] { "Micro 1", "Micro 2" });
            UpdateSetting(GetStoredValue());
        }

        public override object GetStoredValue() { return currentAudioSettings.inputDevice; }

        public override void UpdateSetting(object newValue)
        {
            currentAudioSettings.inputDevice = (int)newValue;
            ApplySettings();
            Settings.i.ChangeAudioDevicesSettings();
        }
    }
}