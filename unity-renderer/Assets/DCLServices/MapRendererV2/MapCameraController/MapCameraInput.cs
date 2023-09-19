using DCLServices.MapRendererV2.MapLayers;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapCameraController
{
    public readonly struct MapCameraInput
    {
        public readonly MapLayer EnabledLayers;
        public readonly Vector2Int TextureResolution;
        public readonly Vector2Int ZoomRange;

        /// <param name="enabledLayers">active layers</param>
        /// <param name="position">default position</param>
        /// <param name="zoom">default zoom</param>
        /// <param name="textureResolution">desired texture resolution</param>
        /// <param name="zoomRange">zoom thresholds in parcels</param>
        public MapCameraInput(MapLayer enabledLayers, Vector2Int textureResolution, Vector2Int zoomRange)
        {
            EnabledLayers = enabledLayers;
            TextureResolution = textureResolution;
            ZoomRange = zoomRange;
        }
    }
}
