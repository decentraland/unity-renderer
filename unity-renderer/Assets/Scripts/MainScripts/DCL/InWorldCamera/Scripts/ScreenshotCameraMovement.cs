using System;
using System.Data;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace MainScripts.DCL.InWorldCamera.Scripts
{
    public class ScreenshotCameraMovement : MonoBehaviour
    {
        public float movementSpeed = 5f;
        public float rotationSpeed = 100f;

        float mouseX;
        float mouseY;
        float rollInput = 0f;

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
            transform.Translate(movement, Space.Self);
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
