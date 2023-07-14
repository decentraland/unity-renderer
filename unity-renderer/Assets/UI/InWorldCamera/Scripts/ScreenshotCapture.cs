﻿using UnityEngine;
using UnityEngine.UI;

namespace MainScripts.DCL.InWorldCamera.Scripts
{
    public class ScreenshotCapture
    {
        private const int DESIRED_WIDTH = 1920;
        private const int DESIRED_HEIGHT = 1080;

        private readonly Camera screenshotCamera;
        private readonly Sprite sprite;
        private readonly RectTransform imageRectTransform;
        private readonly RectTransform canvasRectTransform;

        private (float width, float height) spriteRect;

        private Texture2D textureLazyValue;
        private Texture2D texture => textureLazyValue ??= new Texture2D(Mathf.RoundToInt(DESIRED_WIDTH), Mathf.RoundToInt(DESIRED_HEIGHT), TextureFormat.RGB24, false);

        public ScreenshotCapture(Camera screenshotCamera, RectTransform canvasRectTransform, Image refBoundariesImage)
        {
            sprite = refBoundariesImage.sprite;
            imageRectTransform = refBoundariesImage.rectTransform;

            this.screenshotCamera = screenshotCamera;
            this.canvasRectTransform = canvasRectTransform;
        }

        public byte[] CaptureScreenshot()
        {
            spriteRect = GetCurrentSpriteResolution(imageRectTransform.rect, sprite.bounds);

            float scaleFactorW = DESIRED_WIDTH / spriteRect.width;
            float scaleFactorH = DESIRED_HEIGHT / spriteRect.height;

            int renderTextureWidth = Mathf.RoundToInt(canvasRectTransform.rect.width * scaleFactorW);
            int renderTextureHeight = Mathf.RoundToInt(canvasRectTransform.rect.height * scaleFactorH);

            var renderTexture = RenderTexture.GetTemporary(renderTextureWidth, renderTextureHeight, 24);

            screenshotCamera.targetTexture = renderTexture;
            screenshotCamera.Render();
            RenderTexture.active = screenshotCamera.targetTexture;

            float cornerX = (renderTextureWidth - DESIRED_WIDTH) / 2f;
            float cornerY = (renderTextureHeight - DESIRED_HEIGHT) / 2f;

            texture.ReadPixels(new Rect(cornerX, cornerY, DESIRED_WIDTH, DESIRED_HEIGHT), 0, 0);
            texture.Apply();

            // Clean up
            RenderTexture.active = null;
            screenshotCamera.targetTexture = null;
            RenderTexture.ReleaseTemporary(renderTexture);

            return texture.EncodeToJPG();
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
