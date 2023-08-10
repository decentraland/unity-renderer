using Cinemachine.Utility;
using DCL;
using DCL.Camera;
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
        private InputSpikeFixer[] inputSpikeFixer;

        public ScreencaptureCameraRotation(Transform transform, RotationInputSchema inputSchema)
        {
            this.transform = transform;
            input = inputSchema;

            inputSpikeFixer = new []
            {
                new InputSpikeFixer(() => Utils.IsCursorLocked ? CursorLockMode.Locked : CursorLockMode.None),
                new InputSpikeFixer(() => Utils.IsCursorLocked ? CursorLockMode.Locked : CursorLockMode.None)
            };
        }
        private Vector3 axis = new Vector3();
        private Vector3 axisTarget = new Vector3();
        public void Rotate(Transform target, float deltaTime, float rotationSpeed, float rollSpeed, float dampTime, float maxRotationPerFrame)
        {
            // Extract the current yaw and pitch
            float currentYaw = target.eulerAngles.y;
            float currentPitch = target.eulerAngles.x;
            float currentRoll = target.eulerAngles.z;

            if (mouseControlIsEnabled)
            {
                axisTarget[0] = input.cameraXAxis.GetValue();
                axisTarget[1] = input.cameraYAxis.GetValue();
                axis += Damper.Damp(axisTarget - axis, dampTime, Time.deltaTime);

                currentYaw += Mathf.Clamp(inputSpikeFixer[0].GetValue(this.axis[0]) * rotationSpeed * deltaTime, -maxRotationPerFrame, maxRotationPerFrame);
                currentPitch -= Mathf.Clamp(inputSpikeFixer[1].GetValue(this.axis[1])  * rotationSpeed * deltaTime, -maxRotationPerFrame, maxRotationPerFrame);
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
