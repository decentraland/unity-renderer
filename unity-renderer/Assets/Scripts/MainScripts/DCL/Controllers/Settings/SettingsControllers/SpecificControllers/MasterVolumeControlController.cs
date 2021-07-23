using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using DCL.Helpers;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Master Volume", fileName = "MasterVolumeControlController")]
    public class MasterVolumeControlController : SliderSettingsControlController
    {
        [SerializeField]
        AudioMixer audioMixer;

        public override object GetStoredValue() { return currentAudioSettings.masterVolume * 100; }

        public override void UpdateSetting(object newValue) {
            currentAudioSettings.masterVolume = (float)newValue * 0.01f;
            audioMixer.SetFloat("AllBusVolume", Utils.ToAudioMixerGroupVolume(currentAudioSettings.masterVolume));
        }
    }
}