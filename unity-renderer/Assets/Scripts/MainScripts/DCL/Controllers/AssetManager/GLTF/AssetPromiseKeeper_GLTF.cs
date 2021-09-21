namespace DCL
{
    public class AssetPromiseKeeper_GLTF : AssetPromiseKeeper<Asset_GLTF, AssetLibrary_GLTF, AssetPromise_GLTF>
    {
        private static AssetPromiseKeeper_GLTF instance;
        public static AssetPromiseKeeper_GLTF i { get { return instance ??= new AssetPromiseKeeper_GLTF(); } }

        public AssetPromiseKeeper_GLTF() : base(new AssetLibrary_GLTF()) { }

        protected override void OnSilentForget(AssetPromise_GLTF promise)
        {
            promise.OnSilentForget();
        }

        public override AssetPromise_GLTF Forget(AssetPromise_GLTF promise)
        {
            if (promise == null)
                return null;

            if (promise.state == AssetPromiseState.LOADING)
            {
                object id = promise.GetId();

                bool isFirstPromise = masterPromiseById.ContainsKey(id) && masterPromiseById[id] == promise;
                bool hasBlockedPromises = masterToBlockedPromises.ContainsKey(id) && masterToBlockedPromises[id].Count > 0;
                bool isMasterPromise = isFirstPromise && hasBlockedPromises;

                promise.OnForget();

                if (!isMasterPromise && promise.CanCancel())
                {
                    promise.Cancel();
                    return base.Forget(promise);
                }

                if (!isMasterPromise && promise.CanForget())
                {
                    return base.Forget(promise);
                }

                promise.OnSilentForget();
                return promise;
            }

            return base.Forget(promise);
        }
    }
}