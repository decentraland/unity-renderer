using Cinemachine;
using DCL.SettingsController;
using UnityEngine;

namespace DCL.SettingsControls
{
    [CreateAssetMenu(menuName = "Settings/Controllers/Controls/Mouse Sensivity", fileName = "MouseSensivityControlController")]
    public class MouseSensivityControlController : SliderSettingsControlController
    {
        internal const float FIRST_PERSON_MIN_SPEED = 25f;
        internal const float FIRST_PERSON_MAX_SPEED = 350f;
        internal const float THIRD_PERSON_X_MIN_SPEED = 100f;
        internal const float THIRD_PERSON_X_MAX_SPEED = 450f;
        internal const float THIRD_PERSON_Y_MIN_SPEED = 0.5f;
        internal const float THIRD_PERSON_Y_MAX_SPEED = 3f;

        private CinemachinePOV povCamera;

        public override void Initialize()
        {
            base.Initialize();

            povCamera = GeneralSettingsReferences.i.firstPersonCamera.GetCinemachineComponent<CinemachinePOV>();
        }

        public override object GetStoredValue()
        {
            return currentGeneralSettings.mouseSensitivity;
        }

        public override void UpdateSetting(object newValue)
        {
            currentGeneralSettings.mouseSensitivity = (float)newValue;

            var povSpeed = Mathf.Lerp(FIRST_PERSON_MIN_SPEED, FIRST_PERSON_MAX_SPEED, currentGeneralSettings.mouseSensitivity);
            povCamera.m_HorizontalAxis.m_MaxSpeed = povSpeed;
            povCamera.m_VerticalAxis.m_MaxSpeed = povSpeed;
            GeneralSettingsReferences.i.thirdPersonCamera.m_XAxis.m_MaxSpeed = Mathf.Lerp(THIRD_PERSON_X_MIN_SPEED, THIRD_PERSON_X_MAX_SPEED, currentGeneralSettings.mouseSensitivity);
            GeneralSettingsReferences.i.thirdPersonCamera.m_YAxis.m_MaxSpeed = Mathf.Lerp(THIRD_PERSON_Y_MIN_SPEED, THIRD_PERSON_Y_MAX_SPEED, currentGeneralSettings.mouseSensitivity);
        }
    }
}