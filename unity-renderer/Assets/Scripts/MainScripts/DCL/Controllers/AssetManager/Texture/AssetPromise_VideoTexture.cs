using System;
using DCL.Helpers;
using MainScripts.DCL.Analytics.PerformanceAnalytics;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

namespace DCL
{
    public class AssetPromise_VideoTexture : AssetPromise_Texture
    {
        private static readonly Texture2D WHITE_TEXTURE = Texture2D.whiteTexture;
        private string hashId;

        public AssetPromise_VideoTexture(string hashId) : base(hashId)
        {
            this.hashId = hashId;
        }

        protected override void OnAfterLoadOrReuse() { }

        protected override void OnBeforeLoadOrReuse() { }

        protected override object GetLibraryAssetCheckId() { return hashId; }

        protected override void OnCancelLoading() { }

        protected override void OnLoad(Action OnSuccess, Action<Exception> OnFail)
        {
            asset.texture = WHITE_TEXTURE;
            OnSuccess?.Invoke();
        }

        protected override bool AddToLibrary()
        {
            asset = library.Get(asset.id);
            return true;
        }

        public override object GetId()
        {
            return hashId;
        }
    }
}
