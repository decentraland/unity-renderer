using Cinemachine;
using DCL.SettingsCommon.SettingsControllers.BaseControllers;
using UnityEngine;

namespace DCL.SettingsCommon.SettingsControllers.SpecificControllers
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Mouse Sensitivity", fileName = "MouseSensitivityControlController")]
    public class MouseSensivityControlController : SliderSettingsControlController
    {
        internal const float FIRST_PERSON_MIN_SPEED = 25f / 100f;
        internal const float FIRST_PERSON_MAX_SPEED = 350f / 100f;
        internal const float THIRD_PERSON_X_MIN_SPEED = 4.5f / 10f;
        internal const float THIRD_PERSON_X_MAX_SPEED = 4.5f;
        internal const float THIRD_PERSON_Y_MIN_SPEED = 0.05f / 10f;
        internal const float THIRD_PERSON_Y_MAX_SPEED = 0.05f;
        private CinemachinePOV povCamera;

        public override void Initialize()
        {
            base.Initialize();

            povCamera = SceneReferences.i.firstPersonCamera.GetCinemachineComponent<CinemachinePOV>();
        }

        public override object GetStoredValue() { return currentGeneralSettings.mouseSensitivity; }

        public override void UpdateSetting(object newValue)
        {
            currentGeneralSettings.mouseSensitivity = (float)newValue;

            var povSpeed = Mathf.Lerp(FIRST_PERSON_MIN_SPEED, FIRST_PERSON_MAX_SPEED, currentGeneralSettings.mouseSensitivity);
            povCamera.m_HorizontalAxis.m_MaxSpeed = povSpeed;
            povCamera.m_VerticalAxis.m_MaxSpeed = povSpeed;
            SceneReferences.i.thirdPersonCamera.m_XAxis.m_MaxSpeed = Mathf.Lerp(THIRD_PERSON_X_MIN_SPEED, THIRD_PERSON_X_MAX_SPEED, currentGeneralSettings.mouseSensitivity);
            SceneReferences.i.thirdPersonCamera.m_YAxis.m_MaxSpeed = Mathf.Lerp(THIRD_PERSON_Y_MIN_SPEED, THIRD_PERSON_Y_MAX_SPEED, currentGeneralSettings.mouseSensitivity);
        }
    }
}