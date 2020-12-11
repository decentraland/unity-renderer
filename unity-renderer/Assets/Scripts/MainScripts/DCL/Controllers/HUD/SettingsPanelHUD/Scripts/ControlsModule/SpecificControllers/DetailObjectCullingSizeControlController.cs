using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Detail Object Culling Size", fileName = "DetailObjectCullingSizeControlController")]
    public class DetailObjectCullingSizeControlController : SettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentQualitySetting.detailObjectCullingThreshold;
        }

        public override void OnControlChanged(object newValue)
        {
            currentQualitySetting.detailObjectCullingThreshold = (float)newValue;
        }
    }
}