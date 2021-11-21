using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Music Volume", fileName = "MusicVolumeControlController")]
    public class MusicVolumeControlController : SliderSettingsControlController
    {
        public override object GetStoredValue() { return currentAudioSettings.musicVolume * 100; }

        public override void UpdateSetting(object newValue) {
            currentAudioSettings.musicVolume = (float)newValue * 0.01f;
            Settings.i.ApplyMusicVolume();
            Settings.i.ApplyAudioSettings();
        }
    }
}
