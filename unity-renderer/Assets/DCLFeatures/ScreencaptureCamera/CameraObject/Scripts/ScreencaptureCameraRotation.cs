using DCL;
using DCL.Helpers;
using UnityEngine;

namespace DCLFeatures.ScreencaptureCamera.CameraObject
{
    public class ScreencaptureCameraRotation
    {
        private readonly Transform target;
        private readonly RotationInputSchema input;

        private bool mouseControlIsEnabled;

        private Vector2 currentMouseDelta;
        private Vector2 smoothedMouseDelta;

        private float currentRollRate;
        private float smoothedRollRate;

        public ScreencaptureCameraRotation(Transform target, RotationInputSchema inputSchema)
        {
            this.target = target;
            input = inputSchema;
        }

        public void Rotate(float deltaTime, float rotationSpeed, float rollSpeed, float damping)
        {
            // Extract the current yaw and pitch
            float currentYaw = target.eulerAngles.y;
            float currentPitch = target.eulerAngles.x;
            float currentRoll = target.eulerAngles.z;

            currentRoll += SmoothedRollRate(deltaTime, rollSpeed, damping) * deltaTime;

            if (mouseControlIsEnabled)
            {
                smoothedMouseDelta = CalculateSmoothedMouseDelta(deltaTime, rotationSpeed, damping);
                currentYaw += smoothedMouseDelta.x * deltaTime;
                currentPitch -= smoothedMouseDelta.y * deltaTime;
            }

            target.rotation = Quaternion.Euler(currentPitch, currentYaw, currentRoll);
        }

        private Vector2 CalculateSmoothedMouseDelta(float deltaTime, float rotationSpeed, float damping)
        {
            currentMouseDelta.x = input.cameraXAxis.GetValue() * rotationSpeed;
            currentMouseDelta.y = input.cameraYAxis.GetValue() * rotationSpeed;

            return Vector2.Lerp(smoothedMouseDelta, currentMouseDelta, deltaTime * damping);
        }

        private float SmoothedRollRate(float deltaTime, float rollSpeed, float damping)
        {
            if (input.cameraRollLeft.isOn)
                currentRollRate = rollSpeed;
            else if (input.cameraRollRight.isOn)
                currentRollRate = -rollSpeed;
            else
                currentRollRate = 0f;

            smoothedRollRate = Mathf.Lerp(smoothedRollRate, currentRollRate, deltaTime * damping);
            return smoothedRollRate;
        }

        public void Activate()
        {
            if (Utils.IsCursorLocked)
                EnableRotation();

            input.mouseFirstClick.OnStarted += EnableRotation;
            input.mouseFirstClick.OnFinished += DisableRotation;
        }

        public void Deactivate()
        {
            DisableRotation();

            input.mouseFirstClick.OnStarted -= EnableRotation;
            input.mouseFirstClick.OnFinished -= DisableRotation;
        }

        private void EnableRotation(DCLAction_Hold action) =>
            EnableRotation();

        private void DisableRotation(DCLAction_Hold action) =>
            DisableRotation();

        private void EnableRotation() =>
            SwitchRotation(isEnabled: true);

        private void DisableRotation()
        {
            SwitchRotation(isEnabled: false);
            smoothedRollRate = 0f;
            Utils.UnlockCursor();
        }

        private void SwitchRotation(bool isEnabled)
        {
            DataStore.i.camera.panning.Set(false);
            mouseControlIsEnabled = isEnabled;
        }
    }
}
