using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.MapLayers;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCLServices.MapRendererV2.MapCameraController
{
    internal class MapCameraController : IMapCameraControllerInternal
    {
        private static readonly Vector2 ZOOM_CAMERA_SIZE = new (150, 400); // TODO Evaluate if we need to expose this in the constructor
        private const float CAMERA_HEIGHT = 10;

        public Action<IMapCameraController> OnDisposed { get; set; }
        public MapLayer EnabledLayers { get; }

        private readonly RenderTexture renderTexture;
        private readonly ICoordsUtils coordsUtils;
        private readonly MapCameraObject mapCameraObject;

        public MapCameraController(
            int width,
            int height,
            MapCameraObject mapCameraObject,
            MapLayer enabledLayers,
            ICoordsUtils coordsUtils
        ) : this(new RenderTexture(width, height, 0), mapCameraObject, enabledLayers, coordsUtils) { }

        public MapCameraController(
            RenderTexture renderTexture,
            MapCameraObject mapCameraObject,
            MapLayer enabledLayers,
            ICoordsUtils coordsUtils
        )
        {
            this.renderTexture = renderTexture;
            this.EnabledLayers = enabledLayers;
            this.coordsUtils = coordsUtils;

            mapCameraObject.transform.localPosition = Vector3.up * CAMERA_HEIGHT;
            mapCameraObject.mapCamera.orthographic = true;
        }

        public RenderTexture GetRenderTexture() =>
            renderTexture;

        public void SetZoom(float value)
        {
            value = Mathf.Clamp01(value);
            mapCameraObject.mapCamera.orthographicSize = Mathf.Lerp(ZOOM_CAMERA_SIZE.x, ZOOM_CAMERA_SIZE.y, value);
        }

        public void SetPosition(Vector2 coordinates)
        {
            Vector3 position = coordsUtils.CoordsToPositionUnclamped(coordinates);
            mapCameraObject.transform.position = new Vector3(position.x, CAMERA_HEIGHT, position.y);
        }

        public void SetActive(bool active)
        {
            mapCameraObject.gameObject.SetActive(active);
        }

        public void Dispose()
        {
            Object.Destroy(mapCameraObject);
            OnDisposed?.Invoke(this);
        }
    }
}
