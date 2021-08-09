using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/UI SFX Volume", fileName = "UISFXVolumeControlController")]
    public class UISFXVolumeControlController : SliderSettingsControlController
    {
        public override object GetStoredValue() { return currentAudioSettings.uiSFXVolume * 100; }

        public override void UpdateSetting(object newValue) {
            currentAudioSettings.uiSFXVolume = (float)newValue * 0.01f;
            Settings.i.ApplyUISFXVolume();
        }
    }
}
