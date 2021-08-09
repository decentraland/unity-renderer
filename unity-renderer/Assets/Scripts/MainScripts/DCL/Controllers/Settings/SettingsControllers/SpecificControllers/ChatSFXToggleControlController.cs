using UnityEngine;

namespace DCL.SettingsControls
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