using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace DCL
{
    public class JSGifProcessor : IGifProcessor
    {
        private readonly string url;
        private Action<GifFrameData[]> successCallback;
        private Action<Exception> failCallback;

        public JSGifProcessor(string url) { this.url = url; }

        public async UniTask Load(Action<GifFrameData[]> OnSuccess, Action<Exception> OnFail, CancellationToken cancellationToken)
        {
            try
            {
                await Internal_Load(OnSuccess, OnFail, cancellationToken);
            }
            catch (Exception e)
            {
                if (!(e is OperationCanceledException))
                {
                    OnFail(e);
                }
            }
        }

        private async UniTask Internal_Load(Action<GifFrameData[]> OnSuccess, Action<Exception> OnFail, CancellationToken token)
        {
            successCallback = OnSuccess;
            failCallback = OnFail;

            token.ThrowIfCancellationRequested();
            await GIFProcessingBridge.i.RequestGIFProcessor(url, OnComplete, this.OnFail).ToUniTask(cancellationToken: token);
        }

        private void OnComplete(GifFrameData[] newTextures)
        {
            if (newTextures == null || newTextures.Length == 0)
            {
                OnFail();
                return;
            }

            successCallback?.Invoke(newTextures);
        }

        private void OnFail() { failCallback?.Invoke( new Exception("JS Gif fetch failed")); }

        public void DisposeGif() { GIFProcessingBridge.i.DeleteGIF(url); }
    }
}