using DCL;
using DCL.Helpers;

namespace AssetPromiseKeeper_Tests
{
    public class APKShouldWorkWhen_GLTF : 
        APKWithPoolableAssetShouldWorkWhen_Base< AssetPromiseKeeper_GLTF, 
                                                AssetPromise_GLTF, 
                                                Asset_GLTF, 
                                                AssetLibrary_GLTF>
    {
        protected override AssetPromise_GLTF CreatePromise()
        {
            string url = Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";
            var prom = new AssetPromise_GLTF(scene.contentProvider, url);
           
            return prom;
        }
    }
}