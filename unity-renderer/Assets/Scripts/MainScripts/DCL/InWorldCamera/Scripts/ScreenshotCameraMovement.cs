﻿using UnityEngine;

namespace MainScripts.DCL.InWorldCamera.Scripts
{
    public class ScreenshotCameraMovement : MonoBehaviour
    {
        [SerializeField] private CharacterController characterController;

        [Space]
        [SerializeField] private InputAction_Measurable characterXAxis;
        [SerializeField] private InputAction_Measurable characterYAxis;
        [SerializeField] private InputAction_Hold cameraUpAction;
        [SerializeField] private InputAction_Hold cameraDownAction;

        [Space]
        [SerializeField] private float movementSpeed = 5f;

        private float mouseX;
        private float mouseY;
        private float rollInput;

        private void Awake()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            Translate(CalculateNewPosition());
        }

        private Vector3 CalculateNewPosition()
        {
            float horizontal = characterXAxis.GetValue();
            float vertical = characterYAxis.GetValue();

            var upDown = 0f;
            if (cameraUpAction.isOn) upDown = 1f;
            if (cameraDownAction.isOn) upDown = -1f;

            Vector3 movement = new Vector3(horizontal, upDown, vertical) * (movementSpeed * Time.deltaTime);
            return transform.position + transform.TransformDirection(movement);
        }

        private void Translate(Vector3 newPosition)
        {
            Vector3 movement = newPosition - transform.position;
            characterController.Move(movement);
        }
    }
}
