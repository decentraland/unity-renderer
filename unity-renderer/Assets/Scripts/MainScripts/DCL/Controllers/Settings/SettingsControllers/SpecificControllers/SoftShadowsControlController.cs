using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/SoftShadows", fileName = "SoftShadowsControlController")]
    public class SoftShadowsControlController : ToggleSettingsControlController
    {
        public override object GetStoredValue() =>
            currentQualitySetting.softShadows;

        public override void UpdateSetting(object newValue)
        {
            currentQualitySetting.softShadows = (bool)newValue;

            if (SceneReferences.i.environmentLight)
            {
                LightShadows shadowType = LightShadows.None;

                if (currentQualitySetting.shadows)
                    shadowType = currentQualitySetting.softShadows ? LightShadows.Soft : LightShadows.Hard;

                SceneReferences.i.environmentLight.shadows = shadowType;
            }
            else
                Debug.LogWarning("Cannot set shadow mode to current light, SceneReferences.i.environmentLight is null");
        }
    }
}
