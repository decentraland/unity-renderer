using UnityEngine;

namespace MainScripts.DCL.InWorldCamera.Scripts
{
    public class ScreenshotCameraMovement : MonoBehaviour
    {
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private CharacterController characterController;

        public float movementSpeed = 5f;
        public float rotationSpeed = 100f;

        float mouseX;
        float mouseY;
        float rollInput = 0f;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            // Initialize mouseX, mouseY, and roll with current camera rotation
            Vector3 currentRotation = transform.rotation.eulerAngles;
            mouseX = currentRotation.y;
            mouseY = currentRotation.x;
        }

        private void Update()
        {
            Translate();

            rollInput = 0f;

            if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.LeftShift))
                Roll();
            else
                Rotate();
        }

        private void Translate()
        {
            // Camera movement
            float vertical = Input.GetAxis("Vertical");
            float horizontal = Input.GetAxis("Horizontal");

            var upDown = 0f;
            if (Input.GetKey(KeyCode.Q))
                upDown = 1f;
            else if (Input.GetKey(KeyCode.E))
                upDown = -1f;

            Vector3 movement = new Vector3(horizontal, upDown, vertical) * movementSpeed * Time.deltaTime;
            Vector3 newPosition = transform.position + transform.TransformDirection(movement);

            MoveCamera(newPosition);
        }

        private void MoveCamera(Vector3 newPosition)
        {
            // const float RADIUS = 0.5f; // Adjust this value based on the size of your camera collider
            // float maxDistance = Vector3.Distance(transform.position, newPosition);
            //
            // if (Physics.SphereCast(transform.position, RADIUS, newPosition - transform.position, out RaycastHit hit, maxDistance))
            //     newPosition = hit.point - ((newPosition - transform.position).normalized * RADIUS);
            //
            // // Set the new position
            // transform.position = newPosition;

            Vector3 movement = newPosition - transform.position;
            characterController.Move(movement);
        }

        private void Rotate()
        {
            mouseX += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            mouseY -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
            mouseY = Mathf.Clamp(mouseY, -90f, 90f); // Limit vertical rotation to avoid camera flipping
            Vector3 rotation = new Vector3(mouseY, mouseX, transform.localRotation.z);
            transform.localRotation = Quaternion.Euler(rotation);
        }

        private void Roll()
        {
            if (Input.GetKey(KeyCode.LeftShift))
                rollInput = 1f;
            else if (Input.GetKey(KeyCode.Z))
                rollInput = -1f;

            // Calculate roll amount
            float rollAmount = rollInput * rotationSpeed * Time.deltaTime;

            // Apply roll rotation
            transform.Rotate(Vector3.forward, rollAmount);
        }
    }
}
