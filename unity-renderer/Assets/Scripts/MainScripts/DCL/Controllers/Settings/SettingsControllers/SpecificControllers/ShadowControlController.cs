using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Shadow", fileName = "ShadowControlController")]
    public class ShadowControlController : ToggleSettingsControlController
    {
        public override object GetStoredValue() =>
            currentQualitySetting.shadows;

        public override void UpdateSetting(object newValue)
        {
            currentQualitySetting.shadows = (bool)newValue;

            if (SceneReferences.i.environmentLight)
            {
                LightShadows shadowType = LightShadows.None;

                if (currentQualitySetting.shadows)
                    shadowType = currentQualitySetting.shadows ? LightShadows.Soft : LightShadows.Hard;

                SceneReferences.i.environmentLight.shadows = shadowType;
            }
            else
                Debug.LogWarning("Cannot set shadow mode to current light, SceneReferences.i.environmentLight is null");

            CommonSettingsScriptableObjects.shadowsDisabled.Set(!currentQualitySetting.shadows);
        }
    }
}
