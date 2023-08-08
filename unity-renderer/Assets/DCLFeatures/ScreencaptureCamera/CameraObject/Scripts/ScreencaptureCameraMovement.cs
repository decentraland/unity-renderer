using DCL;
using DCL.Helpers;
using UnityEngine;

namespace DCLFeatures.ScreencaptureCamera.CameraObject
{
    public class ScreencaptureCameraMovement : MonoBehaviour
    {
        private const float MAX_DISTANCE_FROM_PLAYER = 16f;
        private const float MOVEMENT_SPEED = 5f;

        [SerializeField] private CharacterController characterController;
        [SerializeField] private TranslationInputSchema translationInputSchema;

        [Header("ROTATION")]
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private InputAction_Measurable cameraXAxis;
        [SerializeField] private InputAction_Measurable cameraYAxis;
        [SerializeField] private InputAction_Hold mouseFirstClick;

        private float mouseX;
        private float mouseY;
        private bool rotationIsEnabled;

        private ScreencaptureCameraTranslation translation;
        private void Awake()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            translation = new ScreencaptureCameraTranslation(characterController, MOVEMENT_SPEED, MAX_DISTANCE_FROM_PLAYER, translationInputSchema);
        }

        private void Update()
        {
            translation.Translate(Time.deltaTime);

            if (rotationIsEnabled)
                Rotate(Time.deltaTime);
        }

        private void OnEnable()
        {
            rotationIsEnabled = false;
            mouseX = transform.rotation.eulerAngles.y;
            mouseY = transform.rotation.eulerAngles.x;

            mouseFirstClick.OnStarted += EnableRotation;
            mouseFirstClick.OnFinished += DisableRotation;
        }

        private void OnDisable()
        {
            rotationIsEnabled = false;
            mouseX = transform.rotation.eulerAngles.y;
            mouseY = transform.rotation.eulerAngles.x;

            mouseFirstClick.OnStarted -= EnableRotation;
            mouseFirstClick.OnFinished -= DisableRotation;
        }

        private void EnableRotation(DCLAction_Hold action) =>
            SwitchRotation(isEnabled: true);

        private void DisableRotation(DCLAction_Hold action)
        {
            SwitchRotation(isEnabled: false);
            Utils.UnlockCursor();
        }

        private void SwitchRotation(bool isEnabled)
        {
            DataStore.i.camera.panning.Set(false);
            rotationIsEnabled = isEnabled;
        }

        private void Rotate(float deltaTime)
        {
            mouseX += cameraXAxis.GetValue() * rotationSpeed * deltaTime;
            mouseY -= cameraYAxis.GetValue() * rotationSpeed * deltaTime;

            // mouseY = Mathf.Clamp(mouseY, -90f, 90f);

            transform.rotation = Quaternion.Euler(mouseY, mouseX, 0f);
        }
    }
}
