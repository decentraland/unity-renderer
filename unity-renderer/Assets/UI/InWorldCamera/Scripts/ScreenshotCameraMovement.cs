﻿using DCL;
using DCL.Helpers;
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
            Translate(newPosition: CalculateNewPosition(Time.deltaTime));

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

        private void Translate(Vector3 newPosition) =>
            characterController.Move(newPosition);

        private Vector3 CalculateNewPosition(float deltaTime)
        {
            var upDown = 0f;
            if (cameraUpAction.isOn) upDown = 1f;
            if (cameraDownAction.isOn) upDown = -1f;

            return new Vector3(characterXAxis.GetValue(), upDown, characterYAxis.GetValue()) * (movementSpeed * deltaTime);
        }

        private void Rotate(float deltaTime)
        {
            mouseX += cameraXAxis.GetValue() * rotationSpeed * deltaTime;
            mouseY -= cameraYAxis.GetValue() * rotationSpeed * deltaTime;

            transform.rotation = Quaternion.Euler(mouseY, mouseX, 0f);
        }
    }
}
