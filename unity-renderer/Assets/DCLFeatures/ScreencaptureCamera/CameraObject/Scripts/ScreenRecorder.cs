using System;
using UnityEngine;

namespace DCLFeatures.ScreencaptureCamera.CameraObject
{
    public class ScreenRecorder
    {
        private const int TARGET_FRAME_WIDTH = 1920;
        private const int TARGET_FRAME_HEIGHT = 1080;
        private const float FRAME_SCALE = 0.87f;

        private readonly float targetAspectRatio;
        private readonly RectTransform canvasRectTransform;

        public ScreenRecorder(RectTransform canvasRectTransform)
        {
            targetAspectRatio = (float)TARGET_FRAME_WIDTH / TARGET_FRAME_HEIGHT;
            Debug.Assert(targetAspectRatio != 0, "Target aspect ratio cannot be null");

            this.canvasRectTransform = canvasRectTransform;
        }

        public virtual Texture2D CaptureScreenshot()
        {
            Debug.Log($"targetAspectRatio {targetAspectRatio}");

            float currentScreenWidth = canvasRectTransform.rect.width * canvasRectTransform.lossyScale.x;
            float currentScreenHeight = canvasRectTransform.rect.height * canvasRectTransform.lossyScale.y;
            float currentScreenAspectRatio = currentScreenWidth / currentScreenHeight;
            Debug.Log($"currentScreen {currentScreenWidth}, {currentScreenHeight}");
            Debug.Log($"currentScreenAspectRatio {currentScreenAspectRatio}");

            float currentFrameWidth;
            float currentFrameHeight;

            Debug.Log($"fame simple scale {currentScreenWidth * FRAME_SCALE}, {currentScreenHeight * FRAME_SCALE}");

            // Adjust current by smallest side
            if (currentScreenAspectRatio > targetAspectRatio) // Height is the limiting dimension, so scaling width based on it
            {
                currentFrameHeight = currentScreenHeight * FRAME_SCALE;
                currentFrameWidth = currentFrameHeight * targetAspectRatio;
            }
            else // Width is the limiting dimension, so scaling height based on it
            {
                currentFrameWidth = currentScreenWidth * FRAME_SCALE;
                currentFrameHeight = currentFrameWidth / targetAspectRatio;
            }

            Debug.Log($"currentFrame {currentFrameWidth}, {currentFrameHeight}");

            //=====---- Target
            Debug.Log("TARGET");

            float upscaleFrameWidth = TARGET_FRAME_WIDTH / currentFrameWidth;
            float upscaleFrameHeight = TARGET_FRAME_HEIGHT / currentFrameHeight;
            Debug.Assert(Math.Abs(upscaleFrameWidth - upscaleFrameHeight) < 0.0001f);
            Debug.Log($"targetUpscale {upscaleFrameWidth}, {upscaleFrameHeight}");
            float targetUpscale = upscaleFrameWidth;

            float calculatedTargetFrameWidth = currentFrameWidth * targetUpscale;
            float calculatedTargetFrameHeight = currentFrameHeight * targetUpscale;
            float targetScreenWidth = currentScreenWidth * targetUpscale;
            float targetScreenHeight = currentScreenHeight * targetUpscale;
            Debug.Log($"target Frame and Screen {calculatedTargetFrameWidth}:{calculatedTargetFrameHeight}, {targetScreenWidth}:{targetScreenHeight}");

            //=====---- Rounded Upscaled
            Debug.Log("UPSCALED");

            int upscaleFactor = Mathf.CeilToInt(targetUpscale);
            Debug.Log($"rounded Upscale {upscaleFactor}");

            float upscaledFrameWidth = currentFrameWidth * upscaleFactor;
            float upscaledFrameHeight = currentFrameHeight * upscaleFactor;
            float upscaledScreenWidth = currentScreenWidth * upscaleFactor;
            float upscaledScreenHeight = currentScreenHeight * upscaleFactor;

            Debug.Log($"Upscaled Frame and Screen {upscaledFrameWidth}:{upscaledFrameHeight}, {upscaledScreenWidth}:{upscaledScreenHeight}");

            //=====---- Downscaled from Rounded
            Debug.Log("DOWNSCALED");

            float downscaleScreenWidth = targetScreenWidth / upscaledScreenWidth;
            float downscaleScreenHeight = targetScreenHeight / upscaledScreenHeight;
            Debug.Assert(Math.Abs(downscaleScreenWidth - downscaleScreenHeight) < 0.0001f);
            Debug.Log($"{downscaleScreenWidth}, {downscaleScreenHeight}");
            float targetDownscale = downscaleScreenWidth;
            Debug.Log($"{targetDownscale}");

            float downscaledFrameWidth = upscaledFrameWidth * targetDownscale;
            float downscaledFrameHeight = upscaledFrameHeight * targetDownscale;
            int downscaledScreenWidth = Mathf.RoundToInt(upscaledScreenWidth * targetDownscale);
            int downscaledScreenHeight = Mathf.RoundToInt(upscaledScreenHeight * targetDownscale);

            Debug.Log($"Downscaled Frame and Screen {downscaledFrameWidth}:{downscaledFrameHeight}, {downscaledScreenWidth}:{downscaledScreenHeight}");

            //=====---- Final
            Texture2D screenshotTexture = ScreenCapture.CaptureScreenshotAsTexture(upscaleFactor);

            // Cropping 1920x1080 central part
            int cornerX = Mathf.RoundToInt((upscaledScreenWidth - upscaledFrameWidth) / 2f);
            int cornerY = Mathf.RoundToInt((upscaledScreenHeight - upscaledFrameHeight) / 2f);
            Debug.Log($"Coreners {cornerX}:{cornerY}");

            var upscaledFrameTexture = new Texture2D(Mathf.RoundToInt(upscaledFrameWidth), Mathf.RoundToInt(upscaledFrameHeight), TextureFormat.RGB24, false);
            Color[] pixels = screenshotTexture.GetPixels(cornerX, cornerY, Mathf.RoundToInt(upscaledFrameWidth), Mathf.RoundToInt(upscaledFrameHeight));
            upscaledFrameTexture.SetPixels(pixels);
            upscaledFrameTexture.Apply();

            Texture2D finalTexture = ResizeTo2K(upscaledFrameTexture);

            return finalTexture;
        }

        private static Texture2D ResizeTo2K(Texture originalTexture)
        {
            var rt = new RenderTexture(TARGET_FRAME_WIDTH, TARGET_FRAME_HEIGHT, 24);
            RenderTexture.active = rt;

            // Copy and scale the original texture into the RenderTexture
            Graphics.Blit(originalTexture, rt);

            // Create a new Texture2D to hold the resized texture data
            var resizedTexture = new Texture2D(TARGET_FRAME_WIDTH, TARGET_FRAME_HEIGHT);

            // Read the pixel data from the RenderTexture into the Texture2D
            resizedTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            resizedTexture.Apply();

            // Clean up by releasing the RenderTexture
            RenderTexture.active = null;
            rt.Release();

            return resizedTexture;
        }
    }
}
