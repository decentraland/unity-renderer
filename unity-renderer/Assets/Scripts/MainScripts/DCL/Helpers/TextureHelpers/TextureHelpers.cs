using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using DCL;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using Graphics = UnityEngine.Graphics;
using Object = UnityEngine.Object;

public static class TextureHelpers
{
    // We if we allow bigger chunks, compressing those chunks may be slow
    private const int MAX_CHUNK_SIZE = 128;
    private const bool DISABLE_THROTTLED_COMPRESSION = false;
    
    // Compression lookup table
    private static readonly Dictionary<TextureFormat, TextureFormat> supportedFormats = new Dictionary<TextureFormat, TextureFormat>()
    {
        { TextureFormat.ARGB32, TextureFormat.DXT5 },
        { TextureFormat.RGBA32, TextureFormat.DXT5 },
        { TextureFormat.RGB24, TextureFormat.DXT1 }
    };
    
    private static readonly Dictionary<PixelFormat, TextureFormat> pixelFormatToTextureFormat = new Dictionary<PixelFormat, TextureFormat>()
    {
        { PixelFormat.Format32bppArgb, TextureFormat.RGBA32 },
        { PixelFormat.Format24bppRgb, TextureFormat.RGB24 }
    };
    private static readonly Dictionary<PixelFormat, TextureFormat> supportedFormatsBmp = new Dictionary<PixelFormat, TextureFormat>()
    {
        { PixelFormat.Format32bppArgb, TextureFormat.DXT5 },
        { PixelFormat.Format24bppRgb, TextureFormat.DXT1 }
    };
    
    // Compression lookup table
    private static readonly Dictionary<TextureFormat, int> formatByteLength = new Dictionary<TextureFormat, int>()
    {
        { TextureFormat.ARGB32, 4 },
        { TextureFormat.RGBA32, 4 },
        { TextureFormat.RGB24, 3 },
        { TextureFormat.DXT5, 16 },
        { TextureFormat.DXT1, 8 },
    };
    
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
    
