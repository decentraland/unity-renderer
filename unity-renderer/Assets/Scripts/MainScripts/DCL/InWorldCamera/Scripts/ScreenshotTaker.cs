using MainScripts.DCL.InWorldCamera.Scripts;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ScreenshotTaker : MonoBehaviour
{
    public string screenshotFileName = "screenshot4.png";
    public int desiredWidth = 1920;
    public int desiredHeight = 1080;

    [Space(10)]
    public Camera screenshotCamera;
    public Image image;
    public Canvas canvas;

    [Space(10)]
    [SerializeField] private RectTransform canvasRectTransform;
    [SerializeField] private RectTransform imageRectTransform;
    [SerializeField] private Sprite sprite;
    [SerializeField] private ScreenshotCameraMovement cameraMovement;

    private bool cameraEnabled;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (cameraEnabled)
            {
                screenshotCamera.enabled = false;
                cameraMovement.enabled = false;
                canvas.enabled = false;
            }
            else
            {
                CopyCamera();

                screenshotCamera.enabled = true;
                cameraMovement.enabled = true;
                canvas.enabled = true;
            }

            cameraEnabled = !cameraEnabled;
        }

        if (cameraEnabled && Input.GetKeyDown(KeyCode.Space))
            CaptureScreenshot();
    }

    [ContextMenu(nameof(CopyCamera))]
    private void CopyCamera()
    {
        // screenshotCamera.transform.SetPositionAndRotation(Camera.main.transform.position, Camera.main.transform.rotation);
        screenshotCamera.CopyFrom(Camera.main);
    }

    [ContextMenu(nameof(CaptureScreenshot))]
    private void CaptureScreenshot()
    {
        sprite = image.sprite;
        imageRectTransform = image.rectTransform;

        (float spriteWidth, float spriteHeight) = CalculateSpriteWidthHeight();

        float scaleFactorW = desiredWidth / spriteWidth;
        float scaleFactorH = desiredHeight / spriteHeight;

        int renderTextureWidth = Mathf.RoundToInt(canvasRectTransform.rect.width * scaleFactorW);
        int renderTextureHeight = Mathf.RoundToInt(canvasRectTransform.rect.height * scaleFactorH);

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

        var texture = new Texture2D(Mathf.RoundToInt(desiredWidth), Mathf.RoundToInt(desiredHeight), TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(cornerX, cornerY, desiredWidth, desiredHeight), 0, 0);
        texture.Apply();

        // Save
        SaveScreenshot(texture);

        // Clean up
        Destroy(texture);

        RenderTexture.active = null; // Added to avoid errors
        screenshotCamera.targetTexture = null;
        RenderTexture.ReleaseTemporary(renderTexture);

        Debug.Log("Screenshot taken");

        (float, float) CalculateSpriteWidthHeight()
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
    }

    private void SaveScreenshot(Texture2D texture)
    {
        // Save the screenshot
        byte[] fileBytes = texture.EncodeToPNG();
        // System.IO.File.WriteAllBytes(screenshotFileName, fileBytes);

        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            string base64String = System.Convert.ToBase64String(fileBytes);

            // Construct the data URI for the file
            var dataUri = $"data:application/octet-stream;base64,{base64String}";

            // Open the data URI in a new browser window
            Application.OpenURL(dataUri);
        }
        else // Standalone platforms
        {
            // Save the file bytes to a temporary path
            string tempFilePath = Path.Combine(Application.temporaryCachePath, screenshotFileName);
            File.WriteAllBytes(tempFilePath, fileBytes);

            // Generate a download link for the user
            string downloadLink = GetDownloadLink();

            // Open the link in a new browser window
            Application.OpenURL(downloadLink);
        }
    }

    private string GetDownloadLink()
    {
        string url = string.Format("{0}/{1}", Application.temporaryCachePath, screenshotFileName);
        return url.Replace('\\', '/');
    }
}
