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

        private readonly RotationInputSchema input;
        private readonly InputSpikeFixer[] inputSpikeFixer;

        private bool mouseControlIsEnabled;

        private Vector3 axis;
        private Vector3 axisTarget;

        public ScreencaptureCameraRotation(RotationInputSchema inputSchema)
        {
            input = inputSchema;

            inputSpikeFixer = new[]
            {
                new InputSpikeFixer(() => Utils.IsCursorLocked ? CursorLockMode.Locked : CursorLockMode.None),
                new InputSpikeFixer(() => Utils.IsCursorLocked ? CursorLockMode.Locked : CursorLockMode.None),
            };
        }

        public void Rotate(Transform target, float deltaTime, float rotationSpeed, float dampTime, float maxRotationPerFrame)
        {
            if (!mouseControlIsEnabled) return;

            Vector3 currentAngles = target.eulerAngles;

            axisTarget[0] = input.CameraXAxis.GetValue();
            axisTarget[1] = input.CameraYAxis.GetValue();
            axis += Damper.Damp(axisTarget - axis, dampTime, deltaTime);

            currentAngles.y += Mathf.Clamp(inputSpikeFixer[0].GetValue(axis[0]) * rotationSpeed * deltaTime, -maxRotationPerFrame, maxRotationPerFrame); // Yaw
            currentAngles.x -= Mathf.Clamp(inputSpikeFixer[1].GetValue(axis[1]) * rotationSpeed * deltaTime, -maxRotationPerFrame, maxRotationPerFrame); // Pitch

            target.rotation = Quaternion.Euler(currentAngles);
        }

        public void Activate()
        {
            if (Utils.IsCursorLocked)
                EnableRotation(DUMMY_ACTION);

            input.MouseFirstClick.OnStarted += EnableRotation;
            input.MouseFirstClick.OnFinished += DisableRotation;
        }

        public void Deactivate()
        {
            DisableRotation(DUMMY_ACTION);

            input.MouseFirstClick.OnStarted -= EnableRotation;
            input.MouseFirstClick.OnFinished -= DisableRotation;
        }

        private void EnableRotation(DCLAction_Hold _) =>
            SwitchRotation(isEnabled: true);

        private void DisableRotation(DCLAction_Hold _)
        {
            SwitchRotation(isEnabled: false);
            Utils.UnlockCursor();
        }

        private void SwitchRotation(bool isEnabled)
        {
            DataStore.i.camera.panning.Set(false);
            mouseControlIsEnabled = isEnabled;
        }
    }
}
