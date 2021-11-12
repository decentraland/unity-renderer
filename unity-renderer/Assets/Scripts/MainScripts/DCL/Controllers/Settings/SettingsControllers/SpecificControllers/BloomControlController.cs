using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Bloom", fileName = "BloomControlController")]
    public class BloomControlController : ToggleSettingsControlController
    {
        public override object GetStoredValue() { return currentQualitySetting.bloom; }

        public override void UpdateSetting(object newValue)
        {
            currentQualitySetting.bloom = (bool)newValue;

            if (SceneReferences.i.postProcessVolume)
            {
                if (SceneReferences.i.postProcessVolume.profile.TryGet<Bloom>(out Bloom bloom))
                {
                    bloom.active = currentQualitySetting.bloom;
                }
            }
        }
    }
}