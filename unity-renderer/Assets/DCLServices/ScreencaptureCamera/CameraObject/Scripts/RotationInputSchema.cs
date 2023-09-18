using System;

namespace DCLFeatures.ScreencaptureCamera.CameraObject
{
    [Serializable]
    public struct RotationInputSchema
    {
        public readonly InputAction_Measurable CameraXAxis;
        public readonly InputAction_Measurable CameraYAxis;
        public readonly InputAction_Hold MouseFirstClick;

        public RotationInputSchema(InputAction_Measurable cameraXAxis, InputAction_Measurable cameraYAxis, InputAction_Hold mouseFirstClick)
        {
            this.CameraXAxis = cameraXAxis;
            this.CameraYAxis = cameraYAxis;
            this.MouseFirstClick = mouseFirstClick;
        }
    }
}
