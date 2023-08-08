using DCL;
using DCL.Helpers;
using System;
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
        [SerializeField] private float rotationDamping = 7;
        [SerializeField] private float rollSpeed = 50f;

        [SerializeField] private InputAction_Measurable cameraXAxis;
        [SerializeField] private InputAction_Measurable cameraYAxis;
        [SerializeField] private InputAction_Hold mouseFirstClick;
        [SerializeField] private InputAction_Hold cameraRollPlus;
        [SerializeField] private InputAction_Hold cameraRollMinus;

        private bool rotationIsEnabled;

        private ScreencaptureCameraTranslation translation;
        private Vector2 currentMouseDelta;
        private Vector2 smoothedMouseDelta;

        private void Awake()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            translation = new ScreencaptureCameraTranslation(characterController, translationSpeed, MAX_DISTANCE_FROM_PLAYER, translationInputSchema);
        }

        private void Update()
        {
            translation.Translate(Time.deltaTime, translationDamping, maxTranslationChangePerFrame);
            Rotate(Time.deltaTime, rotationDamping);
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
            smoothedRollRate = 0f;
            SwitchRotation(isEnabled: false);
            Utils.UnlockCursor();
        }

        private void SwitchRotation(bool isEnabled)
        {
            DataStore.i.camera.panning.Set(false);
            rotationIsEnabled = isEnabled;
        }

        private float currentRollRate = 0f; // The current change in roll per frame
        private float smoothedRollRate = 0f; // The smoothed change in roll per frame

        private void Rotate(float deltaTime, float damping)
        {
            // Extract the current yaw and pitch
            float currentYaw = transform.eulerAngles.y;
            float currentPitch = transform.eulerAngles.x;
            float currentRoll = transform.eulerAngles.z;

            // Apply the mouse's delta rotations to yaw and pitch
            if (rotationIsEnabled)
            {
                currentMouseDelta.x = cameraXAxis.GetValue() * rotationSpeed;
                currentMouseDelta.y = cameraYAxis.GetValue() * rotationSpeed;
                // Smooth the mouse delta using damping
                smoothedMouseDelta = Vector2.Lerp(smoothedMouseDelta, currentMouseDelta, deltaTime * damping);

                currentYaw += smoothedMouseDelta.x * deltaTime;
                currentPitch -= smoothedMouseDelta.y * deltaTime;
            }

            // Determine the desired roll rate based on user input
            if (cameraRollPlus.isOn)
                currentRollRate = rollSpeed;
            else if (cameraRollMinus.isOn)
                currentRollRate = -rollSpeed;
            else
                currentRollRate = 0f;

            // Smooth the roll rate
            smoothedRollRate = Mathf.Lerp(smoothedRollRate, currentRollRate, deltaTime * damping);

            // Apply the smoothed roll rate to the current roll
            currentRoll += smoothedRollRate * deltaTime;

            transform.rotation = Quaternion.Euler(currentPitch, currentYaw, currentRoll);
        }
    }
}
