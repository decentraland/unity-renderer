using DCL.SettingsController;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Color Grading", fileName = "ColorGradingControlController")]
    public class ColorGradingControlController : ToggleSettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentQualitySetting.colorGrading;
        }

        public override void UpdateSetting(object newValue)
        {
            currentQualitySetting.colorGrading = (bool)newValue;

            Tonemapping toneMapping;
            if (QualitySettingsReferences.i.postProcessVolume.profile.TryGet<Tonemapping>(out toneMapping))
            {
                toneMapping.active = currentQualitySetting.colorGrading;
            }
        }
    }
}