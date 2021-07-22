using UnityEngine;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Chat SFX Toggle", fileName = "ChatSFXToggleControlController")]
    public class ChatSFXToggleControlController : ToggleSettingsControlController
    {
        public override object GetStoredValue() { return currentGeneralSettings.sfxVolume > 0 ? true : false; }

        public override void UpdateSetting(object newValue)
        {
            bool newBoolValue = (bool)newValue;
            currentGeneralSettings.sfxVolume = newBoolValue ? 1 : 0;
            AudioListener.volume = currentGeneralSettings.sfxVolume;
        }
    }
}