using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.MapLayers;
using System;
using UnityEngine;
using Object = UnityEngine.Object;
using Utils = DCL.Helpers.Utils;

namespace DCLServices.MapRendererV2.MapCameraController
{
    internal partial class MapCameraController : IMapCameraControllerInternal
    {
        private const float CAMERA_HEIGHT = 10;
        public event Action<IMapCameraControllerInternal> OnReleasing;

        public MapLayer EnabledLayers { get; private set; }
        public Camera Camera => mapCameraObject.mapCamera;

        private readonly IMapInteractivityControllerInternal interactivityBehavior;
        private readonly ICoordsUtils coordsUtils;
        private readonly MapCameraObject mapCameraObject;

        private RenderTexture renderTexture;
        private Vector2Int zoomValues;

        public MapCameraController(
            IMapInteractivityControllerInternal interactivityBehavior,
            MapCameraObject mapCameraObject,
            ICoordsUtils coordsUtils
        )
        {
            this.interactivityBehavior = interactivityBehavior;
            this.coordsUtils = coordsUtils;
            this.mapCameraObject = mapCameraObject;

            mapCameraObject.transform.localPosition = Vector3.up * CAMERA_HEIGHT;
            mapCameraObject.mapCamera.orthographic = true;
        }

        void IMapCameraControllerInternal.Initialize(Vector2Int textureResolution, Vector2Int zoomValues, MapLayer layers)
        {
            renderTexture = new RenderTexture(textureResolution.x, textureResolution.x, 0);
            this.zoomValues = zoomValues;
            EnabledLayers = layers;

            interactivityBehavior.Initialize(layers);
        }

        public RenderTexture GetRenderTexture()
        {
            if (renderTexture == null)
                throw new Exception("Trying to get RenderTexture from a not initialized MapCameraController");

            return renderTexture;
        }

        public IMapInteractivityController GetInteractivityController() => interactivityBehavior;

        public void SetZoom(float value)
        {
            value = Mathf.Clamp01(value);
            mapCameraObject.mapCamera.orthographicSize = Mathf.Lerp(zoomValues.x, zoomValues.y, value);
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

        public void Release()
        {
            renderTexture?.Release();
            interactivityBehavior.Release();
            OnReleasing?.Invoke(this);
        }

        public void Dispose()
        {
            if (mapCameraObject != null)
                Utils.SafeDestroy(mapCameraObject.gameObject);

            interactivityBehavior.Dispose();

            renderTexture?.Release();
            renderTexture = null;
        }
    }
}
