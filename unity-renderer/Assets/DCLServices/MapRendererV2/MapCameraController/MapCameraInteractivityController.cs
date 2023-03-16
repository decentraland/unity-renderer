using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.MapLayers;
using DCLServices.MapRendererV2.MapLayers.ParcelHighlight;
using MainScripts.DCL.Helpers.Utils;
using UnityEngine;
using UnityEngine.Pool;

namespace DCLServices.MapRendererV2.MapCameraController
{
    internal class MapCameraInteractivityController : IMapInteractivityControllerInternal
    {
        private readonly Transform cameraParent;
        private readonly IObjectPool<IParcelHighlightMarker> markersPool;
        private readonly ICoordsUtils coordsUtils;
        private readonly Camera camera;

        private IParcelHighlightMarker marker;

        public bool HighlightEnabled { get; private set; }

        public MapCameraInteractivityController(
            Transform cameraParent,
            Camera camera,
            IObjectPool<IParcelHighlightMarker> markersPool,
            ICoordsUtils coordsUtils)
        {
            this.cameraParent = cameraParent;
            this.markersPool = markersPool;
            this.coordsUtils = coordsUtils;
            this.camera = camera;
        }

        public void HighlightParcel(Vector2 normalizedCoordinates)
        {
            if (marker == null)
                return;

            var localPosition = GetLocalPosition(normalizedCoordinates);
            var mapCoords = coordsUtils.PositionToCoords(localPosition);

            marker.Activate();
            marker.SetCoordinates(mapCoords, localPosition);
        }

        public void Initialize(MapLayer layers)
        {
            HighlightEnabled = EnumUtils.HasFlag(layers, MapLayer.ParcelHoverHighlight);

            if (HighlightEnabled)
                marker = markersPool.Get();
        }

        public void RemoveHighlight()
        {
            if (marker == null)
                return;

            marker.Deactivate();
        }

        public Vector2Int GetParcel(Vector2 normalizedCoordinates) =>
            coordsUtils.PositionToCoords(GetLocalPosition(normalizedCoordinates));

        private Vector3 GetLocalPosition(Vector2 normalizedCoordinates)
        {
            // normalized position is equal to viewport position
            var worldPoint = camera.ViewportToWorldPoint(normalizedCoordinates);

            var localPosition = cameraParent ? cameraParent.InverseTransformPoint(worldPoint) : worldPoint;
            localPosition.z = 0;
            return localPosition;
        }

        public void Dispose()
        {
            Release();
        }

        public void Release()
        {
            if (marker != null)
            {
                markersPool.Release(marker);
                marker = null;
            }
        }
    }
}
