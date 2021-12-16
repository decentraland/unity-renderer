using DCL.Interface;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Dynamic Skybox", fileName = "DynamicSkyboxControlController")]
    public class DynamicSkyboxControlController : ToggleSettingsControlController
    {
        // Start is called before the first frame update
        public override object GetStoredValue() { return currentGeneralSettings.dynamicProceduralSkbox; }

        // Update is called once per frame
        public override void UpdateSetting(object newValue)
        {
            currentGeneralSettings.dynamicProceduralSkbox = (bool)newValue;
            if (currentGeneralSettings.dynamicProceduralSkbox)
            {
                DataStore.i.skyboxConfig.useDynamicSkybox.Set(true);
            }
            else
            {
                DataStore.i.skyboxConfig.useDynamicSkybox.Set(false);
            }

            CommonSettingsScriptableObjects.dynamicSkyboxDisabled.Set(currentGeneralSettings.dynamicProceduralSkbox);
        }
    }
}