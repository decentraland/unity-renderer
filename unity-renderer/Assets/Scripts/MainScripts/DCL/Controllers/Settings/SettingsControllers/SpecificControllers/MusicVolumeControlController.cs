using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Music Volume", fileName = "MusicVolumeControlController")]
    public class MusicVolumeControlController : SliderSettingsControlController
    {
        public override object GetStoredValue() { return 0; }

        public override void UpdateSetting(object newValue) {
            //currentGeneralSettings.voiceChatVolume = (float)newValue * 0.01f;
            //WebInterface.ApplySettings(currentGeneralSettings.voiceChatVolume, (int)currentGeneralSettings.voiceChatAllow);
        }
    }
}
