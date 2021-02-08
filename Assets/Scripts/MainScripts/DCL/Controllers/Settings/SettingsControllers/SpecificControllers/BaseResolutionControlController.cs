using DCL.Interface;
using UnityEngine;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Base Resolution", fileName = "BaseResolutionControlController")]
    public class BaseResolutionControlController : SpinBoxSettingsControlController
    {
        public override object GetStoredValue()
        {
            return (int)currentQualitySetting.baseResolution;
        }

        public override void UpdateSetting(object newValue)
        {
            currentQualitySetting.baseResolution = (SettingsData.QualitySettings.BaseResolution)newValue;

            switch (currentQualitySetting.baseResolution)
            {
                case SettingsData.QualitySettings.BaseResolution.BaseRes_720:
                    WebInterface.SetBaseResolution(720);
                    break;
                case SettingsData.QualitySettings.BaseResolution.BaseRes_1080:
                    WebInterface.SetBaseResolution(1080);
                    break;
                case SettingsData.QualitySettings.BaseResolution.BaseRes_Unlimited:
                    WebInterface.SetBaseResolution(9999);
                    break;
            }
        }
    }
}