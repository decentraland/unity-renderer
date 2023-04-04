using GLTFast;
using System;
using GLTFast.Loading;

namespace DCL.GLTFast.Wrappers
{
    internal class GLTFastTexturePromiseWrapper : ITextureDownload
    {
        private readonly AssetPromiseKeeper_Texture texturePromiseKeeper;
        private AssetPromise_Texture assetPromiseTexture;

        public GLTFastTexturePromiseWrapper(AssetPromiseKeeper_Texture texturePromiseKeeper, AssetPromise_Texture assetPromiseTexture)
        {
            this.texturePromiseKeeper = texturePromiseKeeper;
            this.assetPromiseTexture = assetPromiseTexture;

            this.assetPromiseTexture.OnSuccessEvent += OnSuccess;
            assetPromiseTexture.OnFailEvent += OnFail;
        }

        public void Dispose()
        {
            if (assetPromiseTexture == null) return;

            assetPromiseTexture.OnSuccessEvent -= OnSuccess;
            assetPromiseTexture.OnFailEvent -= OnFail;

            assetPromiseTexture = null;
        }

        public bool Success { get; private set; }

        public string Error { get; private set; }

        public byte[] Data => Array.Empty<byte>();

        public string Text => string.Empty;

        public bool? IsBinary => true;

        public IDisposableTexture GetTexture(bool forceSampleLinear) =>
            new GLTFastDisposableDisposableTexturePromise(assetPromiseTexture, texturePromiseKeeper);

        private void OnSuccess(Asset_Texture assetTexture)
        {
            Success = true;
        }

        private void OnFail(Asset_Texture arg1, Exception arg2)
        {
            Error = arg2.Message;
        }
    }
}
