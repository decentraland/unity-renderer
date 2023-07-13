using UnityEngine;

namespace MainScripts.DCL.InWorldCamera.Scripts
{
    public class ScreenshotCameraMovement : MonoBehaviour
    {
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

        private float mouseX;
        private float mouseY;
        private float rollInput;

        private void Awake()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            mouseX = 0;
            mouseY = 0;
        }

        private void Update()
        {
            Translate(newPosition: CalculateNewPosition(Time.deltaTime));
            Rotate(Time.deltaTime);
        }

        private void Translate(Vector3 newPosition)
        {
            Vector3 movement = newPosition - transform.position;
            characterController.Move(movement);
        }

        private Vector3 CalculateNewPosition(float deltaTime)
        {
            float horizontal = characterXAxis.GetValue();
            float vertical = characterYAxis.GetValue();

            var upDown = 0f;
            if (cameraUpAction.isOn) upDown = 1f;
            if (cameraDownAction.isOn) upDown = -1f;

            Vector3 movement = new Vector3(horizontal, upDown, vertical) * (movementSpeed * deltaTime);
            return transform.position + transform.TransformDirection(movement);
        }

        private void Rotate(float deltaTime)
        {
            mouseX += cameraXAxis.GetValue() * rotationSpeed * deltaTime;
            mouseY -= cameraYAxis.GetValue() * rotationSpeed * deltaTime;
            mouseY = Mathf.Clamp(mouseY, -90f, 90f); // Limit vertical rotation to avoid camera flipping
            transform.localRotation = Quaternion.Euler(new Vector3(mouseY, mouseX, transform.localRotation.z));
        }
    }
}
