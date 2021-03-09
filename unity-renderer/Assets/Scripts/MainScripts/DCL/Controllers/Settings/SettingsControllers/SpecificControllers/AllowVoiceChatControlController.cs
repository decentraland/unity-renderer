using DCL.Interface;
using UnityEngine;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Allow Voice Chat", fileName = "AllowVoiceChatControlController")]
    public class AllowVoiceChatControlController : SpinBoxSettingsControlController
    {
        public override object GetStoredValue()
        {
            return (int)currentGeneralSettings.voiceChatAllow;
        }

        public override void UpdateSetting(object newValue)
        {
            int newIntValue = (int)newValue;
            currentGeneralSettings.voiceChatAllow = (SettingsData.GeneralSettings.VoiceChatAllow)newIntValue;
            WebInterface.ApplySettings(currentGeneralSettings.voiceChatVolume, newIntValue);
        }
    }
}