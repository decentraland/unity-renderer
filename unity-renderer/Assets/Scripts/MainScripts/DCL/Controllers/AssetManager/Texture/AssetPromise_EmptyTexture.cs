using System;
using DCL.Helpers;
using MainScripts.DCL.Analytics.PerformanceAnalytics;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

namespace DCL
{
    public class AssetPromise_EmptyTexture : AssetPromise_Texture
    {
        protected override void OnLoad(Action OnSuccess, Action<Exception> OnFail)
        {
            asset.texture = new Texture2D(1, 1);
            OnSuccess?.Invoke();
        }

        public AssetPromise_EmptyTexture() : base("")
        { }
    }
}
