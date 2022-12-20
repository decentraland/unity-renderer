using DCL.UIElements.Image;

namespace DCL.ECSComponents.Utils
{
    public struct UITextureUpdater
    {
        private Texture lastTexture;
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

        public void Update(Texture texture)
        {
            if (Equals(lastTexture, texture))
                return;

            lastTexture = texture;
            var prevPromise = lastPromise;

            lastPromise = new AssetPromise_Texture(texture.Src, texture.GetWrapMode(), texture.GetFilterMode());
            lastPromise.OnSuccessEvent += OnTextureDownloaded;
            texturePromiseKeeper.Keep(lastPromise);
            texturePromiseKeeper.Forget(prevPromise);
        }

        private void OnTextureDownloaded(Asset_Texture texture)
        {
            target.Texture = texture.texture;
        }
    }
}
