namespace DCL
{
    public class AssetPromiseKeeper_AudioClip : AssetPromiseKeeper<Asset_AudioClip, AssetLibrary_RefCounted<Asset_AudioClip>, AssetPromise_AudioClip>
    {
        private static AssetPromiseKeeper_AudioClip instance;
        public static AssetPromiseKeeper_AudioClip i { get { return instance ??= new AssetPromiseKeeper_AudioClip(); } }

        public AssetPromiseKeeper_AudioClip() : base(new AssetLibrary_RefCounted<Asset_AudioClip>()) { }
    }
}