using System.Collections;
using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_Material_Tests
{
    public class APK_Material_Promise_Should : TestsBase_APK<AssetPromiseKeeper_Material,
        AssetPromise_Material,
        Asset_Material,
        AssetLibrary_RefCounted<Asset_Material>>
    {
        protected AssetPromise_Material CreatePromise()
        {
            AssetPromise_Material_Model.Texture model = CreateTextureModel();
            var prom = new AssetPromise_Material(CreateMaterialModel(model));
            return prom;
        }

        protected AssetPromise_Material CreatePromise(TextureWrapMode wrapmode, FilterMode filterMode)
        {
            AssetPromise_Material_Model.Texture model = CreateTextureModel(wrapmode,filterMode);
            AssetPromise_Material prom = new AssetPromise_Material(CreateMaterialModel(model));

            return prom;
        }
        
        private AssetPromise_Material_Model CreateMaterialModel(AssetPromise_Material_Model.Texture texture)
        {
            return AssetPromise_Material_Model.CreateBasicMaterial(texture, 1f, Color.white);
        }

        protected AssetPromise_Material_Model.Texture CreateTextureModel(TextureWrapMode wrapmode = TextureWrapMode.Clamp, 
            FilterMode filterMode = FilterMode.Bilinear)
        {
            return new AssetPromise_Material_Model.Texture(TestAssetsUtils.GetPath() + "/Images/atlas.png", wrapmode, filterMode);
        }

        [UnityTest]
        public IEnumerator BeSetupCorrectlyAfterLoad()
        {
            // Check non-default-settings texture
            Asset_Material loadedAsset = null;
            var prom = CreatePromise(TextureWrapMode.Clamp, FilterMode.Trilinear);

            prom.OnSuccessEvent += (x) => loadedAsset = x;

            keeper.Keep(prom);

            yield return prom;

            Assert.IsNotNull(loadedAsset);
            Assert.IsNotNull(loadedAsset.material);

            // Check default texture
            loadedAsset = null;
            prom = CreatePromise();

            prom.OnSuccessEvent += (x) => loadedAsset = x;

            keeper.Keep(prom);

            yield return prom;

            Assert.IsNotNull(loadedAsset);
        }

        [UnityTest]
        public IEnumerator ShareTextureAmongPromisesWithSameSettings()
        {
            // 2 non-default textures
            Asset_Material loadedAsset = null;
            var prom = CreatePromise(TextureWrapMode.Clamp, FilterMode.Trilinear);

            prom.OnSuccessEvent += (x) => loadedAsset = x;

            keeper.Keep(prom);
            yield return prom;

            Asset_Material loadedAsset2 = null;
            var prom2 = CreatePromise(TextureWrapMode.Clamp, FilterMode.Trilinear);

            prom2.OnSuccessEvent += (x) => loadedAsset2 = x;

            keeper.Keep(prom2);
            yield return prom2;

            Assert.IsNotNull(loadedAsset);
            Assert.IsNotNull(loadedAsset.material);
            Assert.IsNotNull(loadedAsset2);
            Assert.IsNotNull(loadedAsset2.material);

            Assert.IsTrue(loadedAsset.material == loadedAsset2.material);

            // 2 default textures
            Asset_Material loadedAsset3 = null;
            var prom3 = CreatePromise(TextureWrapMode.Clamp, FilterMode.Trilinear);

            prom3.OnSuccessEvent += (x) => loadedAsset3 = x;

            keeper.Keep(prom3);
            yield return prom3;

            Asset_Material loadedAsset4 = null;
            var prom4 = CreatePromise(TextureWrapMode.Clamp, FilterMode.Trilinear);

            prom4.OnSuccessEvent += (x) => loadedAsset4 = x;

            keeper.Keep(prom4);
            yield return prom4;

            Assert.IsNotNull(loadedAsset3);
            Assert.IsNotNull(loadedAsset3.material);
            Assert.IsNotNull(loadedAsset4);
            Assert.IsNotNull(loadedAsset4.material);

            Assert.IsTrue(loadedAsset3.material == loadedAsset4.material);
        }
        
        [UnityTest]
        public IEnumerator KeepRefCountCorrectly()
        {
            var model = CreateMaterialModel(CreateTextureModel(TextureWrapMode.Clamp, FilterMode.Trilinear));
            var prom = new AssetPromise_Material(model);
            keeper.Keep(prom);
            yield return prom;

            Assert.AreEqual(1, keeper.library.masterAssets[model].referenceCount);

            var prom2 = new AssetPromise_Material(model);
            keeper.Keep(prom2);
            yield return prom2;

            Assert.AreEqual(2, keeper.library.masterAssets[model].referenceCount);

            keeper.Forget(prom);
            Assert.AreEqual(1, keeper.library.masterAssets[model].referenceCount);

            prom = new AssetPromise_Material(model);
            keeper.Keep(prom);
            yield return prom;

            Assert.AreEqual(2, keeper.library.masterAssets[model].referenceCount);
            keeper.Forget(prom);
            keeper.Forget(prom2);

            Assert.AreEqual(0, keeper.library.masterAssets.Count);
        }        
    }
}