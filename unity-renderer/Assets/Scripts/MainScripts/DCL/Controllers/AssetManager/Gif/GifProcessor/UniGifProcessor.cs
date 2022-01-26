using System;
using System.Threading;
using DCL;
using Cysharp.Threading.Tasks;
using DCL.Helpers;

public class UniGifProcessor : IGifProcessor
{
    private readonly string url;
    private IWebRequestAsyncOperation webRequestOp;
    private Action<GifFrameData[]> successCallback;
    private Action<Exception> failCallback;

    public UniGifProcessor(string url) { this.url = url; }

    public async UniTask Load(Action<GifFrameData[]> OnSuccess, Action<Exception> OnFail, CancellationToken cancellationToken)
    {
        try
        {
            await UniGifProcessorLoad(OnSuccess, OnFail, cancellationToken);
        }
        catch (Exception e)
        {
            if (!(e is OperationCanceledException))
            {
                OnFail(e);
            }
        }
    }

    public void DisposeGif() { webRequestOp?.Dispose(); }

    private async UniTask UniGifProcessorLoad(Action<GifFrameData[]> OnSuccess, Action<Exception> OnFail, CancellationToken cancellationToken)
    {
        successCallback = OnSuccess;
        failCallback = OnFail;
        webRequestOp = DCL.Environment.i.platform.webRequest.Get(url, disposeOnCompleted: false);
        cancellationToken.ThrowIfCancellationRequested();
        await TaskUtils.WaitWebRequest(webRequestOp, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        if (webRequestOp.isSucceded)
        {
            var bytes = webRequestOp.webRequest.downloadHandler.data;
            await TaskUtils.Run(() => UniGif.GetTextureListAsync(bytes, OnUniGifComplete, token: cancellationToken), cancellationToken: cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }
        else
        {
            OnFail?.Invoke(new Exception("Gif web request operation failed"));
        }

        webRequestOp.Dispose();
    }

    private void OnUniGifComplete(GifFrameData[] texturelist, int animationloops, int width, int height)
    {
        if (texturelist != null)
        {
            successCallback?.Invoke(texturelist);
        }
        else
        {
            failCallback?.Invoke(new Exception("Gif does not have any frames"));
        }
    }
}