using DCL.Interface;
using UnityEngine;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Voice Chat Volume", fileName = "VoiceChatVolumeControlController")]
    public class VoiceChatVolumeControlController : SliderSettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentGeneralSettings.voiceChatVolume * 100;
        }

        public override void UpdateSetting(object newValue)
        {
            currentGeneralSettings.voiceChatVolume = (float)newValue * 0.01f;
            WebInterface.ApplySettings(currentGeneralSettings.voiceChatVolume, (int)currentGeneralSettings.voiceChatAllow);
        }
    }
}