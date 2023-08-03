using System;

namespace DCLFeatures.ScreencaptureCamera
{
    [Serializable]
    public class ScreencaptureCameraInputSchema
    {
        public InputAction_Trigger ToggleScreenshotCameraAction;
        public InputAction_Trigger ToggleCameraReelAction;
        public InputAction_Trigger TakeScreenshotAction;
        public InputAction_Trigger CloseWindowAction;
    }
}
