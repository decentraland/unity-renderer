using DCL;
using DCL.Helpers;
using UnityEngine;

namespace DCLFeatures.ScreencaptureCamera.CameraObject
{
    public class ScreencaptureCameraRotation
    {
        private const DCLAction_Hold DUMMY_ACTION = new ();

        private readonly Transform transform;
        private readonly RotationInputSchema input;

        private bool mouseControlIsEnabled;

        private Vector2 currentMouseDelta;
        private Vector2 smoothedMouseDelta;

        private float currentRollRate;
        private float smoothedRollRate;

        public ScreencaptureCameraRotation(Transform transform, RotationInputSchema inputSchema)
        {
            this.transform = transform;
            input = inputSchema;
        }

        public void Rotate(Transform target, float deltaTime, float rotationSpeed, float rollSpeed, float damping, float maxRotationPerFrame)
        {
            // Extract the current yaw and pitch
            float currentYaw = target.eulerAngles.y;
            float currentPitch = target.eulerAngles.x;
            float currentRoll = target.eulerAngles.z;

            if (mouseControlIsEnabled)
            {
                currentYaw += input.cameraXAxis.GetValue() * rotationSpeed * deltaTime;
                currentPitch -= input.cameraYAxis.GetValue() * rotationSpeed * deltaTime;
            }

            target.rotation = Quaternion.Euler(currentPitch, currentYaw, currentRoll);
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
