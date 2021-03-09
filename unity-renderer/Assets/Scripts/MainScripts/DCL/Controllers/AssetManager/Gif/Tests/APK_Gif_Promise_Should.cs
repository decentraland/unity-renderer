using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_Gif_Tests
{
    public class APK_Gif_Promise_Should : TestsBase_APK<AssetPromiseKeeper_Gif,
                                                            AssetPromise_Gif,
                                                            Asset_Gif,
                                                            AssetLibrary_RefCounted<Asset_Gif>>
    {
        protected AssetPromise_Gif CreatePromise()
        {
            string url = Utils.GetTestsAssetsPath() + "/Images/gif1.gif";
            var prom = new AssetPromise_Gif(url);
            return prom;
        }

        [UnityTest]
        public IEnumerator ShareGifAmongPromises()
        {
            Asset_Gif[] assets = new Asset_Gif [] {null, null};
            AssetPromise_Gif[] promises = new AssetPromise_Gif [] {CreatePromise(), CreatePromise()};

            promises[0].OnSuccessEvent += (x) => assets[0] = x;
            promises[1].OnSuccessEvent += (x) => assets[1] = x;

            for (int i = 0; i < promises.Length; i++)
            {
                keeper.Keep(promises[i]);
                yield return promises[i];
            }

            for (int i = 0; i < assets.Length; i++)
            {
                Assert.IsNotNull(assets[i]);
                Assert.IsNotNull(assets[i].texture);
            }

            Assert.IsTrue(assets[0].texture == assets[1].texture);
        }
    }
}