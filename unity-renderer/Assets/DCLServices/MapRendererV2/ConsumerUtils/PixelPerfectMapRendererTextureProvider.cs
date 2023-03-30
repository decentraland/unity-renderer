using DCLServices.MapRendererV2.MapCameraController;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCLServices.MapRendererV2.ConsumerUtils
{
    [RequireComponent(typeof(RawImage))]
    [RequireComponent(typeof(RectTransform))]
    public class PixelPerfectMapRendererTextureProvider : MonoBehaviour
    {
        [NonSerialized]
        private RectTransform cachedRectTransform;
        private RectTransform rectTransform => cachedRectTransform ??= (RectTransform)transform;

        private RawImage rawImage;
        private RawImage targetImage => rawImage ??= GetComponent<RawImage>();

        private IMapCameraController cameraController;
        private Camera hudCamera;

        private static Vector3[] worldCorners = new Vector3[4];

        public void Activate(IMapCameraController cameraController)
        {
            this.cameraController = cameraController;
        }

        public void SetHudCamera(Camera hudCamera)
        {
            this.hudCamera = hudCamera;
        }

        public void Deactivate()
        {
            cameraController = null;
        }

        public Vector2Int GetPixelPerfectTextureResolution()
        {
            // assumes CanvasScale Match Height = 1;

            var rectSize = rectTransform.rect.size;
            var ratio = rectSize.x / rectSize.y;

            // translate rect to screen space
            rectTransform.GetWorldCorners(worldCorners);

            Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(hudCamera, worldCorners[0]);
            Vector2 topRight = RectTransformUtility.WorldToScreenPoint(hudCamera, worldCorners[2]);

            var screenSize = topRight - bottomLeft;
            return new Vector2Int((int) screenSize.x, (int) screenSize.y);
        }

        private void OnRectTransformDimensionsChange()
        {
            if (cameraController == null)
                return;

            cameraController.ResizeTexture(GetPixelPerfectTextureResolution());

            targetImage.SetAllDirty();
        }
    }
}
