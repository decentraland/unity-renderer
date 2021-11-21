using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Master Volume", fileName = "MasterVolumeControlController")]
    public class MasterVolumeControlController : SliderSettingsControlController
    {
        public override object GetStoredValue() { return currentAudioSettings.masterVolume * 100; }

        public override void UpdateSetting(object newValue) {
            currentAudioSettings.masterVolume = (float)newValue * 0.01f;
            Settings.i.ApplyMasterVolume();
            Settings.i.ApplyAudioSettings();
        }
    }
}