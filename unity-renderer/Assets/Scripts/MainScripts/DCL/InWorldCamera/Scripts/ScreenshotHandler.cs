using System.IO;
using UnityEditor;
using UnityEngine;

public class ScreenshotHandler : MonoBehaviour
{
    [ContextMenu(nameof(TakeScreenshot))]
    public void TakeScreenshot()
    {
        var camera = GetComponent<Camera>();
        RenderTexture rt = new RenderTexture(1924, 1080, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        camera.targetTexture = rt;
        camera.Render();

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        RenderTexture.active = null;

        byte[] bytes = tex.EncodeToPNG();
        const string PATH = "Assets/screenshot.png";

        File.WriteAllBytes(PATH, bytes);
        // AssetDatabase.ImportAsset(PATH);
        Debug.Log("Saved to " + PATH);
    }
}
