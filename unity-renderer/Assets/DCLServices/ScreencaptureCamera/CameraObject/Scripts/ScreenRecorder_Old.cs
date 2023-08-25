using System;
using UnityEngine;

namespace DCLFeatures.ScreencaptureCamera.CameraObject
{
    public class ScreenRecorder_Old
    {
        private const int TARGET_FRAME_WIDTH = 1920;
        private const int TARGET_FRAME_HEIGHT = 1080;
        private const float FRAME_SCALE = 0.87f; // Defines the scale of the frame in relation to the screen

        private readonly float targetAspectRatio;
        private readonly RectTransform canvasRectTransform;

        public ScreenRecorder_Old(RectTransform canvasRectTransform)
        {
            targetAspectRatio = (float)TARGET_FRAME_WIDTH / TARGET_FRAME_HEIGHT;
            Debug.Assert(targetAspectRatio != 0, "Target aspect ratio cannot be null");

            this.canvasRectTransform = canvasRectTransform;
        }

        public virtual Texture2D CaptureScreenshot()
        {
            ScreenFrameData currentScreenFrame = CalculateCurrentScreenFrame();
            (_, float targetRescale) = CalculateTargetScreenFrame(currentScreenFrame);
            int roundedUpscale = Mathf.CeilToInt(targetRescale);
            ScreenFrameData rescaledScreenFrame = CalculateRoundRescaledScreenFrame(currentScreenFrame, roundedUpscale);

            Texture2D screenshotTexture = ScreenCapture.CaptureScreenshotAsTexture(roundedUpscale); // upscaled Screen Frame resolution
            Texture2D upscaledFrameTexture = CropTexture2D(screenshotTexture, rescaledScreenFrame.CalculateFrameCorners(), rescaledScreenFrame.FrameWidthInt, rescaledScreenFrame.FrameHeightInt);
            Texture2D finalTexture = ResizeTexture2D(upscaledFrameTexture, TARGET_FRAME_WIDTH, TARGET_FRAME_HEIGHT);

            return finalTexture;
        }

        private static Texture2D CropTexture2D(Texture2D texture, Vector2Int startCorner, int width, int height)
        {
            Color[] pixels = texture.GetPixels(startCorner.x, startCorner.y, width, height);

            var result = new Texture2D(width, height, TextureFormat.RGB24, false);
            result.SetPixels(pixels);
            result.Apply();

            return result;
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

        private static ScreenFrameData CalculateRoundRescaledScreenFrame(ScreenFrameData rescalingScreenFrame, int roundedRescaleFactor) =>
            new ()
            {
                FrameWidth = rescalingScreenFrame.FrameWidth * roundedRescaleFactor,
                FrameHeight = rescalingScreenFrame.FrameHeight * roundedRescaleFactor,
                ScreenWidth = rescalingScreenFrame.ScreenWidth * roundedRescaleFactor,
                ScreenHeight = rescalingScreenFrame.ScreenHeight * roundedRescaleFactor,
            };

        private struct ScreenFrameData
        {
            public float ScreenWidth;
            public float ScreenHeight;

            public float FrameWidth;
            public float FrameHeight;

            public int ScreenWidthInt => Mathf.RoundToInt(ScreenWidth);
            public int ScreenHeightInt => Mathf.RoundToInt(ScreenHeight);

            public int FrameWidthInt => Mathf.RoundToInt(FrameWidth);
            public int FrameHeightInt => Mathf.RoundToInt(FrameHeight);

            public float ScreenAspectRatio => ScreenWidth / ScreenHeight;
            public float FrameAspectRatio => FrameWidth / FrameHeight;

            public Vector2Int CalculateFrameCorners() =>
                new ()
                {
                    x = Mathf.RoundToInt((ScreenWidth - FrameWidth) / 2f),
                    y = Mathf.RoundToInt((ScreenHeight - FrameHeight) / 2f),
                };

            public static ScreenFrameData operator *(ScreenFrameData frame, int factor) =>
                new()
                {
                    FrameWidth = frame.FrameWidth * factor,
                    FrameHeight = frame.FrameHeight * factor,
                    ScreenWidth = frame.ScreenWidth * factor,
                    ScreenHeight = frame.ScreenHeight * factor
                };
        }
    }
}
