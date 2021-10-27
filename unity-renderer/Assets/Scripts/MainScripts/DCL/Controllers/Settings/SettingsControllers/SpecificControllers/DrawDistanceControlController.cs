using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Draw Distance", fileName = "DrawDistanceControlController")]
    public class DrawDistanceControlController : SliderSettingsControlController
    {
        public override object GetStoredValue() { return currentQualitySetting.cameraDrawDistance; }

        public override void UpdateSetting(object newValue)
        {
            currentQualitySetting.cameraDrawDistance = (float)newValue;

            if (SceneReferences.i.thirdPersonCamera)
                SceneReferences.i.thirdPersonCamera.m_Lens.FarClipPlane = currentQualitySetting.cameraDrawDistance;

            if (SceneReferences.i.firstPersonCamera)
                SceneReferences.i.firstPersonCamera.m_Lens.FarClipPlane = currentQualitySetting.cameraDrawDistance;

            RenderSettings.fogEndDistance = currentQualitySetting.cameraDrawDistance;
            RenderSettings.fogStartDistance = currentQualitySetting.cameraDrawDistance * 0.8f;
        }
    }
}