using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using System.Linq;
using DCL.Helpers;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DCL.Components
{

    public class LoadWrapper_GLTF : LoadWrapper
    {
        static readonly bool VERBOSE = false;
        RendereableAssetLoadHelper loadHelper;

#if UNITY_EDITOR
        [ContextMenu("Debug Load Count")]
        public void DebugLoadCount()
        {
            loadHelper?.DebugLoadCount();
        }
#endif

        public override void Load(string targetUrl, Action<LoadWrapper> OnSuccess, Action<LoadWrapper> OnFail)
        {
            if (loadHelper != null)
            {
                loadHelper.Unload();

                if (VERBOSE)
                    Debug.Log("Forgetting not null loader...");
            }

            alreadyLoaded = false;
            Assert.IsFalse(string.IsNullOrEmpty(targetUrl), "url is null!!");
            loadHelper = new RendereableAssetLoadHelper(this.entity.scene.contentProvider, entity.scene.sceneData.baseUrlBundles);

            loadHelper.settings.parent = transform;

            if (initialVisibility == false)
            {
                loadHelper.settings.visibleFlags = AssetPromiseSettings_Rendering.VisibleFlags.INVISIBLE;
            }
            else
            {
                if (useVisualFeedback)
                    loadHelper.settings.visibleFlags = AssetPromiseSettings_Rendering.VisibleFlags.VISIBLE_WITH_TRANSITION;
                else
                    loadHelper.settings.visibleFlags = AssetPromiseSettings_Rendering.VisibleFlags.VISIBLE_WITHOUT_TRANSITION;
            }

            loadHelper.OnSuccessEvent += (x) => OnSuccessWrapper(OnSuccess);
            loadHelper.OnFailEvent += () => OnFailWrapper(OnSuccess);
            loadHelper.Load(targetUrl);
        }

        private void OnFailWrapper(Action<LoadWrapper> OnFail)
        {
            alreadyLoaded = true;
            OnFail?.Invoke(this);
        }

        private void OnSuccessWrapper(Action<LoadWrapper> OnSuccess)
        {
            alreadyLoaded = true;
            this.entity.OnCleanupEvent -= OnEntityCleanup;
            this.entity.OnCleanupEvent += OnEntityCleanup;
            OnSuccess?.Invoke(this);
        }

        public void OnEntityCleanup(ICleanableEventDispatcher source)
        {
            Unload();
        }

        public override void Unload()
        {
            loadHelper.Unload();
            this.entity.OnCleanupEvent -= OnEntityCleanup;
        }

        public void OnDestroy()
        {
            if (Application.isPlaying)
            {
                Unload();
            }
        }
    }
}
