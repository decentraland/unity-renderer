using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace DCL
{
    public class AssetPromise_Mock : AssetPromise<Asset_Mock>
    {
        public float loadTime;
        public object idGenerator;
        private object id;
        Coroutine assetMockCoroutine;

        internal override object GetId()
        {
            Assert.IsTrue(idGenerator != null, "idGenerator should not be null");

            if (id == null)
                id = idGenerator.GetHashCode();

            return id;
        }

        protected override void OnLoad(Action OnSuccess, Action OnFail)
        {
            loadTime = 1;
            assetMockCoroutine = CoroutineStarter.Start(MockLoadingCoroutine(OnSuccess));
        }

        protected override void OnCancelLoading()
        {
            CoroutineStarter.Stop(assetMockCoroutine);
        }

        IEnumerator MockLoadingCoroutine(Action OnSuccess)
        {
            yield return new WaitForSeconds(loadTime);
            OnSuccess?.Invoke();
        }


        //NOTE(Brian): Used for testing
        public void Load_Test()
        {
            Load();
        }

        public Asset_Mock GetAsset_Test()
        {
            return asset;
        }

        public void Unload_Test()
        {
            Unload();
        }

        public void SetLibrary_Test(AssetLibrary<Asset_Mock> library)
        {
            this.library = library;
        }

        protected override void ApplySettings_LoadStart()
        {
        }

        protected override void ApplySettings_LoadFinished()
        {
        }
    }

    public class AssetPromise_Mock_Alt_Loading_Approach : AssetPromise_Mock
    {
    }

}
