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
            ScreenFrameData targetScreenFrame = CalculateTargetScreenFrame(currentScreenFrameData: CalculateCurrentScreenFrame());

            var initialRenderTexture = RenderTexture.GetTemporary(targetScreenFrame.ScreenWidthInt, targetScreenFrame.ScreenHeightInt, 24, GraphicsFormat.R32G32B32A32_SFloat, 8);

            originalBaseTargetTexture = baseCamera.targetTexture;
            baseCamera.targetTexture = initialRenderTexture;

            yield return new WaitForEndOfFrame(); // for UI to appear on screenshot. Converting to UniTask didn't work :(
            baseCamera.Render();

            baseCamera.targetTexture = originalBaseTargetTexture;

            var finalRenderTexture = RenderTexture.GetTemporary(targetScreenFrame.ScreenWidthInt, targetScreenFrame.ScreenHeightInt, 24);
            Graphics.Blit(initialRenderTexture, finalRenderTexture); // we need to Blit to have HDR included on crop

            CropToScreenshotFrame(finalRenderTexture, targetScreenFrame);

            RenderTexture.ReleaseTemporary(initialRenderTexture);
            RenderTexture.ReleaseTemporary(finalRenderTexture);

            onComplete?.Invoke(screenshot);
        }

        private void CropToScreenshotFrame(RenderTexture finalRenderTexture, ScreenFrameData targetScreenFrame)
        {
            RenderTexture.active = finalRenderTexture;
            Vector2Int corners = targetScreenFrame.CalculateFrameCorners();
            screenshot.ReadPixels(new Rect(corners.x, corners.y, TARGET_FRAME_WIDTH, TARGET_FRAME_HEIGHT), 0, 0);
            screenshot.Apply();
            RenderTexture.active = null;
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

        private static ScreenFrameData CalculateTargetScreenFrame(ScreenFrameData currentScreenFrameData)
        {
            var screenFrameData = new ScreenFrameData();

            float upscaleFrameWidth = TARGET_FRAME_WIDTH / currentScreenFrameData.FrameWidth;
            float upscaleFrameHeight = TARGET_FRAME_HEIGHT / currentScreenFrameData.FrameHeight;
            Debug.Assert(Math.Abs(upscaleFrameWidth - upscaleFrameHeight) < 0.01f);

            float targetRescale = upscaleFrameWidth;

            screenFrameData.ScreenWidth = currentScreenFrameData.ScreenWidth * targetRescale;
            screenFrameData.ScreenHeight = currentScreenFrameData.ScreenHeight * targetRescale;
            screenFrameData.FrameWidth = currentScreenFrameData.FrameWidth * targetRescale;
            screenFrameData.FrameHeight = currentScreenFrameData.FrameHeight * targetRescale;
            Debug.Assert(Math.Abs(screenFrameData.FrameWidth - TARGET_FRAME_WIDTH) < 0.1f);
            Debug.Assert(Math.Abs(screenFrameData.FrameHeight - TARGET_FRAME_HEIGHT) < 0.1f);

            return screenFrameData;
        }
    }
}
