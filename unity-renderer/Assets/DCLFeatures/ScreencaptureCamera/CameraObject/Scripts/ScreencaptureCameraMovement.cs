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

        private bool targetIsSet;

        private void Awake()
        {
            rotation = new ScreencaptureCameraRotation(rotationInputSchema);
            translation = new ScreencaptureCameraTranslation(translationInputSchema);
        }

        private void Update()
        {
            if (!targetIsSet) return;

            rotation.Rotate(target.transform, Time.deltaTime, rotationSpeed, rotationDamping, maxRotationPerFrame);
            translation.Translate(target, translationSpeed, MAX_DISTANCE_FROM_PLAYER, Time.deltaTime);
        }

        private void OnEnable()
        {
            rotation.Activate();
        }

        private void OnDisable()
        {
            rotation.Deactivate();
        }

        public void SetTarget(CharacterController cameraTarget)
        {
            this.target = cameraTarget;
            targetIsSet = true;
        }
    }
}
