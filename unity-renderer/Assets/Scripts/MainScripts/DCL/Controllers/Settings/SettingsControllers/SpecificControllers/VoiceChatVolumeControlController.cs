using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Voice Chat Volume", fileName = "VoiceChatVolumeControlController")]
    public class VoiceChatVolumeControlController : SliderSettingsControlController
    {
        public override object GetStoredValue() { return currentAudioSettings.voiceChatVolume * 100; }

        public override void UpdateSetting(object newValue)
        {
            currentAudioSettings.voiceChatVolume = (float)newValue * 0.01f;
            Settings.i.ApplyVoiceChatSettings();
            Settings.i.ApplyAudioSettings();
        }
    }
}