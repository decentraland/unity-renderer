using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Allow Voice Chat", fileName = "AllowVoiceChatControlController")]
    public class AllowVoiceChatControlController : SettingsControlController
    {
        public override object GetStoredValue()
        {
            return (int)currentGeneralSettings.voiceChatAllow;
        }

        public override void OnControlChanged(object newValue)
        {
            currentGeneralSettings.voiceChatAllow = (SettingsData.GeneralSettings.VoiceChatAllow)newValue;
        }
    }
}