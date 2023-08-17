﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.Universal;
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

        public ScreenRecorder(RectTransform canvasRectTransform)
        {
            targetAspectRatio = (float)TARGET_FRAME_WIDTH / TARGET_FRAME_HEIGHT;
            Debug.Assert(targetAspectRatio != 0, "Target aspect ratio cannot be null");

            this.canvasRectTransform = canvasRectTransform;
        }

         public Texture2D CaptureScreenshotWithRenderTexture(Camera baseCamera)
        {
            ScreenFrameData currentScreenFrame = CalculateCurrentScreenFrame();
            (ScreenFrameData targetScreenFrame, _) = CalculateTargetScreenFrame(currentScreenFrame);

            RenderTexture initialRenderTexture = new RenderTexture(targetScreenFrame.ScreenWidthInt, targetScreenFrame.ScreenHeightInt, 0, DefaultFormat.HDR);

            UniversalAdditionalCameraData baseCameraData = baseCamera.GetUniversalAdditionalCameraData();

            // Set the base camera's target texture
            RenderTexture originalTargetTexture = baseCamera.targetTexture;
            baseCamera.targetTexture = initialRenderTexture;

            // Iterate through all overlay cameras and set their target textures
            var overlayCameras = baseCameraData.cameraStack;
            var originalOverlayTargetTextures = new List<RenderTexture>();

            if (overlayCameras != null)
            {
                foreach (Camera overlayCamera in overlayCameras)
                {
                    originalOverlayTargetTextures.Add(overlayCamera.targetTexture);
                    overlayCamera.targetTexture = initialRenderTexture;
                }
            }

            // Render the cameras to the RenderTexture
            baseCamera.Render();

            baseCamera.targetTexture = originalTargetTexture;

            if (overlayCameras != null)
            {
                // Revert the target textures of the base and overlay cameras
                for (var i = 0; i < overlayCameras.Count; i++)
                    overlayCameras[i].targetTexture = originalOverlayTargetTextures[i];
            }

            // Create an intermediate RenderTexture with the size of the cropped area
            RenderTexture finalRenderTexture = new RenderTexture(initialRenderTexture.width, initialRenderTexture.height, 0);
            Graphics.Blit(initialRenderTexture, finalRenderTexture);

            // Read the pixels from the RenderTexture
            RenderTexture.active = finalRenderTexture;

            var corners = targetScreenFrame.CalculateFrameCorners();
            Texture2D screenshot = new Texture2D(TARGET_FRAME_WIDTH, TARGET_FRAME_HEIGHT, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(corners.x, corners.y, TARGET_FRAME_WIDTH, TARGET_FRAME_HEIGHT), 0, 0);
            screenshot.Apply();
            RenderTexture.active = null;

            // Clean up
            Object.Destroy(initialRenderTexture);
            Object.Destroy(finalRenderTexture);

            return screenshot;
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
        }
    }
}
