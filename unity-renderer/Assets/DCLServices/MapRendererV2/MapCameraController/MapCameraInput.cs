using DCLServices.MapRendererV2.MapLayers;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapCameraController
{
    public readonly struct MapCameraInput
    {
        public readonly MapLayer EnabledLayers;
        public readonly Vector2Int Position;
        public readonly float Zoom;

        public MapCameraInput(MapLayer enabledLayers, Vector2Int position, float zoom)
        {
            EnabledLayers = enabledLayers;
            Position = position;
            Zoom = zoom;
        }
    }
}
