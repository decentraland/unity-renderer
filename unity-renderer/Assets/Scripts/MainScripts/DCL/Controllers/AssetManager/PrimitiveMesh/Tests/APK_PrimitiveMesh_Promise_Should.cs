using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace AssetPromiseKeeper_PrimitiveMesh_Tests
{
    public class APK_PrimitiveMesh_Promise_Should : TestsBase_APK<AssetPromiseKeeper_PrimitiveMesh,
        AssetPromise_PrimitiveMesh,
        Asset_PrimitiveMesh,
        AssetLibrary_RefCounted<Asset_PrimitiveMesh>>
    {
        protected AssetPromise_PrimitiveMesh CreatePromise()
        {
            PrimitiveMeshModel model = new PrimitiveMeshModel(PrimitiveMeshModel.Type.Box);
            var prom = new AssetPromise_PrimitiveMesh(model);
            return prom;
        }
        
        protected AssetPromise_PrimitiveMesh CreatePromise(PrimitiveMeshModel model)
        {
            var prom = new AssetPromise_PrimitiveMesh(model);
            return prom;
        }

        [UnityTest]
        public IEnumerator BeSetupCorrectlyAfterLoad()
        {
            Asset_PrimitiveMesh loadedAsset = null;
            var prom = CreatePromise();

            prom.OnSuccessEvent += (x) => loadedAsset = x;

            keeper.Keep(prom);

            yield return prom;

            Assert.IsNotNull(loadedAsset);
            Assert.IsNotNull(loadedAsset.mesh);
            
            loadedAsset = null;
            prom = CreatePromise();

            prom.OnSuccessEvent += (x) => loadedAsset = x;

            keeper.Keep(prom);

            yield return prom;

            Assert.IsNotNull(loadedAsset);
            Assert.IsNotNull(loadedAsset.mesh);
        }

        [UnityTest]
        public IEnumerator ShareMeshAmongPromisesWithSameSettings()
        {
            Asset_PrimitiveMesh loadedAsset = null;
            var prom = CreatePromise();

            prom.OnSuccessEvent += (x) => loadedAsset = x;

            keeper.Keep(prom);
            yield return prom;

            Asset_PrimitiveMesh loadedAsset2 = null;
            var prom2 = CreatePromise();

            prom2.OnSuccessEvent += (x) => loadedAsset2 = x;

            keeper.Keep(prom2);
            yield return prom2;

            Assert.IsNotNull(loadedAsset);
            Assert.IsNotNull(loadedAsset.mesh);
            Assert.IsNotNull(loadedAsset2);
            Assert.IsNotNull(loadedAsset2.mesh);

            Assert.IsTrue(loadedAsset.mesh == loadedAsset2.mesh);
        }

        [UnityTest]
        public IEnumerator NotShareMeshAmongPromisesWithDifferentSettings()
        {
            PrimitiveMeshModel model = new PrimitiveMeshModel(PrimitiveMeshModel.Type.Box);
            PrimitiveMeshModel model2 = new PrimitiveMeshModel(PrimitiveMeshModel.Type.Box);
            float[] uvs = new float[]
            {
                0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1,
                0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1,
                0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1,
                0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1,
                0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1,
                0, 0.75f, 0.25f, 0.75f, 0.25f, 1, 0, 1
            };
            model2.uvs.Add(uvs.ToList());
            Asset_PrimitiveMesh loadedAsset = null;
            var prom = CreatePromise(model);

            prom.OnSuccessEvent += (x) => loadedAsset = x;

            keeper.Keep(prom);
            yield return prom;

            Asset_PrimitiveMesh loadedAsset2 = null;
            var prom2 = CreatePromise(model2);

            prom2.OnSuccessEvent += (x) => loadedAsset2 = x;

            keeper.Keep(prom2);
            yield return prom2;

            Assert.IsNotNull(loadedAsset);
            Assert.IsNotNull(loadedAsset.mesh);
            Assert.IsNotNull(loadedAsset2);
            Assert.IsNotNull(loadedAsset2.mesh);

            Assert.IsTrue(loadedAsset.mesh != loadedAsset2.mesh);
        }
    }
}