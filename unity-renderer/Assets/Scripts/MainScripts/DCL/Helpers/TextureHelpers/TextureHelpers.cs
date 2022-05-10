using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using Unity.Collections;
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


    private static readonly Dictionary<TextureFormat, int> formatByteLength = new Dictionary<TextureFormat, int>()
    {
        { TextureFormat.ARGB32, 4 },
        { TextureFormat.RGBA32, 4 },
        { TextureFormat.RGB24, 3 },
        { TextureFormat.DXT5, 16 },
        { TextureFormat.DXT1, 8 },
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
        
        yield return throttle;

        var yChunks = texture.height / chunkSize;
        var xChunks = texture.width / chunkSize;

        var finalTextureData = finalTexture.GetRawTextureData<byte>();
        var textureData = texture.GetRawTextureData();

        for (int y = 0; y < yChunks; y++)
        {
            for (int x = 0; x < xChunks; x++)
            {
                // TODO: Reuse this texture2D
                Texture2D chunkTexture = new Texture2D(chunkSize, chunkSize, texture.format, generateMimpaps, linear);
                yield return throttle;
                
                // Get chunk texture from original texture
                var data = GetChunkTextureData(textureData, x, y, texture.width, chunkSize, formatByteLength[texture.format]);
                chunkTexture.LoadRawTextureData(data);
                yield return throttle;

                // Compress the chunk
                chunkTexture.Compress(false);
                yield return throttle;

                // Copy into final texture
                WriteChunkTextureDataDXT5(finalTextureData, chunkTexture.GetRawTextureData<byte>(), x, y, chunkSize, texture.width, formatByteLength[chunkTexture.format] );

                yield return throttle;
                Object.Destroy(chunkTexture);
            }
        }

        Object.Destroy(texture);

        finalTexture.wrapMode = texture.wrapMode;
        finalTexture.filterMode = texture.filterMode;

        finalTexture.Apply(generateMimpaps, uploadToGPU);

        OnSuccess(finalTexture);
    }
    private static void CopyFromChunkToTexture(Texture2D chunkTexture, int chunkSize, Texture2D finalTexture, int srcX, int srcY)
    {
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
    }
    private static void CopyFromTextureToChunk(Texture2D texture, int srcX, int srcY, int chunkSize, Texture2D chunkTexture)
    {
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

    public static byte[] GetChunkTextureData(byte[] target, int coordX, int coordY,
        int originalWidth, int chunkSize, int byteLength)
    {
        byte[] result = new byte[4 * chunkSize * chunkSize];

        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                var textureXIndex = coordX * chunkSize * byteLength + x * byteLength;
                var textureYIndex = coordY * originalWidth * chunkSize * byteLength + y * byteLength * originalWidth;
                var textureIndex = textureXIndex + textureYIndex;
                var chunkIndex = chunkSize * y * byteLength + x * byteLength;

                for (int c = 0; c < byteLength; c++)
                {
                    result[chunkIndex + c] = target[textureIndex + c];
                }
            }
        }

        return result;
    }

    private static void WriteChunkTextureDataDXT5(
        NativeArray<byte> blankTexture,
        NativeArray<byte> chunkData,
        int coordX, int coordY, int chunkSize, int originalWidth, int byteLength)
    {
        // Number of bytes that a DXT chunk has ( 4x4 pixels )
        var bytesPerPixelChunk = byteLength;
        // Number of DXT chunks that our chunk has
        var dxtChunkSize = chunkSize / 4;
        // Chunk width in bytes
        var chunkWidth = dxtChunkSize * bytesPerPixelChunk;
        // Total width in bytes
        var totalWidth = originalWidth / chunkSize * chunkWidth;

        // Offsets by chunk coordinates
        var xOffset = coordX * chunkWidth;
        var yOffset = coordY * totalWidth * (chunkSize / 4);

        for (int y = 0; y < dxtChunkSize; y++)
        {
            for (int x = 0; x < chunkWidth; x++)
            {
                int chunkIndex = x + y * chunkWidth;
                int targetIndex = yOffset + xOffset + x + y * totalWidth;

                blankTexture[targetIndex] = chunkData[chunkIndex];
            }
        }
    }

}