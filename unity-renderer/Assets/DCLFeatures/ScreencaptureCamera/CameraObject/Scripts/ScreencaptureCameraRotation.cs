using DCL;
using DCL.Helpers;
using UnityEngine;

namespace DCLFeatures.ScreencaptureCamera.CameraObject
{
    public class ScreencaptureCameraRotation
    {
        private const DCLAction_Hold DUMMY_ACTION = new ();

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

        public void Rotate(float deltaTime, float rotationSpeed, float rollSpeed, float damping, float maxRotationPerFrame)
        {
            Vector3 currentEulerAngles = target.eulerAngles;

            // currentEulerAngles.z += Mathf.Clamp(SmoothedRollRate(deltaTime, rollSpeed, damping) * deltaTime, -maxRotationPerFrame, maxRotationPerFrame);

            if (mouseControlIsEnabled)
            {
                smoothedMouseDelta = CalculateSmoothedMouseDelta(deltaTime, rotationSpeed, damping);
                currentEulerAngles.y += Mathf.Clamp(smoothedMouseDelta.x * deltaTime, -maxRotationPerFrame, maxRotationPerFrame); // Yaw
                currentEulerAngles.x -= Mathf.Clamp(smoothedMouseDelta.y * deltaTime, -maxRotationPerFrame, maxRotationPerFrame); // Pitch
            }

            target.rotation = Quaternion.Euler(currentEulerAngles);
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
                EnableRotation(DUMMY_ACTION);

            input.mouseFirstClick.OnStarted += EnableRotation;
            input.mouseFirstClick.OnFinished += DisableRotation;
        }

        public void Deactivate()
        {
            DisableRotation(DUMMY_ACTION);

            input.mouseFirstClick.OnStarted -= EnableRotation;
            input.mouseFirstClick.OnFinished -= DisableRotation;
        }

        private void EnableRotation(DCLAction_Hold _) =>
            SwitchRotation(isEnabled: true);

        private void DisableRotation(DCLAction_Hold _)
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
