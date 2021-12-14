using DCL.Interface;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Dynamic Skybox", fileName = "DynamicSkyboxControlController")]
    public class DynamicSkyboxControlController : SpinBoxSettingsControlController
    {
        // Start is called before the first frame update
        public override object GetStoredValue() { return (int)currentGeneralSettings.proceduralSkyboxMode; }

        // Update is called once per frame
        public override void UpdateSetting(object newValue)
        {
            int newIntValue = (int)newValue;
            currentGeneralSettings.proceduralSkyboxMode = (GeneralSettings.ProceduralSkyboxMode)newIntValue;
            if (currentGeneralSettings.proceduralSkyboxMode == GeneralSettings.ProceduralSkyboxMode.DYNAMIC)
            {
                DataStore.i.skyboxConfig.useDynamicSkybox.Set(true);
            }
            else
            {
                DataStore.i.skyboxConfig.useDynamicSkybox.Set(false);
            }
        }
    }
}