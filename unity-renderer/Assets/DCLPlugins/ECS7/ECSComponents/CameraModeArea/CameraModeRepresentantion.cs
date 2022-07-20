using DCL.Components;

namespace DCL.ECSComponents
{
    public class CameraModeRepresentantion : ICameraModeArea
    {
        public DCL.Camera.CameraMode.ModeId cameraMode { get; private set; }
        
        public void SetCameraMode(DCL.Camera.CameraMode.ModeId cameraMode)
        {
            this.cameraMode = cameraMode;
        }
    }
}