using System;
using DCL.Rendering;
using DCL.SettingsController;
using UnityEngine;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Detail Object Culling Size", fileName = "DetailObjectCullingSizeControlController")]
    public class DetailObjectCullingSizeControlController : SliderSettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentQualitySetting.detailObjectCullingLimit;
        }

        public override void UpdateSetting(object newValue)
        {
            currentQualitySetting.detailObjectCullingLimit = (float)newValue;

            if (currentQualitySetting.enableDetailObjectCulling)
            {
                var settings = Environment.i.platform.cullingController.GetSettingsCopy();

                settings.rendererProfile = CullingControllerProfile.Lerp(
                    QualitySettingsReferences.i.cullingControllerSettingsData.rendererProfileMin,
                    QualitySettingsReferences.i.cullingControllerSettingsData.rendererProfileMax,
                    currentQualitySetting.detailObjectCullingLimit / 100.0f);

                settings.skinnedRendererProfile = CullingControllerProfile.Lerp(
                    QualitySettingsReferences.i.cullingControllerSettingsData.skinnedRendererProfileMin,
                    QualitySettingsReferences.i.cullingControllerSettingsData.skinnedRendererProfileMax,
                    currentQualitySetting.detailObjectCullingLimit / 100.0f);

                Environment.i.platform.cullingController.SetSettings(settings);
            }
        }
    }
}