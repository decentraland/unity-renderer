using UnityEngine;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Max Non-LOD Avatars", fileName = "MaxNonLODAvatarsControlController")]
    public class MaxNonLODAvatarsControlController : SliderSettingsControlController
    {
        public override void Initialize()
        {
            base.Initialize();

            UpdateSetting(currentGeneralSettings.maxNonLODAvatars);
        }

        public override object GetStoredValue() { return currentGeneralSettings.maxNonLODAvatars; }

        public override void UpdateSetting(object newValue)
        {
            AvatarsLODController.i.maxNonLODAvatars = (int)((float)newValue);
            AvatarsLODController.i.UpdateAllLODs();
        }
    }
}