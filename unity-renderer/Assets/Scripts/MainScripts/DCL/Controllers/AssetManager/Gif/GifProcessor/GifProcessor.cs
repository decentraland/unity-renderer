using System.Collections;
using System;
using DCL.Helpers;
using UnityEngine.Networking;
using DCL;

/// <summary>
/// GifProcessor: Is in charge of choosing which gif processor tu use (typescript's webworker through GIFProcessingBridge or Unity's plugin UniGif)
/// for downloading, processing and discarding gifs
/// </summary>
public class GifProcessor : IGifProcessor
{
    private bool jsGIFProcessingEnabled = false;
    private IWebRequestAsyncOperation webRequestOp;
    private string url;

    public GifProcessor(string url)
    {
        this.url = url;
        KernelConfig.i.EnsureConfigInitialized().Then(config => jsGIFProcessingEnabled = config.gifSupported);
    }

    /// <summary>
    /// Request the download and processing of a gif
    /// </summary>
    /// <param name="OnSuccess">success callback with gif's frames arry</param>
    /// <param name="OnFail">fail callback</param>
    /// <returns></returns>
    public IEnumerator Load(Action<GifFrameData[]> OnSuccess, Action<Exception> OnFail)
    {
        if (jsGIFProcessingEnabled)
        {
            yield return JSProcessorLoad(url, OnSuccess, OnFail);
        }
        else
        {
            yield return UniGifProcessorLoad(url, OnSuccess, OnFail);
        }
    }

    /// <summary>
    /// Notify processor that the gif is disposed.
    /// If using UniGif plugin we just cancel the download if pending
    /// If using webworker we send a message to kernel to cancel download and/or remove created texture from memory
    /// </summary>
    public void DisposeGif()
    {
        if (jsGIFProcessingEnabled)
        {
            DCL.GIFProcessingBridge.i.DeleteGIF(url);
        }
        else if (webRequestOp != null)
        {
            webRequestOp.Dispose();
        }
    }

    private IEnumerator JSProcessorLoad(string url, Action<GifFrameData[]> OnSuccess, Action<Exception> OnFail)
    {
        bool fetchFailed = false;
        yield return DCL.GIFProcessingBridge.i.RequestGIFProcessor(url,
            (GifFrameData[] newTextures) =>
            {
                if (newTextures == null || newTextures.Length == 0)
                {
                    fetchFailed = true;
                    return;
                }

                OnSuccess?.Invoke(newTextures);
            }, () => fetchFailed = true);

        if (fetchFailed)
        {
            OnFail?.Invoke(new Exception("Gif fetch failed"));
        }
    }

    private IEnumerator UniGifProcessorLoad(string url, Action<GifFrameData[]> OnSuccess, Action<Exception> OnFail)
    {
        webRequestOp = DCL.Environment.i.platform.webRequest.Get(url: url, disposeOnCompleted: false);

        yield return webRequestOp;

        if (webRequestOp.isSucceded)
        {
            var bytes = webRequestOp.webRequest.downloadHandler.data;
            yield return UniGif.GetTextureListCoroutine(bytes,
                (frames, loopCount, width, height) =>
                {
                    if (frames != null)
                    {
                        OnSuccess?.Invoke(frames);
                    }
                    else
                    {
                        OnFail?.Invoke(new Exception("Gif does not have any frames"));
                    }
                });
        }
        else
        {
            OnFail?.Invoke(new Exception("Gif web request operation failed"));
        }

        webRequestOp.Dispose();
    }
}