using System;
using UnityEngine;

namespace DCL
{
    public class AssetPromise_Gif: AssetPromise<Asset_Gif>
    {
        private readonly string url;
        private Coroutine loadingRoutine;

        public AssetPromise_Gif(string url)
        {
            this.url = url;
        }

        public override object GetId()
        {
            return url;
        }

        protected override void OnLoad(Action OnSuccess, Action OnFail)
        {
            var processor = new GifProcessor(url);
            asset.processor = processor;
            loadingRoutine = CoroutineStarter.Start(
                processor.Load(
                    frames =>
                    {
                        asset.frames = frames;
                        OnSuccess?.Invoke();
                    }, OnFail));
        }

        protected override void OnCancelLoading()
        {
            CoroutineStarter.Stop(loadingRoutine);
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

        protected override void OnBeforeLoadOrReuse()
        {
        }

        protected override void OnAfterLoadOrReuse()
        {
        }
    }
}