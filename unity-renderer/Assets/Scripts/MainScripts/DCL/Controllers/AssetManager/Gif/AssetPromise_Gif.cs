using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DCL
{
    public class AssetPromise_Gif : AssetPromise<Asset_Gif>
    {
        private readonly string url;
        private Action onSuccsess;
        private Action<Exception> onFail;
        private CancellationTokenSource tokenSource;
        private bool jsGIFProcessingEnabled;
        private bool isLoading = false;

        public AssetPromise_Gif(string url)
        {
            KernelConfig.i.EnsureConfigInitialized().Then(config => jsGIFProcessingEnabled = config.gifSupported);
            this.url = url;
        }

        public override object GetId() { return url; }

        protected override void OnLoad(Action OnSuccess, Action<Exception> OnFail)
        {
            tokenSource = new CancellationTokenSource();
            IGifProcessor processor;

            if (asset.processor == null)
            {
                processor = GetGifProcessor();
                asset.processor = processor;
            }
            else
            {
                processor = asset.processor;
            }
            onSuccsess = OnSuccess;
            onFail = OnFail;

            CancellationToken token = tokenSource.Token;

            isLoading = true;

            processor.Load(OnLoadSuccsess, OnLoadFail, token)
                     .AttachExternalCancellation(token)
                     .Forget();
        }

        private void OnLoadFail(Exception exception)
        {
            isLoading = false;
            onFail(exception);
        }

        private IGifProcessor GetGifProcessor()
        {
            if (jsGIFProcessingEnabled)
            {
                return new JSGifProcessor(url);
            }

            return new GifDecoderProcessor(url, Environment.i.platform.webRequest);
        }

        public override bool keepWaiting => isLoading;

        private void OnLoadSuccsess(GifFrameData[] frames)
        {
            isLoading = false;
            asset.frames = frames;
            onSuccsess?.Invoke();
        }
        protected override void OnCancelLoading()
        {
            isLoading = false;
            tokenSource.Cancel();
            tokenSource.Dispose();
        }

        protected override bool AddToLibrary()
        {
            if (!library.Add(asset))
            {
                Debug.Log("add to library fail?");
                return false;
            }

            asset = library.Get(asset.id);
            return true;
        }

        protected override void OnBeforeLoadOrReuse() { }

        protected override void OnAfterLoadOrReuse() { }
    }
}
