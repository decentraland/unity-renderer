using UnityEngine;

namespace DCLFeatures.ScreencaptureCamera
{
    public class ScreenRecorder
    {
        private const int FRAME_WIDTH = 1920;
        private const int FRAME_HEIGHT = 1080;

        private readonly Sprite sprite;
        private readonly RectTransform imageRectTransform;
        private readonly RectTransform canvasRectTransform;

        private (float width, float height) spriteRect;

        public ScreenRecorder(RectTransform canvasRectTransform, Sprite sprite, RectTransform spriteRectTransform)
        {
            this.sprite = sprite;
            imageRectTransform = spriteRectTransform;

            this.canvasRectTransform = canvasRectTransform;
        }

        private readonly Texture2D finalTexture = new (FRAME_WIDTH, FRAME_HEIGHT, TextureFormat.RGB24, false);

        public virtual Texture2D CaptureScreenshot()
        {
            Texture2D screenshotTexture = ScreenCapture.CaptureScreenshotAsTexture(1);
            return screenshotTexture;
            // Debug.Log($"golden rule rect {imageRectTransform.rect.width}:{imageRectTransform.rect.height}");
            // Debug.Log($"sprite bounds {sprite.bounds.size.x}:{sprite.bounds.size.y}");
            spriteRect = GetCurrentSpriteResolution(imageRectTransform.rect, sprite.bounds);
            // Debug.Log($"sprite bounds {spriteRect.width}:{spriteRect.height}");

            float scaleFactorW = FRAME_WIDTH / spriteRect.width;
            float scaleFactorH = FRAME_HEIGHT / spriteRect.height;

            int upscalingFactor = Mathf.CeilToInt(Mathf.Max(scaleFactorW, scaleFactorH));
            // Debug.Log($"Scale factor {scaleFactorW}:{scaleFactorH} = {upscalingFactor}");

             screenshotTexture = ScreenCapture.CaptureScreenshotAsTexture(upscalingFactor);
            return screenshotTexture;

            // var path = Application.dataPath + "/_UpscaledScreenshot1.png";
            // File.WriteAllBytes(path, screenshotTexture.EncodeToJPG());
            // Debug.Log($"Upscaled res: {Screen.width * upscalingFactor}:{Screen.height * upscalingFactor}");
            // Debug.Log($"Saved to {path}");

            // TODO(Vit): optimize this by removing intermediate texture creation. Read/Write bilinear of cropped frame
            // Scaling to have 1920x1080 in center
            int imageToCropWidth = Mathf.RoundToInt(canvasRectTransform.rect.width * scaleFactorW);
            int imageToCropHeight = Mathf.RoundToInt(canvasRectTransform.rect.height * scaleFactorH);
            Texture2D rescaledScreenshot = ScaleTexture(screenshotTexture, imageToCropWidth, imageToCropHeight);
            // Debug.Log($"Image to crop {imageToCropWidth}:{imageToCropHeight}");

            // Cropping 1920x1080 central part
            int cornerX = Mathf.RoundToInt((imageToCropWidth - FRAME_WIDTH) / 2f);
            int cornerY = Mathf.RoundToInt((imageToCropHeight - FRAME_HEIGHT) / 2f);
            // Debug.Log($"Coreners {cornerX}:{cornerY}");

            Color[] pixels = rescaledScreenshot.GetPixels(cornerX, cornerY, Mathf.RoundToInt(FRAME_WIDTH), Mathf.RoundToInt(FRAME_HEIGHT));
            finalTexture.SetPixels(pixels);
            finalTexture.Apply();

            //  path = Application.dataPath + "/_FinalScreenshot1.png";
            // File.WriteAllBytes(path, finalTexture.EncodeToJPG());
            // Debug.Log($"Saved to {path}");

            return finalTexture;
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
