namespace DCL
{
    public class PromiseLike_Texture : IPromiseLike_TextureAsset
    {
        public ITexture asset => promise.asset;

        private readonly AssetPromise_Texture promise;

        public PromiseLike_Texture(AssetPromise_Texture promiseTexture)
        {
            promise = promiseTexture;
        }

        public void Forget()
        {
            AssetPromiseKeeper_Texture.i.Forget(promise);
        }
    }

    public class PromiseLike_Gif : IPromiseLike_TextureAsset
    {
        public ITexture asset => promise.asset;

        private readonly AssetPromise_Gif promise;

        public PromiseLike_Gif(AssetPromise_Gif promiseGif)
        {
            promise = promiseGif;
        }

        public void Forget()
        {
            AssetPromiseKeeper_Gif.i.Forget(promise);
        }
    }
}