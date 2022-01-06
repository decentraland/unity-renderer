using System;
using DCL;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// GifProcessor: Is in charge of choosing which gif processor tu use (typescript's webworker through GIFProcessingBridge or Unity's plugin UniGif)
/// for downloading, processing and discarding gifs
/// </summary>
public class GifProcessorAsync : IGifProcessor
{
    private IWebRequestAsyncOperation webRequestOp;
    private string url;

    public GifProcessorAsync(string url) { this.url = url; }

    /// <summary>
    /// Request the download and processing of a gif
    /// </summary>
    /// <param name="OnSuccess">success callback with gif's frames arry</param>
    /// <param name="OnFail">fail callback</param>
    /// <returns></returns>
    public async UniTask Load(Action<GifFrameData[]> OnSuccess, Action<Exception> OnFail)
    {
        await UniGifProcessorLoad(url, OnSuccess, OnFail);
    }

    /// <summary>
    /// Notify processor that the gif is disposed.
    /// If using UniGif plugin we just cancel the download if pending
    /// If using webworker we send a message to kernel to cancel download and/or remove created texture from memory
    /// </summary>
    public void DisposeGif()
    {
        if (webRequestOp != null)
        {
            webRequestOp.Dispose();
        }
    }

    private async UniTask UniGifProcessorLoad(string url, Action<GifFrameData[]> OnSuccess, Action<Exception> OnFail)
    {
        await UniTask.SwitchToMainThread();

        webRequestOp = DCL.Environment.i.platform.webRequest.Get(url: url, disposeOnCompleted: false);
        await webRequestOp.asyncOp;
        
        if (webRequestOp.isSucceded)
        {
            var bytes = webRequestOp.webRequest.downloadHandler.data;
            
            await UniTask.SwitchToThreadPool();
            await UniGifAsync.GetTextureListAsync(bytes, Callback(OnSuccess, OnFail));
        }
        else
        {
            OnFail?.Invoke(new Exception("Gif web request operation failed"));
        }

        webRequestOp.Dispose();
    }
    private static Action<GifFrameData[], int, int, int> Callback(Action<GifFrameData[]> OnSuccess, Action<Exception> OnFail)
    {
        return (frames, loopCount, width, height) =>
        {
            if (frames != null)
            {
                OnSuccess?.Invoke(frames);
            }
            else
            {
                OnFail?.Invoke(new Exception("Gif does not have any frames"));
            }
        };
    }
}