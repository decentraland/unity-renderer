using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

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

    public static Texture2D Resize(Texture2D source, int newWidth, int newHeight, bool linear = false, bool useGPUCopy = true)
    {
        newWidth = Mathf.Max(1, newWidth);
        newHeight = Mathf.Max(1, newHeight);

        // RenderTexture default format is ARGB32
        Texture2D nTex = new Texture2D(newWidth, newHeight, TextureFormat.ARGB32, 1, linear);
        nTex.filterMode = source.filterMode;
        nTex.wrapMode = source.wrapMode;

        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode = FilterMode.Point;
        source.filterMode = FilterMode.Point;

        RenderTexture.active = rt;
        Graphics.Blit(source, rt);

        // GPU Texture copy doesn't work for the Asset Bundles Converter since Application.isPlaying is false
        bool supportsGPUTextureCopy = Application.isPlaying && SystemInfo.copyTextureSupport != CopyTextureSupport.None;

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

    private static readonly Dictionary<TextureFormat, TextureFormat> supportedFormats = new Dictionary<TextureFormat, TextureFormat>()
    {
        { TextureFormat.ARGB32, TextureFormat.DXT5 },
        { TextureFormat.RGBA32, TextureFormat.DXT5 },
        { TextureFormat.RGB24, TextureFormat.DXT1 }
    };
    public static IEnumerator ThrottledCompress(Texture2D texture, bool uploadToGPU, Action<Texture2D> OnSuccess, Action<Exception> OnFail, bool generateMimpaps = false, bool linear = false)
    {
        if (texture.format == TextureFormat.DXT5 || texture.format == TextureFormat.DXT1)
        {
            OnSuccess(texture);
            yield break;
        }
        var throttle = new SkipFrameIfDepletedTimeBudget();

        var chunkSize = GetBiggestChunkSizeFor(texture.width, texture.height);

        if (chunkSize < 0)
        {
            OnFail(new Exception($"Texture size: {texture.width}x{texture.height} not compatible with this compression method"));

            yield break;
        }

        if (!supportedFormats.ContainsKey(texture.format))
        {
            OnFail(new Exception($"Texture format: {texture.format} not compatible with this compression method"));

            yield break;
        }

        var targetFormat = supportedFormats[texture.format];

        Texture2D finalTexture = new Texture2D(texture.width, texture.height, targetFormat, generateMimpaps, linear);
        finalTexture.Apply(generateMimpaps, uploadToGPU);
        
        yield return throttle;

        var yChunks = texture.height / chunkSize;
        var xChunks = texture.width / chunkSize;

        for (int y = 0; y < yChunks; y++)
        {
            for (int x = 0; x < xChunks; x++)
            {
                // TODO: Reuse this texture2D
                Texture2D chunkTexture = new Texture2D(chunkSize, chunkSize, texture.format, generateMimpaps, linear);

                yield return throttle;

                // Copy the chunk
                int srcX = x * chunkSize;
                int srcY = y * chunkSize;

                if (srcX % 4 != 0)
                {
                    Debug.LogError("Coordinate not mul4");
                }
                if (srcY % 4 != 0)
                {
                    Debug.LogError("Coordinate not mul4");
                }

                Graphics.CopyTexture(
                    src: texture,
                    srcElement: 0,
                    srcMip: 0,
                    srcX: srcX,
                    srcY: srcY,
                    srcWidth: chunkSize,
                    srcHeight: chunkSize,
                    dst: chunkTexture,
                    dstElement: 0,
                    dstMip: 0,
                    dstX: 0,
                    dstY: 0
                );
                
                // Compress it
                chunkTexture.Compress(false);

                yield return throttle;

                // Upload it to GPU
                chunkTexture.Apply(generateMimpaps, true);

                yield return throttle;

                // Copy into final texture
                Graphics.CopyTexture(
                    src: chunkTexture,
                    srcElement: 0,
                    srcMip: 0,
                    srcX: 0,
                    srcY: 0,
                    srcWidth: chunkSize,
                    srcHeight: chunkSize,
                    dst: finalTexture,
                    dstElement: 0,
                    dstMip: 0,
                    dstX: srcX,
                    dstY: srcY
                );

                yield return throttle;

                Object.Destroy(chunkTexture);
            }
        }
        
        Object.Destroy(texture);

        finalTexture.wrapMode = texture.wrapMode;
        finalTexture.filterMode = texture.filterMode;
        
        OnSuccess(finalTexture);
    }

    /// <summary>
    /// Gets the biggest chunk size that is multiplier of 4 based on a width and height, this is required for writing chunk texture data on DXT formats
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    private static int GetBiggestChunkSizeFor(int width, int height)
    {
        var current = 4;
        var mult = 1;
        var biggest = -1;

        while (current < height && current < width && current < 256)
        {
            if (height % current == 0 && width % current == 0)
            {
                biggest = current;
            }

            mult++;
            current = 4 * mult;
        }

        return biggest;
    }

    private static IEnumerable CopyChunkFromSourceTexture(Texture2D sourceTexture, Texture2D targetChunk, int coordX, int coordY, int chunkSize)
    {
        Debug.Log("Copy texture?");
        var throttle = new SkipFrameIfDepletedTimeBudget();

        Graphics.CopyTexture(
            src: sourceTexture,
            srcElement: 0,
            srcMip: 0,
            srcX: coordX * chunkSize,
            srcY: coordY * chunkSize,
            srcWidth: chunkSize,
            srcHeight: chunkSize,
            dst: targetChunk,
            dstElement: 0,
            dstMip: 0,
            dstX: 0,
            dstY: 0
        );

        yield return throttle;

    }
    private static IEnumerable CopyChunkToFinalTexture(Texture2D chunkTexture, Texture2D target, int coordX, int coordY, int chunkSize)
    {
        Debug.Log("Paste texture?");
        var throttle = new SkipFrameIfDepletedTimeBudget();

        Graphics.CopyTexture(
            src: chunkTexture,
            srcElement: 0,
            srcMip: 0,
            srcX: 0,
            srcY: 0,
            srcWidth: chunkSize,
            srcHeight: chunkSize,
            dst: target,
            dstElement: 0,
            dstMip: 0,
            dstX: coordX * chunkSize,
            dstY: coordY * chunkSize
        );

        yield return throttle;

    }

}