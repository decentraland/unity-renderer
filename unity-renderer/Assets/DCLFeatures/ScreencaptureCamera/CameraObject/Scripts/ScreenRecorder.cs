﻿using System;
using UnityEngine;

namespace DCLFeatures.ScreencaptureCamera
{
    public class ScreenRecorder
    {
        private const int TARGET_FRAME_WIDTH = 1920;
        private const int TARGET_FRAME_HEIGHT = 1080;
        private const float FRAME_SCALE = 0.87f;

        private readonly float targetAspectRatio;

        private readonly Sprite sprite;
        private readonly RectTransform imageRectTransform;
        private readonly RectTransform canvasRectTransform;

        private readonly Texture2D finalTexture = new (TARGET_FRAME_WIDTH, TARGET_FRAME_HEIGHT, TextureFormat.RGB24, false);

        private (float width, float height) spriteRect;

        public ScreenRecorder(RectTransform canvasRectTransform, Sprite sprite, RectTransform spriteRectTransform)
        {
            targetAspectRatio = (float) TARGET_FRAME_WIDTH / TARGET_FRAME_HEIGHT;
            Debug.Assert(targetAspectRatio != 0, "Target aspect ratio cannot be null");

            this.sprite = sprite;
            imageRectTransform = spriteRectTransform;

            this.canvasRectTransform = canvasRectTransform;
        }

        public virtual Texture2D CaptureScreenshot(int targetScreenWidth, int targetScreenHeight)
        {
            //=====---- Final
            // Texture2D screenshotTexture = ScreenCapture.CaptureScreenshotAsTexture(upscaleFactor);
            // TODO(Vit): optimize this by removing intermediate texture creation. Read/Write bilinear of cropped frame
            // Texture2D rescaledScreenshot = ScaleTexture(screenshotTexture, downscaledScreenWidth, downscaledScreenHeight);
            // Cropping 1920x1080 central part
            int cornerX = Mathf.RoundToInt((targetScreenWidth - TARGET_FRAME_WIDTH) / 2f);
            int cornerY = Mathf.RoundToInt((targetScreenHeight - TARGET_FRAME_HEIGHT) / 2f);
            Debug.Log($"Corners {cornerX}:{cornerY}");

            finalTexture.ReadPixels(new Rect(cornerX, cornerY, TARGET_FRAME_WIDTH, TARGET_FRAME_HEIGHT), 0, 0);
            finalTexture.Apply();

            // var path = Application.dataPath + "/_UpscaledScreenshot1.png";
            // File.WriteAllBytes(path, screenshotTexture.EncodeToJPG());
            // Debug.Log($"Upscaled res: {Screen.width * upscalingFactor}:{Screen.height * upscalingFactor}");
            // Debug.Log($"Saved to {path}");

            // var path = Application.dataPath + "/_FinalScreenshot1.jpg";
            // File.WriteAllBytes(path, finalTexture.EncodeToJPG());
            // Debug.Log($"Saved to {path}");

            return finalTexture;
        }

        public void CalculateTargetScreenResolution(out float targetScreenWidth, out float targetScreenHeight)
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
            targetScreenWidth = currentScreenWidth * targetUpscale;
            targetScreenHeight = currentScreenHeight * targetUpscale;
            Debug.Log(
                $"target Frame and Screen {calculatedTargetFrameWidth}:{calculatedTargetFrameHeight}, {targetScreenWidth}:{targetScreenHeight}");

            //=====---- Rounded Upscaled
            Debug.Log("UPSCALED");

            int upscaleFactor = Mathf.CeilToInt(targetUpscale);
            Debug.Log($"rounded Upscale {upscaleFactor}");

            float upscaledFrameWidth = currentFrameWidth * upscaleFactor;
            float upscaledFrameHeight = currentFrameHeight * upscaleFactor;
            float upscaledScreenWidth = currentScreenWidth * upscaleFactor;
            float upscaledScreenHeight = currentScreenHeight * upscaleFactor;

            Debug.Log(
                $"Upscaled Frame and Screen {upscaledFrameWidth}:{upscaledFrameHeight}, {upscaledScreenWidth}:{upscaledScreenHeight}");

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

            Debug.Log(
                $"Downscaled Frame and Screen {downscaledFrameWidth}:{downscaledFrameHeight}, {downscaledScreenWidth}:{downscaledScreenHeight}");
        }

        private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            var result = new Texture2D(targetWidth, targetHeight, source.format, true);

            Color[] rpixels = result.GetPixels(0);

            float incX = 1.0f / targetWidth;
            float incY = 1.0f / targetHeight;

            for (var px = 0; px < rpixels.Length; px++)
                rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * Mathf.Floor(px / targetWidth));

            result.SetPixels(rpixels, 0);
            result.Apply();
            return result;
        }

        private static (float, float) GetCurrentSpriteResolution(Rect imageRect, Bounds spriteBounds)
        {
            float imageWidth = imageRect.width;
            float imageHeight = imageRect.height;

            float imageAspect = imageWidth / imageHeight;
            float spriteAspect = spriteBounds.size.x / spriteBounds.size.y;

            // Depending on which dimension is the limiting one (width or height),
            // calculate the actual size of the sprite on screen (sing the sprite's aspect ratio.)
            return imageAspect > spriteAspect
                ? (imageHeight * spriteAspect, imageHeight) // Height is the limiting dimension, so scaling width based on it
                : (imageWidth, imageWidth / spriteAspect); // Width is the limiting dimension, so scaling height based on it
        }
    }
}
