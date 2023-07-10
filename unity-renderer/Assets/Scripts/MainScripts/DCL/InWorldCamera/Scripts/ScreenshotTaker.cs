using UnityEngine;
using UnityEngine.UI;

public class ScreenshotTaker : MonoBehaviour
{
    public string screenshotFileName = "Assets/screenshot3.png";
    public int desiredWidth = 1920;
    public int desiredHeight = 1080;

    public Camera screenshotCamera;
    public Image image;
    public RectTransform canvas;

    [SerializeField]
    private RectTransform imageRectTransform;
    [SerializeField] private Sprite sprite;

    private void Awake()
    {
        sprite = image.sprite;
        imageRectTransform = image.rectTransform;
    }

    [ContextMenu(nameof(CopyCamera))]
    private void CopyCamera()
    {
        screenshotCamera.CopyFrom(Camera.main);
    }

    [ContextMenu(nameof(DebugScaling))]
    private void DebugScaling()
    {
        sprite = image.sprite;
        imageRectTransform = image.rectTransform;

        var (spriteWidth, spriteHeight) = CalculateImageWH();

        Debug.Log($"current Canvas = {canvas.rect.width},{canvas.rect.height}");
        Debug.Log($"Image = {imageRectTransform.rect.width},{imageRectTransform.rect.height}");
        Debug.Log($"Sprite = {spriteWidth},{spriteHeight}");

        float scaleFactorW = desiredWidth / spriteWidth;
        float scaleFactorH = desiredHeight / spriteHeight;

        int renderTextureW = Mathf.RoundToInt(canvas.rect.width * scaleFactorW);
        int renderTextureH = Mathf.RoundToInt(canvas.rect.height * scaleFactorH);

        Debug.Log($"result RenderTexture = {renderTextureW},{renderTextureH}");

        float rtCenterX = renderTextureW / 2f;
        float rtCenterY = renderTextureH / 2f;

        float cornerX = rtCenterX - (spriteWidth / 2f);
        float cornerY = rtCenterY - (spriteHeight / 2f);

        float scaledImageWidth = spriteWidth * scaleFactorW;
        float scaledImageHeight = spriteHeight * scaleFactorH;

        Debug.Log($"result Texture = {scaledImageWidth},{scaledImageHeight}");
        Debug.Log($"corner = {cornerX},{cornerY}");
    }

    private (float, float) CalculateImageWH()
    {
        Rect imageRect = imageRectTransform.rect;

        float imageWidth = imageRect.width;
        float imageHeight = imageRect.height;

        float imageAspect = imageWidth / imageHeight;
        float spriteAspect = sprite.bounds.size.x / sprite.bounds.size.y;

        float actualWidth, actualHeight;

        // Depending on which dimension is the limiting one (width or height),
        // calculate the actual size of the sprite on screen.
        if (imageAspect > spriteAspect)
        {
            // The height of the RectTransform is the limiting dimension.
            // Calculate the width using the sprite's aspect ratio.
            actualHeight = imageHeight;
            actualWidth = actualHeight * spriteAspect;
        }
        else
        {
            // The width of the RectTransform is the limiting dimension.
            // Calculate the height using the sprite's aspect ratio.
            actualWidth = imageWidth;
            actualHeight = actualWidth / spriteAspect;
        }

        return (actualWidth, actualHeight);
    }

    [ContextMenu(nameof(CaptureScreenshot))]
    void CaptureScreenshot()
    {
        var (spriteWidth, spriteHeight) = CalculateImageWH();

        float scaleFactorW = desiredWidth / spriteWidth;
        float scaleFactorH = desiredHeight / spriteHeight;

        int renderTextureWidth = Mathf.RoundToInt(canvas.rect.width * scaleFactorW);
        int renderTextureHeight = Mathf.RoundToInt(canvas.rect.height * scaleFactorH);

        var renderTexture = RenderTexture.GetTemporary(renderTextureWidth, renderTextureHeight, 24);
        Debug.Log($"RenderTexture = {renderTextureWidth},{renderTextureHeight}");

        screenshotCamera.targetTexture = renderTexture;
        screenshotCamera.Render();
        RenderTexture.active = screenshotCamera.targetTexture;

        float rtCenterX = renderTextureWidth / 2f;
        float rtCenterY = renderTextureHeight / 2f;

        float cornerX = rtCenterX - (desiredWidth / 2f);
        float cornerY = rtCenterY - (desiredHeight / 2f);
        Debug.Log($"corner = {cornerX},{cornerY}");


        Texture2D texture = new Texture2D(Mathf.RoundToInt(desiredWidth), Mathf.RoundToInt(desiredHeight), TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(cornerX, cornerY, desiredWidth, desiredHeight), 0, 0);
        texture.Apply();

        // Save the screenshot
        byte[] bytes = texture.EncodeToPNG();
        System.IO.File.WriteAllBytes(screenshotFileName, bytes);

        // Clean up
        Destroy(texture);

        RenderTexture.active = null; // Added to avoid errors
        screenshotCamera.targetTexture = null;
        RenderTexture.ReleaseTemporary(renderTexture);

        Debug.Log("Screenshot taken");
    }
}
