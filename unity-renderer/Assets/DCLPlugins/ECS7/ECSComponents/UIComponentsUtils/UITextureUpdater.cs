using DCL.Controllers;
using DCL.UIElements.Image;
using Decentraland.Common;
using JetBrains.Annotations;

namespace DCL.ECSComponents.Utils
{
    public struct UITextureUpdater
    {
        private TextureUnion lastTexture;

        private AssetPromise_Texture lastPromise;
        private readonly IUITextureConsumer target;
        private readonly AssetPromiseKeeper_Texture texturePromiseKeeper;

        public UITextureUpdater(IUITextureConsumer target, AssetPromiseKeeper_Texture texturePromiseKeeper)
        {
            this.target = target;
            this.texturePromiseKeeper = texturePromiseKeeper;
            lastPromise = null;
            lastTexture = null;
        }

        public void Update([CanBeNull] TextureUnion texture, IParcelScene parcelScene)
        {
            if (Equals(lastTexture, texture))
                return;

            lastTexture = texture;

            var prevPromise = lastPromise;
            texturePromiseKeeper.Forget(prevPromise);

            if (texture == null)
            {
                target.Texture = null;
                lastPromise = null;
            }
            else
            {
                lastPromise = new AssetPromise_Texture(texture.GetTextureUrl(parcelScene), texture.GetWrapMode(), texture.GetFilterMode());
                lastPromise.OnSuccessEvent += OnTextureDownloaded;
                texturePromiseKeeper.Keep(lastPromise);
            }
        }

        private void OnTextureDownloaded(Asset_Texture texture)
        {
            target.Texture = texture.texture;
        }

        public void Dispose()
        {
            texturePromiseKeeper.Forget(lastPromise);
        }
    }
}
