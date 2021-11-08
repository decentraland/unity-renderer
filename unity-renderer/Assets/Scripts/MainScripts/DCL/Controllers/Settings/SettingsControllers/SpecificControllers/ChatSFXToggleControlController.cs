using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Chat SFX Toggle", fileName = "ChatSFXToggleControlController")]
    public class ChatSFXToggleControlController : ToggleSettingsControlController
    {
        public override object GetStoredValue() { return currentAudioSettings.chatSFXEnabled; }

        public override void UpdateSetting(object newValue)
        {
            bool newBoolValue = (bool)newValue;
            currentAudioSettings.chatSFXEnabled = newBoolValue;
        }
    }
}