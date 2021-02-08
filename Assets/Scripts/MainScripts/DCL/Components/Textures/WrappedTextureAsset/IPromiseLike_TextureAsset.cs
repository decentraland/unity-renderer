namespace DCL
{
    public interface IPromiseLike_TextureAsset
    {
        ITexture asset { get; }
        void Forget();
    }
}