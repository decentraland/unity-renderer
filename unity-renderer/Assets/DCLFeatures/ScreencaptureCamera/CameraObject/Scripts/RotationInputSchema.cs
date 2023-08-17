using System;

namespace DCLFeatures.ScreencaptureCamera.CameraObject
{
    [Serializable]
    public struct RotationInputSchema
    {
        public InputAction_Measurable cameraXAxis;
        public InputAction_Measurable cameraYAxis;
        public InputAction_Hold mouseFirstClick;
        public InputAction_Hold cameraRollLeft;
        public InputAction_Hold cameraRollRight;
    }
}
