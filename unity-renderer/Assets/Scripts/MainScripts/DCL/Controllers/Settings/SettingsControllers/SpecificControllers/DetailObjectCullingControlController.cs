using DCL;
using DCL.Rendering;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Detail Object Culling", fileName = "DetailObjectCullingControlController")]
    public class DetailObjectCullingControlController : ToggleSettingsControlController
    {
        public override object GetStoredValue() { return currentQualitySetting.enableDetailObjectCulling; }

        public override void UpdateSetting(object newValue)
        {
            bool newBoolValue = (bool)newValue;
            currentQualitySetting.enableDetailObjectCulling = newBoolValue;

            ICullingController cullingController = Environment.i.platform.cullingController;

            if ( cullingController != null )
            {
                cullingController.SetObjectCulling(newBoolValue);
                cullingController.SetShadowCulling(newBoolValue);
                cullingController.MarkDirty();
            }

            CommonSettingsScriptableObjects.detailObjectCullingDisabled.Set(!newBoolValue);
        }
    }
}