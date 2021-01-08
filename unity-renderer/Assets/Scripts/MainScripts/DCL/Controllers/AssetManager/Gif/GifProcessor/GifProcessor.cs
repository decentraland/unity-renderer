using System.Collections;
using System;
using DCL.Helpers;
using UnityEngine.Networking;

/// <summary>
/// GifProcessor: Is in charge of choosing which gif processor tu use (typescript's webworker through GIFProcessingBridge or Unity's plugin UniGif)
/// for downloading, processing and discarding gifs
/// </summary>
public class GifProcessor
{
    private bool jsGIFProcessingEnabled = false;
    private UnityWebRequest webRequest;
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
    public IEnumerator Load(Action<GifFrameData[]> OnSuccess, Action OnFail)
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
        else if (!(webRequest is null))
        {
            webRequest.Abort();
        }
    }

    private IEnumerator JSProcessorLoad(string url, Action<GifFrameData[]> OnSuccess, Action OnFail)
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
            OnFail?.Invoke();
        }
    }

    private IEnumerator UniGifProcessorLoad(string url, Action<GifFrameData[]> OnSuccess, Action OnFail)
    {
        webRequest = UnityWebRequest.Get(url);
        yield return webRequest.SendWebRequest();;

        bool success = webRequest != null && webRequest.WebRequestSucceded();
        if (success)
        {
            var bytes = webRequest.downloadHandler.data;
            yield return UniGif.GetTextureListCoroutine(bytes,
                (frames,loopCount, width, height) =>
                {
                    if (frames != null)
                    {
                        OnSuccess?.Invoke(frames);
                    }
                    else
                    {
                        OnFail?.Invoke();
                    }
                });
        }
        else
        {
            OnFail?.Invoke();
        }
        webRequest.Dispose();
        webRequest = null;
    }
}
