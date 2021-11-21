using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Avatar SFX Volume", fileName = "AvatarSFXVolumeControlController")]
    public class AvatarSFXVolumeControlController : SliderSettingsControlController
    {
        public override object GetStoredValue() { return currentAudioSettings.avatarSFXVolume * 100; }

        public override void UpdateSetting(object newValue) {
            currentAudioSettings.avatarSFXVolume = (float)newValue * 0.01f;
            Settings.i.ApplyAvatarSFXVolume();
            Settings.i.ApplyAudioSettings();
        }
    }
}
