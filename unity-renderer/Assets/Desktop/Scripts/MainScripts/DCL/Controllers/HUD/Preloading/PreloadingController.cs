using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using DCL;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MainScripts.DCL.Controllers.HUD.Preloading
{
    public class PreloadingController : IDisposable
    {
        private GameObject view;
        private readonly DataStoreRef<DataStore_LoadingScreen> loadingScreenRef;

        private BaseVariable<bool> isSignUpFlow => DataStore.i.common.isSignUpFlow;
        private bool isDisposed = false;

        public PreloadingController()
        {
            view = Object.Instantiate(GetView());
            loadingScreenRef.Ref.loadingHUD.message.OnChange += OnMessageChange;
            loadingScreenRef.Ref.decoupledLoadingHUD.visible.OnChange += OnDecoupledLoadingScreenVisibilityChange;
            isSignUpFlow.OnChange += SignUpFlowChanged;
        }

        private void OnDecoupledLoadingScreenVisibilityChange(bool current, bool _)
        {
            if (current)
                Dispose();
        }

        private GameObject GetView()
        {
            return Resources.Load<GameObject>("Preloading");
        }

        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;
            WaitForViewsToFadeOut();
        }

        async UniTask WaitForViewsToFadeOut()
        {
            //This wait will be removed when we merge both loading screen into a single decoupled loading screen
            await UniTask.Delay(TimeSpan.FromSeconds(2), ignoreTimeScale: false);
            loadingScreenRef.Ref.loadingHUD.message.OnChange -= OnMessageChange;
            loadingScreenRef.Ref.decoupledLoadingHUD.visible.OnChange -= OnDecoupledLoadingScreenVisibilityChange;
            isSignUpFlow.OnChange -= SignUpFlowChanged;
            Object.Destroy(view.gameObject);
        }

        private void OnMessageChange(string current, string previous)
        {
            if (current.Contains("%"))
                Dispose();
        }

        private void SignUpFlowChanged(bool current, bool previous)
        {
            if (current)
                Dispose();
        }
    }
}
