using System;
using System.Collections;
using UnityEngine;

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
            ScreenFrameData currentScreenFrame = CalculateCurrentScreenFrame();
            (ScreenFrameData targetScreenFrame, float targetRescale) = CalculateTargetScreenFrame(currentScreenFrame);
            int roundedUpscale = Mathf.CeilToInt(targetRescale);
            ScreenFrameData rescaledScreenFrame = currentScreenFrame * roundedUpscale;
            yield return new WaitForEndOfFrame(); // for UI to appear on screenshot. Converting to UniTask didn't work :(

            Texture2D screenshotTexture = ScreenCapture.CaptureScreenshotAsTexture(roundedUpscale); // upscaled Screen Frame resolution

            // Crop Frame
            Vector2Int startCorner = rescaledScreenFrame.CalculateFrameCorners();
            Color[] pixels = screenshotTexture.GetPixels(startCorner.x, startCorner.y, rescaledScreenFrame.FrameWidthInt, rescaledScreenFrame.FrameHeightInt);
            Texture2D upscaledFrameTexture = new Texture2D(rescaledScreenFrame.FrameWidthInt, rescaledScreenFrame.FrameHeightInt, TextureFormat.RGB24, false);
            upscaledFrameTexture.SetPixels(pixels);
            upscaledFrameTexture.Apply();

            // Resize Frame
            var rt = new RenderTexture(TARGET_FRAME_WIDTH, TARGET_FRAME_HEIGHT, 24);
            RenderTexture.active = rt;

            // Copy and scale the original texture into the RenderTexture
            Graphics.Blit(upscaledFrameTexture, rt);

            // Read the pixel data from the RenderTexture into the Texture2D
            screenshot.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            screenshot.Apply();

            RenderTexture.active = null;
            rt.Release();

            // var initialRenderTexture = RenderTexture.GetTemporary(targetScreenFrame.ScreenWidthInt, targetScreenFrame.ScreenHeightInt, 24, GraphicsFormat.R32G32B32A32_SFloat, 8);
            // // Texture2D upscaledFrameTexture = CropTexture2D(screenshotTexture, rescaledScreenFrame.CalculateFrameCorners(), rescaledScreenFrame.FrameWidthInt, rescaledScreenFrame.FrameHeightInt);
            // // Texture2D finalTexture = ResizeTexture2D(upscaledFrameTexture, TARGET_FRAME_WIDTH, TARGET_FRAME_HEIGHT);
            // // originalBaseTargetTexture = baseCamera.targetTexture;
            // // baseCamera.targetTexture = initialRenderTexture;
            // var finalRenderTexture = RenderTexture.GetTemporary(targetScreenFrame.ScreenWidthInt, targetScreenFrame.ScreenHeightInt, 24);
            // Graphics.Blit(initialRenderTexture, finalRenderTexture); // we need to Blit to have HDR included on crop
            // CropToScreenshotFrame(upscaledFrameTexture, targetScreenFrame);

            onComplete?.Invoke(screenshotTexture);
        }

        private static Texture2D ResizeTexture2D(Texture originalTexture, int width, int height)
        {
            var rt = new RenderTexture(width, height, 24);
            RenderTexture.active = rt;

            // Copy and scale the original texture into the RenderTexture
            Graphics.Blit(originalTexture, rt);

            // Create a new Texture2D to hold the resized texture data
            var resizedTexture = new Texture2D(width, height);

            // Read the pixel data from the RenderTexture into the Texture2D
            resizedTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            resizedTexture.Apply();

            RenderTexture.active = null;
            rt.Release();

            return resizedTexture;
        }


        private static Texture2D ResizeToTargetScale(Texture originalTexture, int width, int height)
        {
            var rt = new RenderTexture(width, height, 24);

            RenderTexture.active = rt;
            // Copy and scale the original texture into the RenderTexture
            Graphics.Blit(originalTexture, rt);

            // Create a new Texture2D to hold the resized texture data
            var resizedTexture = new Texture2D(width, height);

            // Read the pixel data from the RenderTexture into the Texture2D
            screenshot.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            screenshot.Apply();

            RenderTexture.active = null;
            rt.Release();

            return resizedTexture;
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

        private static (ScreenFrameData data, float targetRescale) CalculateTargetScreenFrame(ScreenFrameData currentScreenFrameData)
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

            return (screenFrameData, targetRescale);
        }
    }
}
