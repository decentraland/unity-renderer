using Cinemachine;
using UnityEngine;

namespace DCLFeatures.ScreencaptureCamera.CameraObject
{
    public class ScreencaptureCameraMovement : MonoBehaviour
    {
        private const float MAX_DISTANCE_FROM_PLAYER = 16f;

        [Header("TRANSLATION")]
        [SerializeField] private float translationSpeed = 5f;
        [SerializeField] private TranslationInputSchema translationInputSchema;

        [Header("ROTATION")]
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private float maxRotationPerFrame = 1.5f;
        [SerializeField] private float rotationDamping = 7;
        [SerializeField] private RotationInputSchema rotationInputSchema;

        private ScreencaptureCameraTranslation translation;
        private ScreencaptureCameraRotation rotation;
        private CharacterController target;

        private bool isInitialized;
        private CinemachineVirtualCamera virtualCamera;
        private CinemachineBrain cinemachineBrain;
        private Transform characterCamera;

        public void Initialize(CharacterController cameraTarget, CinemachineVirtualCamera virtualCamera, Transform characterCameraTransform)
        {
            target = cameraTarget;
            this.virtualCamera = virtualCamera;
            characterCamera = characterCameraTransform;

            isInitialized = true;
        }

        private void Awake()
        {
            cinemachineBrain = GetComponent<CinemachineBrain>();

            rotation = new ScreencaptureCameraRotation(rotationInputSchema);
            translation = new ScreencaptureCameraTranslation(translationInputSchema);
        }

        private void Update()
        {
            if (!isInitialized) return;

            rotation.Rotate(target.transform, Time.deltaTime, rotationSpeed, rotationDamping, maxRotationPerFrame);
            translation.Translate(target, translationSpeed, MAX_DISTANCE_FROM_PLAYER, Time.deltaTime);
        }

        private void OnEnable()
        {
            rotation.Activate();
            ResetVirtualCameraToCharacter();
        }

        private void OnDisable()
        {
            rotation.Deactivate();
        }

        private void ResetVirtualCameraToCharacter()
        {
            CinemachineHardLockToTarget body = virtualCamera.GetCinemachineComponent<CinemachineHardLockToTarget>();
            CinemachineSameAsFollowTarget composer = virtualCamera.GetCinemachineComponent<CinemachineSameAsFollowTarget>();

            body.m_Damping = 0;
            composer.m_Damping = 0;

            target.enabled = false;
            target.transform.SetPositionAndRotation(characterCamera.position, characterCamera.rotation);
            target.enabled = true;

            cinemachineBrain.ManualUpdate();
            body.m_Damping = 1;
            composer.m_Damping = 1;
        }
    }
}
