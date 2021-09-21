using DCL.Interface;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Base Resolution", fileName = "BaseResolutionControlController")]
    public class BaseResolutionControlController : SpinBoxSettingsControlController
    {
        public override object GetStoredValue() { return (int)currentQualitySetting.baseResolution; }

        public override void UpdateSetting(object newValue)
        {
            currentQualitySetting.baseResolution = (QualitySettings.BaseResolution)newValue;

            switch (currentQualitySetting.baseResolution)
            {
                case QualitySettings.BaseResolution.BaseRes_720:
                    WebInterface.SetBaseResolution(820);
                    break;
                case QualitySettings.BaseResolution.BaseRes_1080:
                    WebInterface.SetBaseResolution(1080);
                    break;
                case QualitySettings.BaseResolution.BaseRes_Unlimited:
                    WebInterface.SetBaseResolution(9999);
                    break;
            }
        }
    }
}