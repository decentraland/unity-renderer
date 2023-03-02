using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Providers;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MainScripts.DCL.Controllers.HUD.Preloading
{
    public class PreloadingController : IDisposable
    {
        private readonly DataStoreRef<DataStore_LoadingScreen> loadingScreenRef;
        private readonly CancellationTokenSource cancellationTokenSource;

        private GameObject view;
        private bool isDisposed;

        private BaseVariable<bool> isSignUpFlow => DataStore.i.common.isSignUpFlow;

        public PreloadingController(IAddressableResourceProvider addressableResourceProvider)
        {
            cancellationTokenSource = new CancellationTokenSource();
            GetView(cancellationTokenSource.Token).Forget();

            loadingScreenRef.Ref.decoupledLoadingHUD.visible.OnChange += OnDecoupledLoadingScreenVisibilityChange;
            isSignUpFlow.OnChange += SignUpFlowChanged;

            async UniTask GetView(CancellationToken cancellationToken) =>
                view = (await addressableResourceProvider.Instantiate<Transform>("Preloading", cancellationToken: cancellationToken))
                   .gameObject;
        }

        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;

            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();

            WaitForViewsToFadeOut().Forget();
        }

        private void OnDecoupledLoadingScreenVisibilityChange(bool current, bool _)
        {
            if (current)
                Dispose();
        }

        private async UniTask WaitForViewsToFadeOut()
        {
            //This wait will be removed when we merge both loading screen into a single decoupled loading screen
            await UniTask.Delay(TimeSpan.FromSeconds(2), ignoreTimeScale: false);

            loadingScreenRef.Ref.decoupledLoadingHUD.visible.OnChange -= OnDecoupledLoadingScreenVisibilityChange;
            isSignUpFlow.OnChange -= SignUpFlowChanged;

            if(view != null)
                Object.Destroy(view.gameObject);
        }

        private void SignUpFlowChanged(bool current, bool previous)
        {
            if (current)
                Dispose();
        }
    }
}
