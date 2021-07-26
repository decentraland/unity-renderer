using DCL.Interface;
using UnityEngine;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Voice Chat Volume", fileName = "VoiceChatVolumeControlController")]
    public class VoiceChatVolumeControlController : SliderSettingsControlController
    {
        public override object GetStoredValue() { return currentAudioSettings.voiceChatVolume * 100; }

        public override void UpdateSetting(object newValue)
        {
            currentAudioSettings.voiceChatVolume = (float)newValue * 0.01f;
            WebInterface.ApplySettings(Settings.i.GetCalculatedVoiceChatVolume(), (int)currentGeneralSettings.voiceChatAllow);
        }
    }
}