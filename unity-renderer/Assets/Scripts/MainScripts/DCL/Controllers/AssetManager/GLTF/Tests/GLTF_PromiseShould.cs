using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Helpers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_GLTF_Tests
{
    public class GLTF_PromiseShould
    {
        [UnityTest]
        public IEnumerator GLTFShouldShareSameExternalTexture()
        {
            string baseUrl = TestAssetsUtils.GetPath() + "/GLTF/TextureSharingGLTF/";

            var provider = new ContentProvider
            {
                baseUrl = baseUrl,
                contents = new List<ContentServerUtils.MappingPair>()
                {
                    new ContentServerUtils.MappingPair() { file = baseUrl + "cube.gltf", hash = "cube.gltf" },
                    new ContentServerUtils.MappingPair() { file = baseUrl + "cylinder.gltf", hash = "cylinder.gltf" },
                    new ContentServerUtils.MappingPair() { file = baseUrl + "Test.png", hash = "Test.png" },
                }
            };
            provider.BakeHashes();

            var keeper = new AssetPromiseKeeper_GLTF();
            IWebRequestController webRequestController = WebRequestController.Create();

            AssetPromise_GLTF cubePromise = new AssetPromise_GLTF(provider, baseUrl + "cube.gltf", webRequestController);
            AssetPromise_GLTF cylinderPromise = new AssetPromise_GLTF(provider, baseUrl + "cylinder.gltf", webRequestController);

            keeper.Keep(cubePromise);
            keeper.Keep(cylinderPromise);

            yield return cubePromise;
            yield return cylinderPromise;

            Texture cubeTexture = cubePromise.asset.container.GetComponentInChildren<Renderer>().material.mainTexture;
            Texture cylinderTexture = cylinderPromise.asset.container.GetComponentInChildren<Renderer>().material.mainTexture;

            Assert.AreEqual(cubeTexture, cylinderTexture);
            keeper.Cleanup();
        }
        
        [UnityTest]
        public IEnumerator NotAddAssetToLibraryMoreThanOnce()
        {
            var keeper = new AssetPromiseKeeper_GLTF();
            IWebRequestController webRequestController = WebRequestController.Create();
            
            string url = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb";
            var provider = new ContentProvider_Dummy();
            
            AssetPromise_GLTF promise1 = new AssetPromise_GLTF(provider, url, webRequestController);
            keeper.Keep(promise1);
            keeper.Forget(promise1);
            
            AssetPromise_GLTF promise2 = new AssetPromise_GLTF(provider, url, webRequestController);
            keeper.Keep(promise2);
            
            yield return promise2;
            yield return promise1;

            var pool = PoolManager.i.GetPool(keeper.library.AssetIdToPoolId(promise1.GetId()));
            
            Assert.AreEqual(2, pool.objectsCount);
            keeper.Cleanup();
        }

    }
}