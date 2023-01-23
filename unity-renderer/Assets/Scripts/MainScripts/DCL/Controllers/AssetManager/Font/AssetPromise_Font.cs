using Cysharp.Threading.Tasks;
using MainScripts.DCL.Controllers.AssetManager;
using MainScripts.DCL.Controllers.AssetManager.Font;
using System;
using System.Threading;
using UnityEngine;

namespace DCL
{
    public class AssetPromise_Font : AssetPromise<Asset_Font>
    {
        private readonly bool fetchingEcsFonts;
        private readonly AssetSource permittedSources;
        private readonly string src;
        private CancellationTokenSource cancellationTokenSource;

        private Coroutine fontCoroutine;

        private Service<IFontAssetResolver> fontResolver;

        public AssetPromise_Font(string src, AssetSource permittedSources = AssetSource.ALL)
        {
            this.permittedSources = permittedSources;
            this.src = src;
        }

        protected override void OnAfterLoadOrReuse() { }

        protected override void OnBeforeLoadOrReuse() { }

        protected override void OnCancelLoading()
        {
            CleanCT();
        }

        public override void Cleanup()
        {
            base.Cleanup();
            CleanCT();
        }

        protected override void OnLoad(Action OnSuccess, Action<Exception> OnFail)
        {
            cancellationTokenSource = new CancellationTokenSource();

            async UniTaskVoid LaunchRequest()
            {
                FontResponse result = await fontResolver.Ref.GetFontAsync(permittedSources, src, cancellationTokenSource.Token);

                if (result.IsSuccess)
                {
                    FontSuccessResponse successResponse = result.GetSuccessResponse();
                    asset.font = successResponse.Font;
                    OnSuccess?.Invoke();
                }
                else { OnFail?.Invoke(result.GetFailResponse().Exception); }
            }

            LaunchRequest().Forget();
        }

        protected override bool AddToLibrary()
        {
            if (!library.Add(asset)) { return false; }

            asset = library.Get(asset.id);
            return true;
        }

        public override object GetId() =>
            src;

        private void CleanCT()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
        }
    }
}
