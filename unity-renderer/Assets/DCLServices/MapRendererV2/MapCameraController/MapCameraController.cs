using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapLayers;
using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Utils = DCL.Helpers.Utils;

namespace DCLServices.MapRendererV2.MapCameraController
{
    internal partial class MapCameraController : IMapCameraControllerInternal
    {
        private const float CAMERA_HEIGHT = 0;

        private const int MAX_TEXTURE_SIZE = 4096;

        public event Action<IMapCameraControllerInternal> OnReleasing;

        public MapLayer EnabledLayers { get; private set; }

        public Camera Camera => mapCameraObject.mapCamera;

        public float Zoom => Mathf.InverseLerp(zoomRangeInParcels.y, zoomRangeInParcels.x, mapCameraObject.mapCamera.orthographicSize);

        public Vector2 LocalPosition => mapCameraObject.mapCamera.transform.localPosition;

        public Vector2 CoordsPosition => coordsUtils.PositionToCoordsUnclamped(LocalPosition);

        private readonly IMapInteractivityControllerInternal interactivityBehavior;
        private readonly ICoordsUtils coordsUtils;
        private readonly IMapCullingController cullingController;
        private readonly MapCameraObject mapCameraObject;

        private RenderTexture renderTexture;

        // Zoom Thresholds in Parcels
        private Vector2Int zoomRangeInParcels;

        private Rect cameraPositionBounds;
        public float VerticalSizeInLocalUnits => mapCameraObject.mapCamera.orthographicSize * 2;
        public IMapInteractivityController InteractivityController => interactivityBehavior;
        private RenderTextureDescriptor descriptor;

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

        void IMapCameraControllerInternal.Initialize(Vector2Int textureResolution, Vector2Int zoomRange, MapLayer layers)
        {
            textureResolution = ClampTextureResolution(textureResolution);

            descriptor = new RenderTextureDescriptor
            {
                width = textureResolution.x,
                height = textureResolution.y,
                depthBufferBits = 0,
                graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm,
                mipCount = 1,
                memoryless = RenderTextureMemoryless.Depth,
                autoGenerateMips = false,
                useMipMap = false,
                volumeDepth = 1,
                msaaSamples = 1,
                dimension = TextureDimension.Tex2D,
            };

            renderTexture = RenderTexture.GetTemporary(descriptor);

            // Bilinear and Trilinear make texture blurry
            renderTexture.filterMode = FilterMode.Point;

            this.zoomRangeInParcels = zoomRange * coordsUtils.ParcelSize;

            EnabledLayers = layers;

            mapCameraObject.mapCamera.targetTexture = renderTexture;

            cullingController.OnCameraAdded(this);

            interactivityBehavior.Initialize(layers);
        }

        public void ResizeTexture(Vector2Int textureResolution)
        {
            if (!Camera) return;

            if (renderTexture.IsCreated())
            {
                mapCameraObject.mapCamera.targetTexture = null;
                RenderTexture.ReleaseTemporary(renderTexture);
            }

            textureResolution = ClampTextureResolution(textureResolution);
            descriptor.width = textureResolution.x;
            descriptor.height = textureResolution.y;
            renderTexture = RenderTexture.GetTemporary(descriptor);

            mapCameraObject.mapCamera.targetTexture = renderTexture;

            Camera.ResetAspect();

            SetLocalPosition(mapCameraObject.transform.localPosition);
        }

        private static Vector2Int ClampTextureResolution(Vector2Int desiredRes)
        {
            float factor = Mathf.Min(1, MAX_TEXTURE_SIZE / (float) Mathf.Max(desiredRes.x, desiredRes.y));
            return Vector2Int.FloorToInt((Vector2) desiredRes * factor);
        }


        public RenderTexture GetRenderTexture()
        {
            if (renderTexture == null)
                throw new Exception("Trying to get RenderTexture from a not initialized MapCameraController");

            return renderTexture;
        }


        public void SetZoom(float value)
        {
            SetCameraSize(value);
            // Clamp local position as boundaries are dependent on zoom
            SetLocalPositionClamped(mapCameraObject.transform.localPosition);
            cullingController.SetCameraDirty(this);
        }

        public void SetPosition(Vector2 coordinates)
        {
            Vector3 position = coordsUtils.CoordsToPositionUnclamped(coordinates);
            mapCameraObject.transform.localPosition = ClampLocalPosition(new Vector3(position.x, position.y, CAMERA_HEIGHT));
            cullingController.SetCameraDirty(this);
        }

        public void SetLocalPosition(Vector2 localCameraPosition)
        {
            SetLocalPositionClamped(localCameraPosition);
            cullingController.SetCameraDirty(this);
        }

        private void SetLocalPositionClamped(Vector2 localCameraPosition)
        {
            mapCameraObject.transform.localPosition = ClampLocalPosition(localCameraPosition);
        }

        public void SetPositionAndZoom(Vector2 coordinates, float zoomValue)
        {
            SetCameraSize(zoomValue);

            Vector3 position = coordsUtils.CoordsToPositionUnclamped(coordinates);
            mapCameraObject.transform.localPosition = ClampLocalPosition(new Vector3(position.x, position.y, CAMERA_HEIGHT));
            cullingController.SetCameraDirty(this);
        }

        private void SetCameraSize(float zoom)
        {
            zoom = Mathf.Clamp01(zoom);
            mapCameraObject.mapCamera.orthographicSize = Mathf.Lerp(zoomRangeInParcels.y, zoomRangeInParcels.x, zoom);

            CalculateCameraPositionBounds();
        }

        private Vector3 ClampLocalPosition(Vector3 localPos)
        {
            localPos.x = Mathf.Clamp(localPos.x, cameraPositionBounds.xMin, cameraPositionBounds.xMax);
            localPos.y = Mathf.Clamp(localPos.y, cameraPositionBounds.yMin, cameraPositionBounds.yMax);
            return localPos;
        }

        private void CalculateCameraPositionBounds()
        {
            var worldBounds = coordsUtils.VisibleWorldBounds;

            var cameraYSize = mapCameraObject.mapCamera.orthographicSize;
            var cameraXSize = cameraYSize * mapCameraObject.mapCamera.aspect;

            cameraPositionBounds = Rect.MinMaxRect(worldBounds.xMin + cameraXSize, worldBounds.yMin + cameraYSize,
                worldBounds.xMax - cameraXSize, worldBounds.yMax - cameraYSize);
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

        public Rect GetCameraRect()
        {
            var cameraYSize = mapCameraObject.mapCamera.orthographicSize;
            var cameraXSize = cameraYSize * mapCameraObject.mapCamera.aspect;

            var size = new Vector2(cameraXSize * 2f, cameraYSize * 2f);

            return new Rect((Vector2) mapCameraObject.transform.localPosition - new Vector2(cameraXSize, cameraYSize), size);
        }

        public void Release()
        {
            cullingController.OnCameraRemoved(this);

            mapCameraObject.mapCamera.targetTexture = null;
            if (renderTexture != null)
                RenderTexture.ReleaseTemporary(renderTexture);

            interactivityBehavior.Release();
            OnReleasing?.Invoke(this);
        }

        public void Dispose()
        {
            if (mapCameraObject != null)
                Utils.SafeDestroy(mapCameraObject.gameObject);

            interactivityBehavior.Dispose();

            mapCameraObject.mapCamera.targetTexture = null;
            if (renderTexture != null)
                RenderTexture.ReleaseTemporary(renderTexture);
        }
    }
}
