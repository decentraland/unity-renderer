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
        private CancellationTokenSource tokenSource;
        private bool jsGIFProcessingEnabled;
        
        public AssetPromise_Gif(string url)
        {
            KernelConfig.i.EnsureConfigInitialized().Then(config => jsGIFProcessingEnabled = config.gifSupported);
            this.url = url;
        }

        public override object GetId() { return url; }

        protected override void OnLoad(Action OnSuccess, Action<Exception> OnFail)
        {
            tokenSource = new CancellationTokenSource();
            IGifProcessor processor = GetGifProcessor();
            onSuccsess = OnSuccess;
            asset.processor = processor;
            CancellationToken token = tokenSource.Token;

            processor.Load(OnLoadSuccsess, OnFail, token)
                     .AttachExternalCancellation(token)
                     .Forget();
        }

        private IGifProcessor GetGifProcessor()
        {
            if (jsGIFProcessingEnabled)
            {
                return new JSGifProcessor(url);
            }

            return new GifDecoderProcessor(url, Environment.i.platform.webRequest);
        }
        private void OnLoadSuccsess(GifFrameData[] frames)
        {
            asset.frames = frames;
            onSuccsess?.Invoke();
        }
        protected override void OnCancelLoading()
        {
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