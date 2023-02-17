using DCL;

namespace MainScripts.DCL.Controllers.AssetManager.AssetBundles.SceneAB
{
    public class AssetPromiseKeeper_SceneAB : AssetPromiseKeeper<Asset_SceneAB, AssetLibrary_RefCounted<Asset_SceneAB>, AssetPromise_SceneAB>
    {
        private static AssetPromiseKeeper_SceneAB instance;
        public static AssetPromiseKeeper_SceneAB i { get { return instance ??= new AssetPromiseKeeper_SceneAB(new AssetLibrary_RefCounted<Asset_SceneAB>()); } }
        public AssetPromiseKeeper_SceneAB(AssetLibrary_RefCounted<Asset_SceneAB> library) : base(library)
        {

        }
    }
}
