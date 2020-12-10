using DCL.SettingsPanelHUD.Common;
using UnityEngine;

namespace DCL.SettingsPanelHUD.Controls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Detail Object Culling", fileName = "DetailObjectCullingControlController")]
    public class DetailObjectCullingControlController : SettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentQualitySetting.enableDetailObjectCulling;
        }

        public override void OnControlChanged(object newValue)
        {
            bool detailObjectCullingValue = (bool)newValue;
            currentQualitySetting.enableDetailObjectCulling = detailObjectCullingValue;
            CommonSettingsScriptableObjects.detailObjectCullingDisabled.Set(!detailObjectCullingValue);
        }
    }
}