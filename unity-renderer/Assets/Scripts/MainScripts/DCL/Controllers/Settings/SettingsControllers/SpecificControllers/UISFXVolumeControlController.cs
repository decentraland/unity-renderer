using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using DCL.Helpers;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/UI SFX Volume", fileName = "UISFXVolumeControlController")]
    public class UISFXVolumeControlController : SliderSettingsControlController
    {
        [SerializeField]
        AudioMixer audioMixer;

        public override object GetStoredValue() { return currentAudioSettings.uiSFXVolume * 100; }

        public override void UpdateSetting(object newValue) {
            currentAudioSettings.uiSFXVolume = (float)newValue * 0.01f;
            audioMixer.SetFloat("UIBusVolume", Utils.ToAudioMixerGroupVolume(currentAudioSettings.uiSFXVolume));
        }
    }
}
