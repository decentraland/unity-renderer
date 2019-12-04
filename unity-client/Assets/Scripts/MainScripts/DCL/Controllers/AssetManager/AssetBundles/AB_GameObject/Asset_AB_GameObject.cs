using UnityEngine;

namespace DCL
{
    public class Asset_AB_GameObject : Asset_WithPoolableContainer
    {
        internal AssetPromise_AB ownerPromise;
        public override GameObject container { get; set; }
        public bool isInstantiated;

        public Asset_AB_GameObject()
        {
            isInstantiated = false;
            container = new GameObject("AB Container");
        }

        public override void Cleanup()
        {
            AssetPromiseKeeper_AB.i.Forget(ownerPromise);
            Object.Destroy(container);
        }

        public void Show(System.Action OnFinish)
        {
            OnFinish?.Invoke();
        }

        public void Hide()
        {
            container.transform.parent = null;
            container.transform.position = Vector3.one * 5000;
        }
    }
}
