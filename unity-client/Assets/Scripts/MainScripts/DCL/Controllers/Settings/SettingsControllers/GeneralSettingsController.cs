using Cinemachine;
using UnityEngine;

namespace DCL.SettingsController
{
    public class GeneralSettingsController : MonoBehaviour
    {
        internal const float FIRST_PERSON_MIN_SPEED = 25f;
        internal const float FIRST_PERSON_MAX_SPEED = 350f;
        internal const float THIRD_PERSON_X_MIN_SPEED = 100f;
        internal const float THIRD_PERSON_X_MAX_SPEED = 450f;
        internal const float THIRD_PERSON_Y_MIN_SPEED = 0.5f;
        internal const float THIRD_PERSON_Y_MAX_SPEED = 3f;

        public CinemachineFreeLook thirdPersonCamera;
        public CinemachineVirtualCamera firstPersonCamera;

        private CinemachinePOV povCamera;

        void Awake()
        {
            povCamera = firstPersonCamera.GetCinemachineComponent<CinemachinePOV>();
        }

        void Start()
        {
            ApplyGeneralSettings(Settings.i.generalSettings);
        }

        void OnEnable()
        {
            Settings.i.OnGeneralSettingsChanged += ApplyGeneralSettings;
        }

        void OnDisable()
        {
            Settings.i.OnGeneralSettingsChanged -= ApplyGeneralSettings;
        }

        void ApplyGeneralSettings(DCL.SettingsData.GeneralSettings settings)
        {
            var povSpeed = Mathf.Lerp(FIRST_PERSON_MIN_SPEED, FIRST_PERSON_MAX_SPEED, settings.mouseSensitivity);
            povCamera.m_HorizontalAxis.m_MaxSpeed = povSpeed;
            povCamera.m_VerticalAxis.m_MaxSpeed = povSpeed;
            thirdPersonCamera.m_XAxis.m_MaxSpeed = Mathf.Lerp(THIRD_PERSON_X_MIN_SPEED, THIRD_PERSON_X_MAX_SPEED, settings.mouseSensitivity);
            thirdPersonCamera.m_YAxis.m_MaxSpeed = Mathf.Lerp(THIRD_PERSON_Y_MIN_SPEED, THIRD_PERSON_Y_MAX_SPEED, settings.mouseSensitivity);

            AudioListener.volume = settings.sfxVolume;
            DCL.Interface.WebInterface.ApplySettings(settings.voiceChatVolume, (int)settings.voiceChatAllow);
        }
    }
}