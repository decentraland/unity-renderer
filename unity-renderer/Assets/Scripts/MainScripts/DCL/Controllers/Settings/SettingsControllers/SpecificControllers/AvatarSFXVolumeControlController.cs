using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Avatar SFX Volume", fileName = "AvatarSFXVolumeControlController")]
    public class AvatarSFXVolumeControlController : SliderSettingsControlController
    {
        public override object GetStoredValue() { return currentAudioSettings.avatarSFXVolume * 100; }

        public override void UpdateSetting(object newValue) {
            currentAudioSettings.avatarSFXVolume = (float)newValue * 0.01f;
            // TODO: Apply volume change
        }
    }
}
