using DCL.SettingsController;
using UnityEngine;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Draw Distance", fileName = "DrawDistanceControlController")]
    public class DrawDistanceControlController : SliderSettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentQualitySetting.cameraDrawDistance;
        }

        public override void UpdateSetting(object newValue)
        {
            currentQualitySetting.cameraDrawDistance = (float)newValue;

            if (QualitySettingsReferences.i.thirdPersonCamera)
                QualitySettingsReferences.i.thirdPersonCamera.m_Lens.FarClipPlane = currentQualitySetting.cameraDrawDistance;

            if (QualitySettingsReferences.i.firstPersonCamera)
                QualitySettingsReferences.i.firstPersonCamera.m_Lens.FarClipPlane = currentQualitySetting.cameraDrawDistance;

            RenderSettings.fogEndDistance = currentQualitySetting.cameraDrawDistance;
            RenderSettings.fogStartDistance = currentQualitySetting.cameraDrawDistance * 0.8f;
        }
    }
}