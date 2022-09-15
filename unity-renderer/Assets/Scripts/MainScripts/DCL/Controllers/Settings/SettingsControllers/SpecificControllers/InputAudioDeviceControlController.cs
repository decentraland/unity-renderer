using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Input Audio Device", fileName = nameof(InputAudioDeviceControlController))]
    public class InputAudioDeviceControlController : SpinBoxSettingsControlController
    {
        public override object GetStoredValue() { return currentAudioSettings.inputDevice; }

        public override void UpdateSetting(object newValue)
        {
            int newIntValue = (int)newValue;
            currentAudioSettings.inputDevice = newIntValue;
            ApplySettings();
            Settings.i.ChangeAudioDevicesSettings();
        }
    }
}