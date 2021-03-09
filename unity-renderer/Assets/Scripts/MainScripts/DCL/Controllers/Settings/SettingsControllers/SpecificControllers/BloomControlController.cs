using DCL.SettingsController;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Bloom", fileName = "BloomControlController")]
    public class BloomControlController : ToggleSettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentQualitySetting.bloom;
        }

        public override void UpdateSetting(object newValue)
        {
            currentQualitySetting.bloom = (bool)newValue;

            if (QualitySettingsReferences.i.postProcessVolume)
            {
                if (QualitySettingsReferences.i.postProcessVolume.profile.TryGet<Bloom>(out Bloom bloom))
                {
                    bloom.active = currentQualitySetting.bloom;
                }
            }
        }
    }
}