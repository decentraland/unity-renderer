using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Allow Voice Chat", fileName = "AllowVoiceChatControlController")]
    public class AllowVoiceChatControlController : SpinBoxSettingsControlController
    {
        public override object GetStoredValue() { return (int)currentGeneralSettings.voiceChatAllow; }

        public override void UpdateSetting(object newValue)
        {
            int newIntValue = (int)newValue;
            currentGeneralSettings.voiceChatAllow = (GeneralSettings.VoiceChatAllow)newIntValue;
            ApplySettings();
            Settings.i.ApplyVoiceChatSettings();
        }
    }
}