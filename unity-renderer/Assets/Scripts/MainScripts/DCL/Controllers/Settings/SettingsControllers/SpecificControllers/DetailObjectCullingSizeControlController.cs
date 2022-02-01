using DCL.Rendering;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;
using Environment = DCL.Environment;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Detail Object Culling Size", fileName = "DetailObjectCullingSizeControlController")]
    public class DetailObjectCullingSizeControlController : SliderSettingsControlController
    {
        public CullingControllerSettingsData cullingControllerSettingsData;
        public override object GetStoredValue() { return currentQualitySetting.detailObjectCullingLimit; }

        public override void UpdateSetting(object newValue)
        {
            currentQualitySetting.detailObjectCullingLimit = (float)newValue;

            if (currentQualitySetting.enableDetailObjectCulling)
            {
                var cullingController = Environment.i.platform.cullingController;

                if ( cullingController != null )
                {
                    var settings = cullingController.GetSettingsCopy();

                    if ( settings != null )
                    {
                        settings.rendererProfile = CullingControllerProfile.Lerp(
                            cullingControllerSettingsData.rendererProfileMin,
                            cullingControllerSettingsData.rendererProfileMax,
                            currentQualitySetting.detailObjectCullingLimit / 100.0f);

                        settings.skinnedRendererProfile = CullingControllerProfile.Lerp(
                            cullingControllerSettingsData.skinnedRendererProfileMin,
                            cullingControllerSettingsData.skinnedRendererProfileMax,
                            currentQualitySetting.detailObjectCullingLimit / 100.0f);

                        cullingController.SetSettings(settings);
                    }
                }
            }
        }
    }
}