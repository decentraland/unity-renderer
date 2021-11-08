using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Players Name Opacity", fileName = "PlayersNameOpacity")]
    public class PlayersNameOpacityControlController : SliderSettingsControlController
    {
        public override void Initialize()
        {
            base.Initialize();
            UpdateSetting(currentGeneralSettings.namesOpacity);
        }

        public override object GetStoredValue() { return currentGeneralSettings.namesOpacity; }

        public override void UpdateSetting(object newValue)
        {
            float valueAsFloat = (float)newValue;
            currentGeneralSettings.namesOpacity = valueAsFloat;
            DataStore.i.HUDs.avatarNamesOpacity.Set(valueAsFloat);
            RaiseOnIndicatorLabelChange(valueAsFloat.ToString("0.0"));
        }
    }
}