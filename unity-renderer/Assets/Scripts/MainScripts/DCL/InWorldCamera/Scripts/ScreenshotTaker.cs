using UnityEngine;

public class ScreenshotTaker : MonoBehaviour
{
    public Camera mainCamera;
    public string screenshotFileName = "Assets/screenshot3.png";
    public int screenshotWidth = 1920;
    public int screenshotHeight = 1080;

    private Camera screenshotCamera;

    [ContextMenu(nameof(CreateCamera))]
    private void CreateCamera()
    {
        // Calculate the viewport size of the secondary camera
        int originalViewportWidth = mainCamera.pixelWidth;
        int originalViewportHeight = mainCamera.pixelHeight;

        // We want the central 16:9 part to be 1920x1080
        int desiredWidth = 1920;
        int desiredHeight = 1080;

        // Calculate the height of the secondary camera's viewport based on the desired width
        int secondaryViewportWidth = originalViewportWidth * desiredWidth / (originalViewportWidth * 9 / 16);
        int secondaryViewportHeight = secondaryViewportWidth * 10 / 10; // Maintain the 10:10 aspect ratio

        // Create a secondary camera and copy properties from the main camera
         screenshotCamera = new GameObject("Screenshot Camera").AddComponent<Camera>();
        screenshotCamera.CopyFrom(mainCamera);
    }

    private void CreateCamera1()
    {
        // Calculate the dimensions of the 16:9 rectangle in the center of the screen
        int height = mainCamera.pixelHeight;
        int width = mainCamera.pixelWidth;
        float desiredAspect = (float)screenshotWidth / screenshotHeight;
        float currentAspect = (float)width / height;
        float scaleHeight = currentAspect / desiredAspect;
        float scaleWidth = 1f;

        // Create a secondary camera and copy properties from the main camera
        screenshotCamera = new GameObject("Screenshot Camera").AddComponent<Camera>();
        screenshotCamera.CopyFrom(mainCamera);

        // Set up the screenshot camera for 16:9 capture, positioned in the center of main camera view
        screenshotCamera.rect = new Rect((1f - scaleWidth) / 2f, (1f - scaleHeight) / 2f, scaleWidth, scaleHeight);
    }

    [ContextMenu(nameof(CaptureScreenshot))]
    void CaptureScreenshot()
    {
        int originalViewportWidth = mainCamera.pixelWidth;
        int originalViewportHeight = mainCamera.pixelHeight;

        // We want the central 16:9 part to be 1920x1080
        int desiredWidth = 1920;
        int desiredHeight = 1080;

        // Calculate the height of the secondary camera's viewport based on the desired width
        int secondaryViewportWidth = originalViewportWidth * desiredWidth / (originalViewportWidth * 9 / 16);
        int secondaryViewportHeight = secondaryViewportWidth * 10 / 10; // Maintain the 10:10 aspect ratio

        // Set up the secondary camera to render to a temporary render texture
        screenshotCamera.targetTexture = RenderTexture.GetTemporary(secondaryViewportWidth, secondaryViewportHeight, 24);

        // Render the screenshot
        screenshotCamera.Render();

        // Read the pixels from the render texture to a new Texture2D
        RenderTexture.active = screenshotCamera.targetTexture;
        Texture2D screenshot = new Texture2D(desiredWidth, desiredHeight, TextureFormat.RGB24, false);

        // Cut out the central part of the render texture
        screenshot.ReadPixels(new Rect((secondaryViewportWidth - desiredWidth) / 2, (secondaryViewportHeight - desiredHeight) / 2, desiredWidth, desiredHeight), 0, 0);
        screenshot.Apply();

        // Save the screenshot
        byte[] bytes = screenshot.EncodeToPNG();
        System.IO.File.WriteAllBytes(screenshotFileName, bytes);

        // Clean up
        RenderTexture.active = null; // Added to avoid errors
        Destroy(screenshot);
        RenderTexture.ReleaseTemporary(screenshotCamera.targetTexture);
    }
    private void CaptureScreenshot1()
    {
        screenshotCamera.targetTexture = RenderTexture.GetTemporary(screenshotWidth, screenshotHeight, 24);

        // Render the screenshot
        screenshotCamera.Render();

        // Read the pixels from the render texture to a new Texture2D
        RenderTexture.active = screenshotCamera.targetTexture;
        Texture2D screenshot = new Texture2D(screenshotWidth, screenshotHeight, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, screenshotWidth, screenshotHeight), 0, 0);
        screenshot.Apply();

        // Save the screenshot
        byte[] bytes = screenshot.EncodeToPNG();
        System.IO.File.WriteAllBytes(screenshotFileName, bytes);

        // Clean up
        RenderTexture.active = null; // Added to avoid errors
        Destroy(screenshot);
        RenderTexture.ReleaseTemporary(screenshotCamera.targetTexture);
        Debug.Log("Screenshot taken");
    }
}
