using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Output Audio Device", fileName = nameof(OutputAudioDeviceControlController))]
    public class OutputAudioDeviceControlController : SpinBoxSettingsControlController
    {
        public override object GetStoredValue() { return currentAudioSettings.outputDevice; }

        public override void UpdateSetting(object newValue)
        {
            currentAudioSettings.avatarSFXVolume = (float)newValue * 0.01f;
            Settings.i.ApplyAvatarSFXVolume();
        }
    }
}