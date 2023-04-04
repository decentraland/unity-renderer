using GLTFast;
using System;
using UnityEngine;

namespace DCL.GLTFast.Wrappers
{
    public class GLTFastDisposableDisposableTexturePromise : IDisposableTexture
    {
        private readonly AssetPromise_Texture promise;
        private readonly AssetPromiseKeeper_Texture assetPromiseKeeper;

        private bool isDisposed;

        public GLTFastDisposableDisposableTexturePromise(AssetPromise_Texture promise, AssetPromiseKeeper_Texture assetPromiseKeeper)
        {
            this.promise = promise;
            this.assetPromiseKeeper = assetPromiseKeeper;
        }
        public Texture2D Texture => isDisposed ? throw new ObjectDisposedException(nameof(AssetPromise_Texture)) : promise.asset.texture;

        public void Dispose()
        {
            if (isDisposed) return;

            assetPromiseKeeper.Forget(promise);

            isDisposed = true;
        }
    }
}
