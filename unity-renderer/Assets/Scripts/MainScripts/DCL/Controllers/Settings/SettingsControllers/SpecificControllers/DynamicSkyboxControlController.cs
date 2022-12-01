using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Dynamic Skybox", fileName = "DynamicSkyboxControlController")]
    public class DynamicSkyboxControlController : ToggleSettingsControlController
    {
        // Start is called before the first frame update
        public override object GetStoredValue() =>
            currentGeneralSettings.dynamicProceduralSkybox;

        // Update is called once per frame
        public override void UpdateSetting(object newValue)
        {
            currentGeneralSettings.dynamicProceduralSkybox = (bool)newValue;

            DataStore.i.skyboxConfig.mode.Set(
                currentGeneralSettings.dynamicProceduralSkybox
                    ? SkyboxMode.Dynamic
                    : SkyboxMode.HoursFixedByUser);

            CommonSettingsScriptableObjects.dynamicSkyboxDisabled.Set(currentGeneralSettings.dynamicProceduralSkybox);
        }
    }
}
