using DCL.Components;

namespace DCL.ECSComponents
{
    public class CameraModeRepresentantion : ICameraModeArea
    {
        public CameraTool.CameraMode.ModeId cameraMode { get; private set; }
        
        public void SetCameraMode(CameraTool.CameraMode.ModeId cameraMode)
        {
            this.cameraMode = cameraMode;
        }
    }
}