    /// <summary>
    /// Compresses a texture of RGBA/RGB format into DXT5/1 by separating the original texture into chunks and compressing each chunk separatedly then using the resulting data to draw the final compressed texture
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="uploadToGPU"></param>
    /// <param name="OnSuccess"></param>
    /// <param name="OnFail"></param>
    /// <param name="generateMimpaps"></param>
    /// <param name="linear"></param>
    /// <returns></returns>
    public static IEnumerator ThrottledCompress(Texture2D texture, bool uploadToGPU, Action<Texture2D> OnSuccess, Action<Exception> OnFail, bool generateMimpaps = false, bool linear = false)
    {
        if (!Application.isPlaying)
        {
            OnSuccess(texture);
            yield break;
        }
        
        // Already compressed? 
        if (texture.format == TextureFormat.DXT5 || texture.format == TextureFormat.DXT1)
        {
            OnSuccess(texture);
            yield break;
        }

        if (DISABLE_THROTTLED_COMPRESSION)
        {
            texture.Compress(true);
            OnSuccess(texture);
            yield break;
        }
        
        // small textures are not worth to throttle
        if (texture.width < 32 || texture.height < 32)
        {
            texture.Compress(false);
            OnSuccess(texture);
            yield break;
        }
        
        // other formats? 
        if (!supportedFormats.ContainsKey(texture.format))
        {
            texture.Compress(false);
            Debug.LogWarning($"Texture format: {texture.format} not compatible with this compression method");
            OnSuccess(texture);
            yield break;
        }
            
        // we sacrifice up to ~3 pixels to make it divisible by 4
        var targetTextureWidth = ReduceSizeUntilMultiplierOf4(texture.width);
        var targetTextureHeight = ReduceSizeUntilMultiplierOf4(texture.height);
        
        // we get the most optimal chunk size
        var chunkSizeWidth = GetBiggestChunkSizeFor(targetTextureWidth);
        var chunkSizeHeight = GetBiggestChunkSizeFor(targetTextureHeight);

        // At this point something went wrong
        if (chunkSizeWidth < 0 || chunkSizeHeight < 0)
        {
            texture.Compress(false);
            Debug.LogWarning($"Texture size: {texture.width}x{texture.height} not compatible with this compression method");
            OnSuccess(texture);
            yield break;
        }

        var throttle = new SkipFrameIfDepletedTimeBudget();

        var targetFormat = supportedFormats[texture.format];
        Texture2D finalTexture = new Texture2D(targetTextureWidth, targetTextureHeight, targetFormat, generateMimpaps, linear);
        
        yield return throttle;

        var xChunks = targetTextureWidth / chunkSizeWidth;
        var yChunks = targetTextureHeight / chunkSizeHeight;

        var finalTextureData = finalTexture.GetRawTextureData<byte>();
        var textureData = texture.GetRawTextureData();

        for (int y = 0; y < yChunks; y++)
        {
            for (int x = 0; x < xChunks; x++)
            {
                // TODO: Reuse this texture2D
                Texture2D chunkTexture = new Texture2D(chunkSizeWidth, chunkSizeHeight, texture.format, generateMimpaps, linear);
                yield return throttle;
                
                // Get chunk texture from original texture
                Profiler.BeginSample("[TextureHelper] GetChunkTextureData");
                var data = GetChunkTextureData(textureData, x, y, texture.width, chunkSizeWidth, chunkSizeHeight, formatByteLength[texture.format]);
                Profiler.EndSample();
                
                Profiler.BeginSample("[TextureHelper] LoadRawTextureData");
                chunkTexture.LoadRawTextureData(data);
                Profiler.EndSample();

                yield return throttle;

                // Compress the chunk
                Profiler.BeginSample("[TextureHelper] Compress");
                chunkTexture.Compress(false);
                Profiler.EndSample();

                yield return throttle;

                // Copy into final texture
                Profiler.BeginSample("[TextureHelper] WriteChunkTextureDataDXT5");
                WriteChunkTextureDataDXT5(finalTextureData, chunkTexture.GetRawTextureData<byte>(), x, y, chunkSizeWidth, chunkSizeHeight, targetTextureWidth, formatByteLength[chunkTexture.format] );
                Profiler.EndSample();

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

    /// <summary>
    /// Compresses a texture of RGBA/RGB format into DXT5/1 by separating the original texture into chunks and compressing each chunk separatedly then using the resulting data to draw the final compressed texture
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="uploadToGPU"></param>
    /// <param name="OnSuccess"></param>
    /// <param name="OnFail"></param>
    /// <param name="generateMimpaps"></param>
    /// <param name="linear"></param>
    /// <param name="wrapMode"></param>
    /// <param name="filterMode"></param>
    /// <returns></returns>
    public static IEnumerator ThrottledCompressBmp(Bitmap bmp, bool uploadToGPU, Action<Texture2D> OnSuccess, Action<Exception> OnFail,
        bool generateMimpaps = false,
        bool linear = false,
        TextureWrapMode wrapMode = TextureWrapMode.Clamp,
        FilterMode filterMode = FilterMode.Bilinear)
    {
        // TODO: raise these up
        /*if (!Application.isPlaying)
        {
            OnSuccess(texture);
            yield break;
        }
        
        // Already compressed? 
        if (texture.format == TextureFormat.DXT5 || texture.format == TextureFormat.DXT1)
        {
            OnSuccess(texture);
            yield break;
        }

        if (DISABLE_THROTTLED_COMPRESSION)
        {
            texture.Compress(true);
            OnSuccess(texture);
            yield break;
        }
        
        
        // small textures are not worth to throttle
        if (bmp.Width < 32 || bmp.Height < 32)
        {
            texture.Compress(false);
            OnSuccess(texture);
            yield break;
        }
        
        // other formats? 
        if (!supportedFormatsBmp.ContainsKey(bmp.PixelFormat))
        {
            texture.Compress(false);
            Debug.LogWarning($"Texture format: {texture.format} not compatible with this compression method");
            OnSuccess(texture);
            yield break;
        }*/
            
        // we sacrifice up to ~3 pixels to make it divisible by 4
        var targetTextureWidth = bmp.Width; //ReduceSizeUntilMultiplierOf4(bmp.Width);
        var targetTextureHeight = bmp.Height; //ReduceSizeUntilMultiplierOf4(bmp.Height);
        
        // we get the most optimal chunk size
        var chunkSizeWidth = targetTextureWidth; //GetBiggestChunkSizeFor(targetTextureWidth);
        var chunkSizeHeight = targetTextureHeight; //GetBiggestChunkSizeFor(targetTextureHeight);

        // At this point something went wrong
        /*if (chunkSizeWidth < 0 || chunkSizeHeight < 0)
        {
            texture.Compress(false);
            Debug.LogWarning($"Texture size: {texture.width}x{texture.height} not compatible with this compression method");
            OnSuccess(texture);
            yield break;
        }*/
        

        var throttle = new SkipFrameIfDepletedTimeBudget();

        var targetFormat = supportedFormatsBmp[bmp.PixelFormat];
        /*Texture2D finalTexture = new Texture2D(targetTextureWidth, targetTextureHeight, targetFormat, generateMimpaps, linear)
        {
            wrapMode = wrapMode,
            filterMode = filterMode
        };*/

        yield return throttle;

        var xChunks = targetTextureWidth / chunkSizeWidth;
        var yChunks = targetTextureHeight / chunkSizeHeight;

        //var finalTextureData = finalTexture.GetRawTextureData<byte>();
    
        for (int y = 0; y < yChunks; y++)
        {
            for (int x = 0; x < xChunks; x++)
            {
                // TODO: Reuse this texture2D
                TextureFormat textureFormat = pixelFormatToTextureFormat[bmp.PixelFormat];
                Texture2D chunkTexture = new Texture2D(chunkSizeWidth, chunkSizeHeight, textureFormat, generateMimpaps, linear);
                yield return throttle;
                
                Profiler.BeginSample($"[TextureHelperBmp] LockBits {x+y}");
                BitmapData btmDt = bmp.LockBits(
                    new Rectangle(0, 0, chunkSizeWidth, chunkSizeHeight),
                    ImageLockMode.ReadWrite,
                    bmp.PixelFormat
                );
                Profiler.EndSample();
                
                yield return throttle;
                
                // Get chunk texture from original texture
                Profiler.BeginSample("[TextureHelperBmp] GetChunkTextureData");
                var data = GetChunkTextureDataFromBmp(btmDt, x, y, bmp.Width, bmp.Height, chunkSizeWidth, chunkSizeHeight, formatByteLength[textureFormat]);
                Profiler.EndSample();
                
                Profiler.BeginSample("[TextureHelperBmp] LoadRawTextureData");
                chunkTexture.LoadRawTextureData(data);
                Profiler.EndSample();

                yield return throttle;

                // Compress the chunk
                Profiler.BeginSample("[TextureHelperBmp] Compress");
                chunkTexture.Compress(false);
                Profiler.EndSample();

                chunkTexture.Apply(generateMimpaps, uploadToGPU);
                OnSuccess(chunkTexture);
                yield break;
                
                yield return throttle;

                // Copy into final texture
                //yield return WriteChunkTextureDataDXT5Throttled(finalTextureData, chunkTexture.GetRawTextureData<byte>(), x, y, chunkSizeWidth, chunkSizeHeight, targetTextureWidth, formatByteLength[chunkTexture.format] );

                yield return throttle;
                
                Object.Destroy(chunkTexture);
                
                Profiler.BeginSample("[TextureHelperBmp] UnLockBits");
                bmp.UnlockBits(btmDt);
                Profiler.EndSample();

                yield return throttle;
            }
        }


        //finalTexture.Apply(generateMimpaps, uploadToGPU);

        //OnSuccess(finalTexture);
    }
    private static int ReduceSizeUntilMultiplierOf4(int size)
    {
        var result = -1;
        var current = size;

        while (current > 1 && result < 0)
        {
            if (current % 4 == 0)
            {
                result = current;
            }

            current--;
        }
        return result;
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
    /// <returns></returns>
    private static int GetBiggestChunkSizeFor(int value)
    {
        var current = 4;
        var mult = 1;
        var biggest = -1;

        while (current < value && current <= MAX_CHUNK_SIZE)
        {
            if (value % current == 0)
            {
                biggest = current;
            }

            mult++;
            current = 4 * mult;
        }

        return biggest;
    }

    public static byte[] GetChunkTextureData(byte[] target, int coordX, int coordY,
        int originalWidth, int chunkWidth, int chunkHeight, int byteLength)
    {
        byte[] result = new byte[byteLength * chunkWidth * chunkHeight];

        for (int y = 0; y < chunkHeight; y++)
        {
            for (int x = 0; x < chunkWidth; x++)
            {
                var textureXIndex = coordX * chunkWidth * byteLength + x * byteLength;
                var textureYIndex = coordY * originalWidth * chunkHeight * byteLength + y * byteLength * originalWidth;
                var textureIndex = textureXIndex + textureYIndex;
                var chunkIndex = chunkWidth * y * byteLength + x * byteLength;

                for (int c = 0; c < byteLength; c++)
                {
                    result[chunkIndex + c] = target[textureIndex + c];
                }
            }
        }

        return result;
    }
    
    public static byte[] GetChunkTextureDataFromBmp(BitmapData bmp, int coordX, int coordY,
        int originalWidth, int originalHeight, int chunkWidth, int chunkHeight, int byteLength)
    {
        byte[] result = new byte[byteLength * chunkWidth * chunkHeight];

        unsafe
        {
            byte* bitmapDataPointer = (byte*)bmp.Scan0;

            if (bitmapDataPointer == null)
            {
                return null;
            }

            for (int y = 0; y < chunkHeight; y++)
            {
                for (int x = 0; x < chunkWidth; x++)
                {
                    var textureXIndex = coordX * chunkWidth * byteLength + x * byteLength;
                    int height = coordY * originalWidth * chunkHeight * byteLength + (y+1) * byteLength * originalWidth;
                    int offset = originalHeight * byteLength * originalWidth;
                    var textureYIndex = offset - height;
                    var textureIndex = textureXIndex + textureYIndex;
                    var chunkIndex = chunkWidth * y * byteLength + x * byteLength;

                    if (byteLength == 3)
                    {
                        result[chunkIndex + 2] = bitmapDataPointer[textureIndex];
                        result[chunkIndex + 1] = bitmapDataPointer[textureIndex + 1];
                        result[chunkIndex] = bitmapDataPointer[textureIndex + 2];
                    }
                    else
                    {
                        result[chunkIndex + 2] = bitmapDataPointer[textureIndex];
                        result[chunkIndex + 1] = bitmapDataPointer[textureIndex + 1];
                        result[chunkIndex] = bitmapDataPointer[textureIndex + 2];
                        result[chunkIndex + 3] = bitmapDataPointer[textureIndex + 3];
                    }
                }
            }
        }

        return result;
    }

    private static void WriteChunkTextureDataDXT5(NativeArray<byte> blankTexture,
        NativeArray<byte> chunkData,
        int coordX, int coordY, int chunkSizeWidth, int chunkSizeHeight, int originalWidth, int byteLength)
    {
        // Number of bytes that a DXT chunk has ( 4x4 pixels )
        var bytesPerPixelChunk = byteLength;
        
        // Number of DXT chunks that our chunk has
        var dxtChunkSizeWidth = chunkSizeWidth / 4;
        var dxtChunkSizeHeight = chunkSizeHeight / 4;
        
        // Chunk width in bytes
        var chunkWidth = dxtChunkSizeWidth * bytesPerPixelChunk;
        // Total width in bytes
        var totalWidth = originalWidth / chunkSizeWidth * chunkWidth;

        // Offsets by chunk coordinates
        var xOffset = coordX * chunkWidth;
        var yOffset = coordY * totalWidth * dxtChunkSizeHeight;

        for (int y = 0; y < dxtChunkSizeHeight; y++)
        {
            for (int x = 0; x < chunkWidth; x++)
            {
                int chunkIndex = x + y * chunkWidth;
                int targetIndex = yOffset + xOffset + x + y * totalWidth;

                blankTexture[targetIndex] = chunkData[chunkIndex];
            }
        }
    }
    
    private static IEnumerator WriteChunkTextureDataDXT5Throttled(NativeArray<byte> blankTexture,
        NativeArray<byte> chunkData,
        int coordX, int coordY, int chunkSizeWidth, int chunkSizeHeight, int originalWidth, int byteLength)
    {
        var throttle = new SkipFrameIfDepletedTimeBudget();

        // Number of bytes that a DXT chunk has ( 4x4 pixels )
        var bytesPerPixelChunk = byteLength;
        
        // Number of DXT chunks that our chunk has
        var dxtChunkSizeWidth = chunkSizeWidth / 4;
        var dxtChunkSizeHeight = chunkSizeHeight / 4;
        
        // Chunk width in bytes
        var chunkWidth = dxtChunkSizeWidth * bytesPerPixelChunk;
        // Total width in bytes
        var totalWidth = originalWidth / chunkSizeWidth * chunkWidth;

        // Offsets by chunk coordinates
        var xOffset = coordX * chunkWidth;
        var yOffset = coordY * totalWidth * dxtChunkSizeHeight;

        for (int y = 0; y < dxtChunkSizeHeight; y++)
        {
            yield return throttle;

            Profiler.BeginSample("[TextureHelperBmp] WriteChunkTextureDataDXT5Throttled");
            for (int x = 0; x < chunkWidth; x++)
            {
                int chunkIndex = x + y * chunkWidth;
                int targetIndex = yOffset + xOffset + x + y * totalWidth;

                blankTexture[targetIndex] = chunkData[chunkIndex];
            }
            Profiler.EndSample();
        }
    }
}