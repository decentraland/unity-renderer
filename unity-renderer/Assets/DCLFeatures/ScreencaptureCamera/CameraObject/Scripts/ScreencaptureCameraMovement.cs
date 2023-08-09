using UnityEngine;

namespace DCLFeatures.ScreencaptureCamera.CameraObject
{
    public class ScreencaptureCameraMovement : MonoBehaviour
    {
        private const float MAX_DISTANCE_FROM_PLAYER = 16f;

        [SerializeField] private CharacterController characterController;

        [Header("TRANSLATION")]
        [SerializeField] private float translationSpeed = 5f;
        [SerializeField] private float maxTranslationPerFrame = 1f;
        [SerializeField] private float translationDamping = 5f;
        [SerializeField] private TranslationInputSchema translationInputSchema;

        [Header("ROTATION")]
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private float maxRotationPerFrame = 1.5f;
        [SerializeField] private float rotationDamping = 7;
        [SerializeField] private float rollSpeed = 50f;
        [SerializeField] private RotationInputSchema rotationInputSchema;

        private ScreencaptureCameraTranslation translation;
        private ScreencaptureCameraRotation rotation;
        private Transform target;

        public void SetTarget(Transform target)
        {
            this.target = target;
        }

        private void Awake()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            rotation = new ScreencaptureCameraRotation(transform, rotationInputSchema);
            translation = new ScreencaptureCameraTranslation(characterController,  translationInputSchema);
        }

        private void Update()
        {
            rotation.Rotate(target, Time.deltaTime, rotationSpeed, rollSpeed, rotationDamping, maxRotationPerFrame);
            translation.Translate(target, Time.deltaTime, translationSpeed,  translationDamping, maxTranslationPerFrame, MAX_DISTANCE_FROM_PLAYER);
        }

        private void OnEnable()
        {
            rotation.Activate();
        }

        private void OnDisable()
        {
            rotation.Deactivate();
        }
    }
}
