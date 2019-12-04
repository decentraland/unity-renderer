using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_GLTF_Tests
{
    public class APK_GLTF_ShouldWorkWhen :
        APKWithPoolableAssetShouldWorkWhen_Base<AssetPromiseKeeper_GLTF,
                                                AssetPromise_GLTF,
                                                Asset_GLTF,
                                                AssetLibrary_GLTF>
    {
        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return InitScene();
            yield return base.SetUp();
        }

        protected override AssetPromise_GLTF CreatePromise()
        {
            string url = Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";
            var prom = new AssetPromise_GLTF(scene.contentProvider, url);
            return prom;
        }
    }
}
