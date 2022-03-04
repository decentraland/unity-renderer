using UnityEngine;
using UnityEngine.Rendering;

public static class TextureHelpers
{
    public static void EnsureTexture2DMaxSize(ref Texture2D texture, int maxTextureSize)
    {
        if (texture == null)
            return;

        if (texture.width == 0 || texture.height == 0)
            return;

        if (Mathf.Max(texture.height, texture.width) <= maxTextureSize)
            return;

        int w, h;
        if (texture.height > texture.width)
        {
            h = maxTextureSize;
            w = (int) ((texture.width / (float) texture.height) * h);
        }
        else
        {
            w = maxTextureSize;
            h = (int) ((texture.height / (float) texture.width) * w);
        }

        var newTexture = Resize(texture, w, h);
        var oldTexture = texture;
        texture = newTexture;
        Object.Destroy(oldTexture);
    }

    public static Texture2D Resize(Texture2D source, int newWidth, int newHeight, bool linear = false)
    {
        source.filterMode = FilterMode.Point;

        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode = FilterMode.Point;

        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        
        // Texture2D nTex = new Texture2D(newWidth, newHeight, source.format, source.mipmapCount, linear);
        // Texture2D nTex = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, rt.mipmapCount, linear);
        // Texture2D nTex = new Texture2D(newWidth, newHeight);
        // Texture2D nTex = new Texture2D(newWidth, newHeight, TextureFormat.ARGB32, -1, linear);
        // Texture2D nTex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, -1, linear);
        Texture2D nTex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, -1, false);
        
        // nTex.filterMode = FilterMode.Point;
        
        // This works for Graphics.CopyTexture() copying from the source (not RT) without resizing...
        // Texture2D nTex = new Texture2D(source.width, source.height, source.format, -1, linear);
        
        Debug.Log($"TextureHelpers - Resize - copyTeSupport: {SystemInfo.copyTextureSupport.ToString()}");
        bool supportsGPUTextureCopy = SystemInfo.copyTextureSupport != CopyTextureSupport.None;
        if (supportsGPUTextureCopy)
        {
            Debug.Log($"TextureHelpers - Resize - Uses COPYTEXTURE. RT format: {rt.format.ToString()}, rt mipCount: {rt.mipmapCount}");
            // Graphics.CopyTexture(rt, nTex);
            // Graphics.CopyTexture(rt, 0, nTex, 0);
            // Graphics.CopyTexture(rt, 0, 0, nTex, 0, 0);
            Graphics.CopyTexture(rt, 0, 0, nTex, 0, 0);
            // Graphics.CopyTexture(rt, 0, 0, 0, 0, newWidth, newHeight, nTex, 0, 0, 0, 0);

            nTex.alphaIsTransparency = true;
        }
        else
        {
            Debug.Log("TextureHelpers - Resize - Uses READPIXELS");
            nTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
            nTex.Apply();
        }

        RenderTexture.ReleaseTemporary(rt);
        RenderTexture.active = null;

        return nTex;
    }

    public static Texture2D CopyTexture(Texture2D sourceTexture)
    {
        Texture2D texture = new Texture2D(sourceTexture.width, sourceTexture.height, sourceTexture.format, false);
        Graphics.CopyTexture(sourceTexture, texture); // TODO: does this work in WebGL?
        return texture;
    }
}