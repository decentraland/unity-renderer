using DCL;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace MainScripts.DCL.Controllers.Settings.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Profanity Chat Filtering", fileName = "ProfanityChatFilteringControlController")]
    public class ProfanityChatFilterControlController : ToggleSettingsControlController
    {
        public override object GetStoredValue() { return currentGeneralSettings.profanityChatFiltering; }
        
        public override void UpdateSetting(object newValue)
        {
            var value = (bool) newValue;
            currentGeneralSettings.profanityChatFiltering = value;
            DataStore.i.settings.profanityChatFilteringEnabled.Set(value);
        }
    }
}