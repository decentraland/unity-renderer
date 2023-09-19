using DCL;
using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

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

        private ScreenFrameData debugTargetScreenFrame;

        public bool IsCapturing { get; private set; }

        public ScreenRecorder(RectTransform canvasRectTransform)
        {
            targetAspectRatio = (float)TARGET_FRAME_WIDTH / TARGET_FRAME_HEIGHT;
            Debug.Assert(targetAspectRatio != 0, "Target aspect ratio cannot be null");

            this.canvasRectTransform = canvasRectTransform;
        }

        public virtual IEnumerator CaptureScreenshot(Action<Texture2D> onComplete)
        {
            IsCapturing = true;

            yield return new WaitForEndOfFrame(); // for UI to appear on screenshot. Converting to UniTask didn't work :(

            ScreenFrameData currentScreenFrame = CalculateCurrentScreenFrame();
            (_, float targetRescale) = CalculateTargetScreenFrame(currentScreenFrame);
            int roundedUpscale = Mathf.CeilToInt(targetRescale);

            // Hotfix for MacOS Desktop crashing on taking screenshot when Explore panel is open
            if (DataStore.i.exploreV2.isOpen.Get() && Application.platform == RuntimePlatform.OSXPlayer)
                roundedUpscale = 1;

            ScreenFrameData rescaledScreenFrame = CalculateRoundRescaledScreenFrame(currentScreenFrame, roundedUpscale);

            Texture2D screenshotTexture = ScreenCapture.CaptureScreenshotAsTexture(roundedUpscale); // upscaled Screen Frame resolution
            Texture2D upscaledFrameTexture = CropTexture2D(screenshotTexture, rescaledScreenFrame.CalculateFrameCorners(), rescaledScreenFrame.FrameWidthInt, rescaledScreenFrame.FrameHeightInt);
            ResizeTexture2D(upscaledFrameTexture);

            onComplete?.Invoke(screenshot);

            Object.Destroy(screenshotTexture);
            Object.Destroy(upscaledFrameTexture);

            IsCapturing = false;
        }

        private static Texture2D CropTexture2D(Texture2D texture, Vector2Int startCorner, int width, int height)
        {
            Color[] pixels = texture.GetPixels(startCorner.x, startCorner.y, width, height);

            var result = new Texture2D(width, height, TextureFormat.RGB24, false);
            result.SetPixels(pixels);
            result.Apply();

            return result;
        }

        private void ResizeTexture2D(Texture originalTexture)
        {
            var renderTexture = RenderTexture.GetTemporary(TARGET_FRAME_WIDTH, TARGET_FRAME_HEIGHT, 0);
            RenderTexture.active = renderTexture;

            // Copy and scale the original texture into the RenderTexture
            Graphics.Blit(originalTexture, renderTexture);

            // Read the pixel data from the RenderTexture into the Texture2D
            screenshot.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            screenshot.Apply();

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(renderTexture);
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
            Debug.Assert(Math.Abs(upscaleFrameWidth - upscaleFrameHeight) < 0.01f, "Screenshot upscale factors should be the same");

            float targetRescale = upscaleFrameWidth;

            screenFrameData.ScreenWidth = currentScreenFrameData.ScreenWidth * upscaleFrameWidth;
            screenFrameData.ScreenHeight = currentScreenFrameData.ScreenHeight * upscaleFrameWidth;
            screenFrameData.FrameWidth = currentScreenFrameData.FrameWidth * upscaleFrameWidth;
            screenFrameData.FrameHeight = currentScreenFrameData.FrameHeight * upscaleFrameWidth;
            Debug.Assert(Math.Abs(screenFrameData.FrameWidth - TARGET_FRAME_WIDTH) < 0.1f, "Calculated screenshot width should be the same as target width");
            Debug.Assert(Math.Abs(screenFrameData.FrameHeight - TARGET_FRAME_HEIGHT) < 0.1f, "Calculated screenshot height should be the same as target height");

            return (screenFrameData, targetRescale);
        }

        private static ScreenFrameData CalculateRoundRescaledScreenFrame(ScreenFrameData rescalingScreenFrame, int roundedRescaleFactor) =>
            new ()
            {
                FrameWidth = rescalingScreenFrame.FrameWidth * roundedRescaleFactor,
                FrameHeight = rescalingScreenFrame.FrameHeight * roundedRescaleFactor,
                ScreenWidth = rescalingScreenFrame.ScreenWidth * roundedRescaleFactor,
                ScreenHeight = rescalingScreenFrame.ScreenHeight * roundedRescaleFactor,
            };
    }
}
