using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DCL
{
    public class AssetPromise_Gif : AssetPromise<Asset_Gif>
    {
        private readonly string url;
        private Coroutine loadingRoutine;

        public AssetPromise_Gif(string url) { this.url = url; }

        public override object GetId() { return url; }

        protected override void OnLoad(Action OnSuccess, Action<Exception> OnFail)
        {
            var isAsync = !Configuration.EnvironmentSettings.RUNNING_TESTS;
#if !UNITY_STANDALONE
                isAsync = false;
#endif
            if (isAsync)
            {
                var processor = new GifProcessorAsync(url);
                asset.processor = processor;
                UniTask.Run( () => processor.Load(frames =>
                    {
                        asset.frames = frames;
                        OnSuccess?.Invoke();
                    }, 
                    OnFail)).Forget();
            }
            else
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
        }

        protected override void OnCancelLoading()
        {
            if (loadingRoutine != null)
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

        protected override void OnBeforeLoadOrReuse() { }

        protected override void OnAfterLoadOrReuse() { }
    }
}