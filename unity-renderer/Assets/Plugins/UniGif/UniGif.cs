/*
UniGif
Copyright (c) 2015 WestHillApps (Hironari Nishioka)
This software is released under the MIT License.
http://opensource.org/licenses/mit-license.php
*/

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static partial class UniGif
{
    public delegate void OnLoadFinished(GifFrameData[] textureList, int animationLoops, int width, int height);
    /// <summary>
    /// Get GIF texture list Coroutine
    /// </summary>
    /// <param name="bytes">GIF file byte data</param>
    /// <param name="callback">Callback method(param is GIF texture list, Animation loop count, GIF image width (px), GIF image height (px))</param>
    /// <param name="filterMode">Textures filter mode</param>
    /// <param name="wrapMode">Textures wrap mode</param>
    /// <param name="debugLog">Debug Log Flag</param>
    /// <param name="token"></param>
    /// <returns>IEnumerator</returns>
    public static async UniTask GetTextureListAsync(
        byte[] bytes,
        OnLoadFinished callback,
        FilterMode filterMode = FilterMode.Bilinear,
        TextureWrapMode wrapMode = TextureWrapMode.Clamp,
        bool debugLog = false,
        CancellationToken token = default)
    {
        int loopCount = -1;
        int width = 0;
        int height = 0;

        // Set GIF data
        var gifData = new GifData();
        if (SetGifData(bytes, ref gifData, debugLog) == false)
        {
            Debug.LogError("GIF file data set error.");
            if (callback != null)
            {
                callback(null, loopCount, width, height);
            }
            return;
        }

        // Decode to textures from GIF data
        GifFrameData[] gifTexList = null;
        token.ThrowIfCancellationRequested();
        await DecodeTexture(gifData, result => gifTexList = result, filterMode, wrapMode, token);

        if (gifTexList == null || gifTexList.Length <= 0)
        {
            Debug.LogError("GIF texture decode error.");
            if (callback != null)
            {
                callback(null, loopCount, width, height);
            }
            return;
        }

        loopCount = gifData.m_appEx.loopCount;
        width = gifData.m_logicalScreenWidth;
        height = gifData.m_logicalScreenHeight;

        if (callback != null)
        {
            callback(gifTexList, loopCount, width, height);
        }
    }
}