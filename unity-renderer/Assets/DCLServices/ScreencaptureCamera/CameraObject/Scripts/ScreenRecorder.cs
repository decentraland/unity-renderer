using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace DCLFeatures.ScreencaptureCamera.CameraObject
{
    public class ScreenRecorder
    {
        private const int TARGET_FRAME_WIDTH = 1920;
        private const int TARGET_FRAME_HEIGHT = 1080;
        private const float FRAME_SCALE = 0.87f; // Defines the scale of the frame in relation to the screen

        private readonly float targetAspectRatio;
        private readonly RectTransform canvasRectTransform;

        private readonly Texture2D screenshot = new (TARGET_FRAME_WIDTH, TARGET_FRAME_HEIGHT, TextureFormat.RGB24, false);
        private RenderTexture originalBaseTargetTexture;

        public ScreenRecorder(RectTransform canvasRectTransform)
        {
            targetAspectRatio = (float)TARGET_FRAME_WIDTH / TARGET_FRAME_HEIGHT;
            Debug.Assert(targetAspectRatio != 0, "Target aspect ratio cannot be null");

            this.canvasRectTransform = canvasRectTransform;
        }

        public virtual IEnumerator CaptureScreenshot(Camera baseCamera, Action<Texture2D> onComplete)
        {
            yield return new WaitForEndOfFrame(); // for UI to appear on screenshot. Converting to UniTask didn't work :(

            ScreenFrameData targetScreenFrame = CalculateTargetScreenFrame(CalculateCurrentScreenFrame());

            var initialRenderTexture = RenderTexture.GetTemporary(targetScreenFrame.ScreenWidthInt, targetScreenFrame.ScreenHeightInt, 0, GraphicsFormat.R32G32B32A32_SFloat);
            ScreenCapture.CaptureScreenshotIntoRenderTexture(initialRenderTexture);

            var finalRenderTexture = RenderTexture.GetTemporary(targetScreenFrame.ScreenWidthInt, targetScreenFrame.ScreenHeightInt, 0);
            Graphics.Blit(initialRenderTexture, finalRenderTexture); // we need to Blit to have HDR included on crop

            CropCentralFrameToScreenshotTexture(finalRenderTexture, targetScreenFrame);

            RenderTexture.ReleaseTemporary(initialRenderTexture);
            RenderTexture.ReleaseTemporary(finalRenderTexture);

            onComplete?.Invoke(FlipTextureVertically(screenshot));
        }

        private ScreenFrameData CalculateCurrentScreenFrame()
        {
            var screenFrameData = new ScreenFrameData
            {
                ScreenWidth = canvasRectTransform.rect.width * canvasRectTransform.lossyScale.x,
                ScreenHeight = canvasRectTransform.rect.height * canvasRectTransform.lossyScale.y,
            };

            // Adjust current by smallest side
            if (screenFrameData.ScreenAspectRatio > targetAspectRatio) // Height is the limiting dimension, so scaling width based on it
            {
                screenFrameData.FrameHeight = screenFrameData.ScreenHeight * FRAME_SCALE;
                screenFrameData.FrameWidth = screenFrameData.FrameHeight * targetAspectRatio;
            }
            else // Width is the limiting dimension, so scaling height based on it
            {
                screenFrameData.FrameWidth = screenFrameData.ScreenWidth * FRAME_SCALE;
                screenFrameData.FrameHeight = screenFrameData.FrameWidth / targetAspectRatio;
            }

            return screenFrameData;
        }

        private void CropCentralFrameToScreenshotTexture(RenderTexture finalRenderTexture, ScreenFrameData targetScreenFrame)
        {
            RenderTexture.active = finalRenderTexture;
            Vector2Int corners = targetScreenFrame.CalculateFrameCorners();

            Debug.Assert(corners.x + targetScreenFrame.FrameWidthInt <= finalRenderTexture.width, "Texture width is smaller than needed for target screenshot resolution");
            Debug.Assert(corners.y + targetScreenFrame.FrameHeightInt <= finalRenderTexture.height, "Texture height is smaller than needed for target screenshot resolution");

            screenshot.ReadPixels(new Rect(corners.x, corners.y, targetScreenFrame.FrameWidthInt, targetScreenFrame.FrameHeightInt), 0, 0);
            screenshot.Apply(updateMipmaps: false);
            RenderTexture.active = null;
        }

        private static Texture2D FlipTextureVertically(Texture2D original)
        {
            var flipped = new Texture2D(original.width, original.height);
            int xN = original.width;
            int yN = original.height;

            for (var i = 0; i < xN; i++)
            for (var j = 0; j < yN; j++)
                flipped.SetPixel(i, yN - j - 1, original.GetPixel(i, j));

            flipped.Apply(updateMipmaps: false);
            return flipped;
        }

        private static ScreenFrameData CalculateTargetScreenFrame(ScreenFrameData currentScreenFrameData)
        {
            var screenFrameData = new ScreenFrameData();

            float upscaleFrameWidth = TARGET_FRAME_WIDTH / currentScreenFrameData.FrameWidth;
            float upscaleFrameHeight = TARGET_FRAME_HEIGHT / currentScreenFrameData.FrameHeight;
            Debug.Assert(Math.Abs(upscaleFrameWidth - upscaleFrameHeight) < 0.01f, "Screenshot upscale factors should be the same");

            screenFrameData.ScreenWidth = currentScreenFrameData.ScreenWidth * upscaleFrameWidth;
            screenFrameData.ScreenHeight = currentScreenFrameData.ScreenHeight * upscaleFrameWidth;
            screenFrameData.FrameWidth = currentScreenFrameData.FrameWidth * upscaleFrameWidth;
            screenFrameData.FrameHeight = currentScreenFrameData.FrameHeight * upscaleFrameWidth;
            Debug.Assert(Math.Abs(screenFrameData.FrameWidth - TARGET_FRAME_WIDTH) < 0.1f, "Calculated screenshot width should be the same as target width");
            Debug.Assert(Math.Abs(screenFrameData.FrameHeight - TARGET_FRAME_HEIGHT) < 0.1f, "Calculated screenshot height should be the same as target height");

            return screenFrameData;
        }
    }
}
