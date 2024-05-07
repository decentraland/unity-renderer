using DCLServices.MapRendererV2.CoordsUtils;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapCameraController
{
    internal partial class MapCameraController
    {
        internal float CAMERA_HEIGHT_EXPOSED => CAMERA_HEIGHT;

        internal ICoordsUtils CoordUtils => coordsUtils;
        internal MapCameraObject MapCameraObject => mapCameraObject;
        internal RenderTexture RenderTexture => renderTexture;
        internal Vector2Int ZoomValues => zoomValues;
    }
}
