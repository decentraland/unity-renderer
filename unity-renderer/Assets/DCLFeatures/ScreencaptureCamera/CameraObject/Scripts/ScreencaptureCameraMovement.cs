using DCL;
using DCL.Helpers;
using UnityEngine;

namespace DCLFeatures.ScreencaptureCamera.CameraObject
{
    public class ScreencaptureCameraMovement : MonoBehaviour
    {
        private const float MAX_DISTANCE_FROM_PLAYER = 16f;

        [SerializeField] private CharacterController characterController;
        [SerializeField] private TranslationInputSchema translationInputSchema;

        [SerializeField] private float translationSpeed = 5f;
        [SerializeField] private float maxTranslationChangePerFrame = 0.5f;
        [SerializeField] private float translationDamping = 5f;

        [Header("ROTATION")]
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private float maxRotationChangePerFrame = 5f;
        [SerializeField] private float rotationDamping = 7;

        [SerializeField] private InputAction_Measurable cameraXAxis;
        [SerializeField] private InputAction_Measurable cameraYAxis;
        [SerializeField] private InputAction_Hold mouseFirstClick;

        private float mouseX;
        private float mouseY;
        private bool rotationIsEnabled;

        private ScreencaptureCameraTranslation translation;
        private Vector2 currentMouseDelta;
        private Vector2 smoothedMouseDelta;

        private void Awake()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            translation = new ScreencaptureCameraTranslation(characterController, translationSpeed, MAX_DISTANCE_FROM_PLAYER, translationInputSchema);

            mouseX = transform.rotation.eulerAngles.y;
            mouseY = transform.rotation.eulerAngles.x;
        }

        private void Update()
        {
            translation.Translate(Time.deltaTime, translationDamping, maxTranslationChangePerFrame);

            if (rotationIsEnabled)
                Rotate(Time.deltaTime, rotationDamping, maxRotationChangePerFrame);
        }

        private void OnEnable()
        {
            if (Utils.IsCursorLocked)
                EnableRotation();

            mouseFirstClick.OnStarted += EnableRotation;
            mouseFirstClick.OnFinished += DisableRotation;
        }

        private void OnDisable()
        {
            DisableRotation();

            mouseFirstClick.OnStarted -= EnableRotation;
            mouseFirstClick.OnFinished -= DisableRotation;
        }

        private void EnableRotation(DCLAction_Hold action) =>
            EnableRotation();

        private void EnableRotation() =>
            SwitchRotation(isEnabled: true);

        private void DisableRotation(DCLAction_Hold action) =>
            DisableRotation();

        private void DisableRotation()
        {
            SwitchRotation(isEnabled: false);
            Utils.UnlockCursor();
        }

        private void SwitchRotation(bool isEnabled)
        {
            DataStore.i.camera.panning.Set(false);

            mouseX = transform.rotation.eulerAngles.y;
            mouseY = transform.rotation.eulerAngles.x;

            rotationIsEnabled = isEnabled;
        }

        private void Rotate(float deltaTime, float damping, float max)
        {
            currentMouseDelta.x = cameraXAxis.GetValue() * rotationSpeed;
            currentMouseDelta.y = cameraYAxis.GetValue() * rotationSpeed;
            smoothedMouseDelta = Vector2.Lerp(smoothedMouseDelta, currentMouseDelta, deltaTime * rotationDamping);
            smoothedMouseDelta = Vector2.ClampMagnitude(smoothedMouseDelta, max);

            mouseX += smoothedMouseDelta.x * deltaTime;
            mouseY -= smoothedMouseDelta.y * deltaTime;
            mouseY = Mathf.Clamp(mouseY, -90f, 90f);

            var targetRotation = Quaternion.Euler(mouseY, mouseX, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, deltaTime * damping);
        }
    }
}
