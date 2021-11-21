using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Scene SFX Volume", fileName = "SceneSFXVolumeControlController")]
    public class SceneSFXVolumeControlController : SliderSettingsControlController
    {
        public override object GetStoredValue() { return currentAudioSettings.sceneSFXVolume * 100; }

        public override void UpdateSetting(object newValue) {
            currentAudioSettings.sceneSFXVolume = (float)newValue * 0.01f;
            Settings.i.ApplyAudioSettings();
        }
    }
}
