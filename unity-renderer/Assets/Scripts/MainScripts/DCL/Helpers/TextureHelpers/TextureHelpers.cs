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
        // RenderTexture default format is ARGB32
        Texture2D nTex = new Texture2D(newWidth, newHeight, TextureFormat.ARGB32, 1, linear);
        nTex.filterMode = source.filterMode;
        nTex.wrapMode = source.wrapMode;

        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode = FilterMode.Point;
        source.filterMode = FilterMode.Point;

        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        
        bool supportsGPUTextureCopy = SystemInfo.copyTextureSupport != CopyTextureSupport.None;
        if (supportsGPUTextureCopy)
        {
            Graphics.CopyTexture(rt, nTex);
        }
        else
        {
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
        
        bool supportsGPUTextureCopy = SystemInfo.copyTextureSupport != CopyTextureSupport.None;
        if (supportsGPUTextureCopy)
        {
            Graphics.CopyTexture(sourceTexture, texture);
        }
        else
        {
            RenderTexture rt = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height);
            rt.filterMode = FilterMode.Point;
            FilterMode sourceFilterMode = sourceTexture.filterMode; 
            sourceTexture.filterMode = FilterMode.Point;

            RenderTexture.active = rt;
            Graphics.Blit(sourceTexture, rt);
            
            texture.ReadPixels(new Rect(0, 0, sourceTexture.width, sourceTexture.height), 0, 0);
            texture.Apply();
            
            RenderTexture.ReleaseTemporary(rt);
            RenderTexture.active = null;
            sourceTexture.filterMode = sourceFilterMode;
        }
        
        return texture;
    }
}