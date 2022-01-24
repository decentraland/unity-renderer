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
        private static int gifLoadingCount = 0;
        public AssetPromise_Gif(string url) { this.url = url; }

        public override object GetId() { return url; }

        protected override void OnLoad(Action OnSuccess, Action<Exception> OnFail)
        {
            tokenSource = new CancellationTokenSource();
            var processor = new GifProcessor(url);
            onSuccsess = OnSuccess;
            asset.processor = processor;

            CancellationToken token = tokenSource.Token;
            gifLoadingCount++;
            Debug.Log($"[{gifLoadingCount}] Loading gif {url}");
            processor.Load(OnLoadSuccsess, OnFail, token)
                     .AttachExternalCancellation(token)
                     .Forget();
        }
        private void OnLoadSuccsess(GifFrameData[] frames)
        {
            gifLoadingCount--;
            asset.frames = frames;
            onSuccsess?.Invoke();
            Debug.Log($"[{gifLoadingCount}] Loading gif finished {url}" );
        }
        protected override void OnCancelLoading()
        {
            gifLoadingCount--;
            tokenSource.Cancel();
            tokenSource.Dispose();
            Debug.Log($"[{gifLoadingCount}] Loading gif FAILED {url}" );

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