using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Chat SFX Toggle", fileName = "ChatSFXToggleControlController")]
    public class ChatSFXToggleControlController : SpinBoxSettingsControlController
    {
        public override object GetStoredValue() =>
            (int)currentAudioSettings.chatNotificationType;

        public override void UpdateSetting(object newValue)
        {
            var newNotificationValue = (AudioSettings.ChatNotificationType)newValue;
            currentAudioSettings.chatNotificationType = newNotificationValue;
        }
    }
}
