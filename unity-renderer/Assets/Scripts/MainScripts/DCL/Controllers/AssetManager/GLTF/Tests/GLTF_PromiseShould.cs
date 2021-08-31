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

    }
}