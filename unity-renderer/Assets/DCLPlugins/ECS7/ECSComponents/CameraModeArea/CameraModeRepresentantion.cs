using DCL.Components;

namespace DCL.ECSComponents
{
    public class CameraModeRepresentantion : ICameraModeArea
    {
        public CameraMode.ModeId cameraMode { get; private set; }
        
        public void SetCameraMode(CameraMode.ModeId cameraMode)
        {
            this.cameraMode = cameraMode;
        }
    }
}