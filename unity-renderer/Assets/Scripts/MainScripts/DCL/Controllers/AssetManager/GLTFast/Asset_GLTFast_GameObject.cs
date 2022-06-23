using System;
using DCL.Configuration;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL
{
    public class Asset_GLTFast_GameObject : Asset_WithPoolableContainer
    {
        internal AssetPromise_GLTFast ownerPromise;
        public override GameObject container { get; set; }

        public Asset_GLTFast_GameObject()
        {
            container = new GameObject("GLTFast Container");
            container.transform.position = EnvironmentSettings.MORDOR;
        }
        
        public override void Cleanup()
        {
            AssetPromiseKeeper_GLTFast.i.Forget(ownerPromise);
            Object.Destroy(container);
        }
        
        public void Hide()
        {
            container.SetActive(false);
            container.transform.parent = null;
            container.transform.position = EnvironmentSettings.MORDOR;
        }

        public void Show(Action success)
        {
            container.SetActive(true);
            success?.Invoke();
        }
    }
}