using DCL.SettingsCommon;
using UnityEngine;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Detail Object Culling", fileName = "DetailObjectCullingControlController")]
    public class DetailObjectCullingControlController : ToggleSettingsControlController
    {
        public override object GetStoredValue()
        {
            return currentQualitySetting.enableDetailObjectCulling;
        }

        public override void UpdateSetting(object newValue)
        {
            bool newBoolValue = (bool)newValue;
            currentQualitySetting.enableDetailObjectCulling = newBoolValue;

            Environment.i.platform.cullingController.SetObjectCulling(newBoolValue);
            Environment.i.platform.cullingController.SetShadowCulling(newBoolValue);
            Environment.i.platform.cullingController.MarkDirty();

            CommonSettingsScriptableObjects.detailObjectCullingDisabled.Set(!newBoolValue);
        }
    }
}