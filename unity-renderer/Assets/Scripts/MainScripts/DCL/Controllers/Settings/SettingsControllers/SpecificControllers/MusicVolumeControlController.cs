using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using DCL.Helpers;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Music Volume", fileName = "MusicVolumeControlController")]
    public class MusicVolumeControlController : SliderSettingsControlController
    {
        [SerializeField]
        AudioMixer audioMixer;

        public override object GetStoredValue() { return currentAudioSettings.musicVolume * 100; }

        public override void UpdateSetting(object newValue) {
            currentAudioSettings.musicVolume = (float)newValue * 0.01f;
            audioMixer.SetFloat("MusicBusVolume", Utils.ToAudioMixerGroupVolume(currentAudioSettings.musicVolume));
        }
    }
}
