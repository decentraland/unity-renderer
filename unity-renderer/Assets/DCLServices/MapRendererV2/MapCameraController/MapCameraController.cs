using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapLayers;
using System;
using UnityEngine;
using Utils = DCL.Helpers.Utils;

namespace DCLServices.MapRendererV2.MapCameraController
{
    internal partial class MapCameraController : IMapCameraControllerInternal
    {
        private const float CAMERA_HEIGHT = 0;

        private const int MAX_TEXTURE_SIZE = 2048;

        public event Action<IMapCameraControllerInternal> OnReleasing;

        public MapLayer EnabledLayers { get; private set; }

        public Camera Camera => mapCameraObject.mapCamera;

        public float Zoom => Mathf.InverseLerp(zoomValues.y, zoomValues.x, mapCameraObject.mapCamera.orthographicSize);

        public Vector2 Position => mapCameraObject.mapCamera.transform.localPosition;

        private readonly IMapInteractivityControllerInternal interactivityBehavior;
        private readonly ICoordsUtils coordsUtils;
        private readonly IMapCullingController cullingController;
        private readonly MapCameraObject mapCameraObject;

        private RenderTexture renderTexture;

        // Zoom Thresholds in Parcels
        private Vector2Int zoomValues;

        public MapCameraController(
            IMapInteractivityControllerInternal interactivityBehavior,
            MapCameraObject mapCameraObject,
            ICoordsUtils coordsUtils,
            IMapCullingController cullingController
        )
        {
            this.interactivityBehavior = interactivityBehavior;
            this.coordsUtils = coordsUtils;
            this.mapCameraObject = mapCameraObject;
            this.cullingController = cullingController;

            mapCameraObject.transform.localPosition = Vector3.up * CAMERA_HEIGHT;
            mapCameraObject.mapCamera.orthographic = true;
        }

        void IMapCameraControllerInternal.Initialize(Vector2Int textureResolution, Vector2Int zoomValues, MapLayer layers)
        {
            textureResolution = ClampTextureResolution(textureResolution);
            renderTexture = new RenderTexture(textureResolution.x, textureResolution.y, 0, RenderTextureFormat.Default, 0);
            // Bilinear and Trilinear make texture blurry
            renderTexture.filterMode = FilterMode.Point;
            renderTexture.autoGenerateMips = false;
            renderTexture.useMipMap = false;

            this.zoomValues = zoomValues * coordsUtils.ParcelSize;
            EnabledLayers = layers;

            mapCameraObject.mapCamera.targetTexture = renderTexture;

            cullingController.OnCameraAdded(this);

            interactivityBehavior.Initialize(layers);
        }

        public void ResizeTexture(Vector2Int textureResolution)
        {
            if (!Camera) return;

            if (renderTexture.IsCreated())
                renderTexture.Release();

            textureResolution = ClampTextureResolution(textureResolution);
            renderTexture.width = textureResolution.x;
            renderTexture.height = textureResolution.y;
            renderTexture.Create();

            Camera.ResetAspect();
        }

        private Vector2Int ClampTextureResolution(Vector2Int desiredRes)
        {
            float factor = Mathf.Min(1, MAX_TEXTURE_SIZE / (float) Mathf.Max(desiredRes.x, desiredRes.y));
            return Vector2Int.FloorToInt((Vector2) desiredRes * factor);
        }

        public float GetVerticalSizeInLocalUnits() =>
            mapCameraObject.mapCamera.orthographicSize * 2;

        public RenderTexture GetRenderTexture()
        {
            if (renderTexture == null)
                throw new Exception("Trying to get RenderTexture from a not initialized MapCameraController");

            return renderTexture;
        }

        public IMapInteractivityController GetInteractivityController() =>
            interactivityBehavior;

        public void SetZoom(float value)
        {
            value = Mathf.Clamp01(value);
            mapCameraObject.mapCamera.orthographicSize = Mathf.Lerp(zoomValues.y, zoomValues.x, value);
            cullingController.SetCameraDirty(this);
        }

        public void SetPosition(Vector2 coordinates)
        {
            Vector3 position = coordsUtils.CoordsToPositionUnclamped(coordinates);
            mapCameraObject.transform.localPosition = new Vector3(position.x, position.y, CAMERA_HEIGHT);
            cullingController.SetCameraDirty(this);
        }

        public void SetLocalPosition(Vector2 localCameraPosition)
        {
            var worldBounds = coordsUtils.WorldBounds;

            var cameraPos = new Vector3(Mathf.Clamp(localCameraPosition.x, worldBounds.xMin, worldBounds.xMax),
                Mathf.Clamp(localCameraPosition.y, worldBounds.yMin, worldBounds.yMax), CAMERA_HEIGHT);

            mapCameraObject.transform.localPosition = cameraPos;
            cullingController.SetCameraDirty(this);
        }

        public void SetPositionAndZoom(Vector2 coordinates, float value)
        {
            value = Mathf.Clamp01(value);
            mapCameraObject.mapCamera.orthographicSize = Mathf.Lerp(zoomValues.y, zoomValues.x, value);

            Vector3 position = coordsUtils.CoordsToPositionUnclamped(coordinates);
            mapCameraObject.transform.localPosition = new Vector3(position.x, position.y, CAMERA_HEIGHT);
            cullingController.SetCameraDirty(this);
        }

        public void SuspendRendering()
        {
            mapCameraObject.mapCamera.enabled = false;
        }

        public void ResumeRendering()
        {
            mapCameraObject.mapCamera.enabled = true;
        }

        public void SetActive(bool active)
        {
            mapCameraObject.gameObject.SetActive(active);
        }

        public void GetFrustumPlanes(Plane[] planes)
        {
            GeometryUtility.CalculateFrustumPlanes(Camera, planes);
        }

        public void Release()
        {
            cullingController.OnCameraRemoved(this);
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
