using System.IO;
using UnityEngine;

namespace DCLFeatures.ScreencaptureCamera
{
    public class ScreenRecorder
    {
        private const int FRAME_WIDTH = 1920;
        private const int FRAME_HEIGHT = 1080;

        private readonly Camera screenshotCamera;
        private readonly Sprite sprite;
        private readonly RectTransform imageRectTransform;
        private readonly RectTransform canvasRectTransform;

        private (float width, float height) spriteRect;

        public ScreenRecorder(Camera screenshotCamera, RectTransform canvasRectTransform, Sprite sprite, RectTransform spriteRectTransform)
        {
            this.sprite = sprite;
            imageRectTransform = spriteRectTransform;

            this.screenshotCamera = screenshotCamera;
            this.canvasRectTransform = canvasRectTransform;
        }

        public virtual Texture2D CaptureScreenshot()
        {
            Debug.Log($"Screen res: {Screen.width}:{Screen.height}");
            Debug.Log($"Sprite frame res: {spriteRect.width}:{spriteRect.height}");

            spriteRect = GetCurrentSpriteResolution(imageRectTransform.rect, sprite.bounds);

            float scaleFactorW = FRAME_WIDTH / spriteRect.width;
            float scaleFactorH = FRAME_HEIGHT / spriteRect.height;
            Debug.Log($"Frame scale factor: {scaleFactorW}:{scaleFactorH}");
            Debug.Log($"Needed new res: {Screen.width * scaleFactorW}:{Screen.height * scaleFactorH}");

            int upscalingFactor = Mathf.CeilToInt(Mathf.Max(scaleFactorW, scaleFactorH));
            Debug.Log($"Upscaling factor rounded: {upscalingFactor}");

            Texture2D screenshotTexture = ScreenCapture.CaptureScreenshotAsTexture(upscalingFactor);
            string path = Application.dataPath + "/_UpscaledScreenshot1.png";
            File.WriteAllBytes(path, screenshotTexture.EncodeToJPG());
            Debug.Log($"Upscaled res: {Screen.width * upscalingFactor}:{Screen.height * upscalingFactor}");
            Debug.Log($"Saved to {path}");

            int imageToCropWidth = Mathf.RoundToInt(canvasRectTransform.rect.width * scaleFactorW);
            int imageToCropHeight = Mathf.RoundToInt(canvasRectTransform.rect.height * scaleFactorH);
            Texture2D rescaledScreenshot = ScaleTexture(screenshotTexture, imageToCropWidth, imageToCropHeight);
            path = Application.dataPath + "/_RescaledScreenshot1.png";
            File.WriteAllBytes(path, rescaledScreenshot.EncodeToJPG());
            Debug.Log($"Rescaled to: {imageToCropWidth}:{imageToCropHeight}");
            Debug.Log($"Saved to {path}");

            int cornerX = Mathf.RoundToInt((imageToCropWidth - FRAME_WIDTH) / 2f);
            int cornerY = Mathf.RoundToInt((imageToCropHeight - FRAME_HEIGHT) / 2f);

            Color[] pixels = rescaledScreenshot.GetPixels(cornerX, cornerY, Mathf.RoundToInt(FRAME_WIDTH), Mathf.RoundToInt(FRAME_HEIGHT));
            var texture = new Texture2D(FRAME_WIDTH, FRAME_HEIGHT, TextureFormat.RGB24, false);
            texture.SetPixels(pixels);
            texture.Apply();

            path = Application.dataPath + "/_FinalScreenshot1.png";
            File.WriteAllBytes(path, texture.EncodeToJPG());
            Debug.Log($"Saved to {path}");

            // // int newFrameWidth = Mathf.RoundToInt(spriteRect.width * scaleFactorW);
            // // int newFrameHeight = Mathf.RoundToInt(spriteRect.height * scaleFactorH);
            // // Debug.Log($"New Sprite frame res: {newFrameWidth}:{newFrameHeight}");

            // // // Clean up the screenshot texture
            // // // Object.Destroy(screenshotTexture);

            // // // Check for the HDR being saved correctly, but being lost when converting to JPG
            // // // File.WriteAllBytes("Assets/screenshot1.png", texture.EncodeToEXR(Texture2D.EXRFlags.CompressZIP));

            return texture;
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
