using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine.TestTools;
using UnityGLTF;

namespace AssetPromiseKeeper_GLTF_Tests
{
    public class APK_GLTF_ShouldWorkWhen :
        APKWithPoolableAssetShouldWorkWhen_Base<AssetPromiseKeeper_GLTF,
            AssetPromise_GLTF,
            Asset_GLTF,
            AssetLibrary_GLTF>
    {
        protected ContentProvider contentProvider;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            contentProvider = new ContentProvider();
            GLTFSceneImporter.budgetPerFrameInMilliseconds = float.MaxValue;
        }

        protected override AssetPromise_GLTF CreatePromise()
        {
            string url = Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";
            var prom = new AssetPromise_GLTF(contentProvider, url);
            return prom;
        }
    }
}