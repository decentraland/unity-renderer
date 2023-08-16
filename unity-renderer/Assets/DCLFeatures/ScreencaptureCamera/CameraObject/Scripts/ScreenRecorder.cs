using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.Universal;

namespace DCLFeatures.ScreencaptureCamera.CameraObject
{
    public class ScreenRecorder
    {
        private const int TARGET_FRAME_WIDTH = 1920;
        private const int TARGET_FRAME_HEIGHT = 1080;
        private const float FRAME_SCALE = 0.87f; // Defines the scale of the frame in relation to the screen

        private readonly float targetAspectRatio;
        private readonly RectTransform canvasRectTransform;

        private readonly List<RenderTexture> originalOverlayTargetTextures = new ();
        private readonly Texture2D screenshot = new (TARGET_FRAME_WIDTH, TARGET_FRAME_HEIGHT, TextureFormat.RGB24, false);
        private List<Camera> overlayCameras;
        private RenderTexture originalBaseTargetTexture;

        public ScreenRecorder(RectTransform canvasRectTransform)
        {
            targetAspectRatio = (float)TARGET_FRAME_WIDTH / TARGET_FRAME_HEIGHT;
            Debug.Assert(targetAspectRatio != 0, "Target aspect ratio cannot be null");

            this.canvasRectTransform = canvasRectTransform;
        }

        public virtual Texture2D CaptureScreenshot(Camera baseCamera)
        {
            ScreenFrameData targetScreenFrame = CalculateTargetScreenFrame(currentScreenFrameData: CalculateCurrentScreenFrame());

            var initialRenderTexture = RenderTexture.GetTemporary(targetScreenFrame.ScreenWidthInt, targetScreenFrame.ScreenHeightInt, 0, GraphicsFormat.R32G32B32A32_SFloat, 8);
            RenderScreenIntoTexture(baseCamera, initialRenderTexture);
            var finalRenderTexture = RenderTexture.GetTemporary(targetScreenFrame.ScreenWidthInt, targetScreenFrame.ScreenHeightInt, 0);
            Graphics.Blit(initialRenderTexture, finalRenderTexture); // we need to Blit to have HDR included on crop

            CropToScreenshotFrame(finalRenderTexture, targetScreenFrame);

            CleanUp(initialRenderTexture, finalRenderTexture);

            return screenshot;
        }

        private void RenderScreenIntoTexture(Camera baseCamera, RenderTexture initialRenderTexture)
        {
            SetAndCacheCamerasTargetTexture(baseCamera, initialRenderTexture);
            baseCamera.Render();
            ResetCamerasTargetTexturesToCached(baseCamera);
        }

        private void CleanUp(RenderTexture initialRenderTexture, RenderTexture finalRenderTexture)
        {
            RenderTexture.ReleaseTemporary(initialRenderTexture);
            RenderTexture.ReleaseTemporary(finalRenderTexture);
            originalOverlayTargetTextures?.Clear();
        }

        private void CropToScreenshotFrame(RenderTexture finalRenderTexture, ScreenFrameData targetScreenFrame)
        {
            RenderTexture.active = finalRenderTexture;
            Vector2Int corners = targetScreenFrame.CalculateFrameCorners();
            screenshot.ReadPixels(new Rect(corners.x, corners.y, TARGET_FRAME_WIDTH, TARGET_FRAME_HEIGHT), 0, 0);
            screenshot.Apply();
            RenderTexture.active = null;
        }

        private void SetAndCacheCamerasTargetTexture(Camera baseCamera, RenderTexture targetRenderTexture)
        {
            originalBaseTargetTexture = baseCamera.targetTexture;
            baseCamera.targetTexture = targetRenderTexture;

            UniversalAdditionalCameraData baseCameraData = baseCamera.GetUniversalAdditionalCameraData();
            overlayCameras = baseCameraData.cameraStack;

            if (overlayCameras != null)
                foreach (Camera overlayCamera in overlayCameras)
                {
                    originalOverlayTargetTextures.Add(overlayCamera.targetTexture);
                    overlayCamera.targetTexture = targetRenderTexture;
                }
        }

        private void ResetCamerasTargetTexturesToCached(Camera baseCamera)
        {
            baseCamera.targetTexture = originalBaseTargetTexture;

            if (overlayCameras != null)
                for (var i = 0; i < overlayCameras.Count; i++)
                    overlayCameras[i].targetTexture = originalOverlayTargetTextures[i];
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
