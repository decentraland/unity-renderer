using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using DCL.Helpers;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Avatar SFX Volume", fileName = "AvatarSFXVolumeControlController")]
    public class AvatarSFXVolumeControlController : SliderSettingsControlController
    {
        [SerializeField]
        AudioMixer audioMixer;

        public override object GetStoredValue() { return currentAudioSettings.avatarSFXVolume * 100; }

        public override void UpdateSetting(object newValue) {
            currentAudioSettings.avatarSFXVolume = (float)newValue * 0.01f;
            audioMixer.SetFloat("AvatarSFXBusVolume", Utils.ToAudioMixerGroupVolume(currentAudioSettings.avatarSFXVolume));
        }
    }
}
