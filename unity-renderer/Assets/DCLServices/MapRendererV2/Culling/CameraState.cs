using DCLServices.MapRendererV2.MapCameraController;
using UnityEngine;

namespace DCLServices.MapRendererV2.Culling
{
    internal class CameraState
    {
        public IMapCameraControllerInternal CameraController;
        public Rect Rect;
    }
}
