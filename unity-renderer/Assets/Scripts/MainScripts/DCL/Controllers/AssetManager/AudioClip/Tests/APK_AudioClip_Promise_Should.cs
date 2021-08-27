using System.Collections;
using AssetPromiseKeeper_Tests;
using DCL;
using DCL.Helpers;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_AudioClip_Tests
{
    public class APK_AudioClip_Promise_Should : TestsBase_APK<AssetPromiseKeeper_AudioClip,
        AssetPromise_AudioClip,
        Asset_AudioClip,
        AssetLibrary_RefCounted<Asset_AudioClip>>
    {
        protected AssetPromise_AudioClip CreatePromise()
        {
            string url = TestAssetsUtils.GetPath() + "/Audio/short_effect.ogg";
            var prom = new AssetPromise_AudioClip(url, new ContentProvider_Dummy());
            return prom;
        }

        [UnityTest]
        public IEnumerator ShareAudioClipAmongPromises()
        {
            Asset_AudioClip[] assets = new Asset_AudioClip[] { null, null };
            AssetPromise_AudioClip[] promises = new[] { CreatePromise(), CreatePromise() };

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
                Assert.IsNotNull(assets[i].audioClip);
            }

            Assert.IsTrue(assets[0].audioClip == assets[1].audioClip);
        }
    }
}