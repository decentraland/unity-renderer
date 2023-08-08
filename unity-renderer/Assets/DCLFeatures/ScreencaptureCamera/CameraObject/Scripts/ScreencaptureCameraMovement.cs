using UnityEngine;

namespace DCLFeatures.ScreencaptureCamera.CameraObject
{
    public class ScreencaptureCameraMovement : MonoBehaviour
    {
        private const float MAX_DISTANCE_FROM_PLAYER = 16f;

        [SerializeField] private CharacterController characterController;

        [Header("TRANSLATION")]
        [SerializeField] private float translationSpeed = 5f;
        [SerializeField] private float maxTranslationPerFrame = 5f;
        [SerializeField] private float translationDamping = 5f;
        [SerializeField] private TranslationInputSchema translationInputSchema;

        [Header("ROTATION")]
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private float rotationDamping = 7;
        [SerializeField] private float rollSpeed = 50f;
        [SerializeField] private RotationInputSchema rotationInputSchema;

        private ScreencaptureCameraTranslation translation;
        private ScreencaptureCameraRotation rotation;

        private void Awake()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            translation = new ScreencaptureCameraTranslation(characterController,  translationInputSchema);
            rotation = new ScreencaptureCameraRotation(transform, rotationInputSchema);
        }

        private void Update()
        {
            translation.Translate(Time.deltaTime, translationSpeed,  translationDamping, maxTranslationPerFrame, MAX_DISTANCE_FROM_PLAYER);
            rotation.Rotate(Time.deltaTime, rotationSpeed, rollSpeed, rotationDamping);
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
