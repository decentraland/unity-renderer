using DCL;
using DCL.Helpers;
using UnityEngine;

namespace DCLFeatures.ScreencaptureCamera
{
    public class ScreencaptureCameraMovement : MonoBehaviour
    {
        private const float MAX_DISTANCE_FROM_PLAYER = 16f;

        [SerializeField] private CharacterController characterController;

        [Header("TRANSLATION")]
        [SerializeField] private float movementSpeed = 5f;
        [SerializeField] private InputAction_Measurable characterXAxis;
        [SerializeField] private InputAction_Measurable characterYAxis;
        [SerializeField] private InputAction_Hold cameraUpAction;
        [SerializeField] private InputAction_Hold cameraDownAction;

        [Header("ROTATION")]
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private InputAction_Measurable cameraXAxis;
        [SerializeField] private InputAction_Measurable cameraYAxis;
        [SerializeField] private InputAction_Hold mouseFirstClick;

        private float mouseX;
        private float mouseY;
        private bool rotationIsEnabled;

        private void Awake()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            Translate(movementVector: InputToMoveVector(Time.deltaTime));

            if (rotationIsEnabled)
                Rotate(Time.deltaTime);
        }

        private void OnEnable()
        {
            mouseX = transform.rotation.eulerAngles.y;
            mouseY = transform.rotation.eulerAngles.x;

            mouseFirstClick.OnStarted += EnableRotation;
            mouseFirstClick.OnFinished += DisableRotation;
        }

        private void OnDisable()
        {
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

        private void Translate(Vector3 movementVector) =>
            characterController.Move(
                RestrictedMovementBySemiSphere(movementVector));

        private Vector3 RestrictedMovementBySemiSphere(Vector3 movementVector)
        {
            if (characterController.transform.position.y + movementVector.y <= 0f)
                movementVector.y = 0f;

            Vector3 playerPosition = DataStore.i.player.playerUnityPosition.Get();
            Vector3 desiredCameraPosition = characterController.transform.position + movementVector;

            float distanceFromPlayer = Vector3.Distance(desiredCameraPosition, playerPosition);

            if (distanceFromPlayer > MAX_DISTANCE_FROM_PLAYER)
            {
                // If the distance is greater than the allowed radius, correct the movement vector
                Vector3 directionFromPlayer = (desiredCameraPosition - playerPosition).normalized;
                desiredCameraPosition = playerPosition + (directionFromPlayer * MAX_DISTANCE_FROM_PLAYER);
                movementVector = desiredCameraPosition - characterController.transform.position;
            }

            return movementVector;
        }

        private Vector3 InputToMoveVector(float deltaTime)
        {
            Vector3 forward = transform.forward.normalized * characterYAxis.GetValue();
            Vector3 horizontal = transform.right.normalized * characterXAxis.GetValue();

            float verticalDirection = cameraUpAction.isOn ? 1 :
                cameraDownAction.isOn ? -1 : 0f;

            Vector3 vertical = transform.up.normalized * verticalDirection;

            return (forward + horizontal + vertical) * (movementSpeed * deltaTime);
        }

        private void Rotate(float deltaTime)
        {
            mouseX += cameraXAxis.GetValue() * rotationSpeed * deltaTime;
            mouseY -= cameraYAxis.GetValue() * rotationSpeed * deltaTime;

            mouseY = Mathf.Clamp(mouseY, -90f, 90f);

            transform.rotation = Quaternion.Euler(mouseY, mouseX, 0f);
        }
    }
}
