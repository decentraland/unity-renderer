using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Hide UI", fileName = "HideUIControlController")]
    public class HideUIControlController : ToggleSettingsControlController
    {
        public override void Initialize()
        {
            base.Initialize();
            CommonScriptableObjects.allUIHidden.OnChange += AllUIHiddenChanged;

            AllUIHiddenChanged(currentGeneralSettings.hideUI);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            CommonScriptableObjects.allUIHidden.OnChange -= AllUIHiddenChanged;
        }

        public override object GetStoredValue() =>
            currentGeneralSettings.hideUI;

        public override void UpdateSetting(object newValue) =>
            CommonScriptableObjects.allUIHidden.Set((bool)newValue);

        private void AllUIHiddenChanged(bool current, bool _ = false)
        {
            currentGeneralSettings.hideUI = current;
            ApplySettings();
        }
    }
}
