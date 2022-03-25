using UnityEngine;
using UnityEngine.Rendering;

public static class TextureHelpers
{
    public static Texture2D ClampSize(Texture2D source, int maxTextureSize, bool linear = false, bool useGPUCopy = true)
    {
        if (source.width <= maxTextureSize && source.height <= maxTextureSize)
            return source;

        float factor = 1.0f;
        int width = source.width;
        int height = source.height;

        if (width >= height)
        {
            factor = (float)maxTextureSize / width;
        }
        else
        {
            factor = (float)maxTextureSize / height;
        }

        Texture2D dstTex = Resize(source, (int) (width * factor), (int) (height * factor), linear, useGPUCopy);

        if (Application.isPlaying)
            Object.Destroy(source);
        else
            Object.DestroyImmediate(source);

        return dstTex;
    }

    public static Texture2D Resize(Texture2D source, int newWidth, int newHeight, bool linear = false, bool useGPUCopy = true)
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
        if (supportsGPUTextureCopy && useGPUCopy)
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
        
        // Note: Surprisingly this works in WebGL here but it doesn't work in Resize()
        Graphics.CopyTexture(sourceTexture, texture);
        
        return texture;
    }
}