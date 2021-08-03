using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class AssetLibrary_Poolable<AssetType> : AssetLibrary<AssetType>
        where AssetType : Asset_WithPoolableContainer
    {
        private readonly string salt = null;
        IPooledObjectInstantiator instantiator;
        public Dictionary<object, AssetType> masterAssets = new Dictionary<object, AssetType>();

        public AssetLibrary_Poolable(IPooledObjectInstantiator instantiator) : this(instantiator, null) { }
        public AssetLibrary_Poolable(IPooledObjectInstantiator instantiator, string salt)
        {
            this.instantiator = instantiator;
            this.salt = salt;
        }

        private void OnPoolRemoved(Pool pool)
        {
            pool.OnCleanup -= OnPoolRemoved;
            object assetId = PoolIdToAssetId(pool.id);

            if (masterAssets.ContainsKey(assetId))
            {
                masterAssets[assetId].Cleanup();
                masterAssets.Remove(assetId);
            }
        }

        public override bool Add(AssetType asset)
        {
            if (asset == null || asset.container == null)
            {
                Debug.LogWarning("asset or asset.container == null? This shouldn't happen");
                return false;
            }

            if (!masterAssets.ContainsKey(asset.id))
                masterAssets.Add(asset.id, asset);

            object poolId = AssetIdToPoolId(asset.id);
            Pool pool = PoolManager.i.AddPool(poolId, asset.container, instantiator);

            pool.OnCleanup -= OnPoolRemoved;
            pool.OnCleanup += OnPoolRemoved;

            if (asset.container == null)
                return false;

            return true;
        }

        public override AssetType Get(object id)
        {
            if (!Contains(id))
                return null;

            AssetType clone = masterAssets[id].Clone() as AssetType;
            object poolId = AssetIdToPoolId(id);

            if (PoolManager.i.ContainsPool(poolId))
            {
                clone.container = PoolManager.i.Get(poolId).gameObject;
            }
            else
            {
                Debug.LogError("Pool was removed and AssetLibrary didn't notice?!");
                return null;
            }

            return clone;
        }

        public AssetType GetCopyFromOriginal(object id)
        {
            if (!Contains(id))
                return null;

            AssetType clone = masterAssets[id].Clone() as AssetType;
            object poolId = AssetIdToPoolId(id);

            if (PoolManager.i.ContainsPool(poolId))
            {
                clone.container = PoolManager.i.GetPool(poolId).InstantiateAsOriginal();
            }
            else
            {
                Debug.LogError("Pool was removed and AssetLibrary didn't notice?!");
                return null;
            }

            return clone;
        }

        public override void Release(AssetType asset)
        {
            if (!Contains(asset))
            {
                Debug.LogError("ERROR: Trying to release an asset not added to this library!");
                return;
            }

            if (asset == null)
            {
                Debug.LogError("ERROR: Trying to release null asset");
                return;
            }

            PoolManager.i.Release(asset.container);
        }

        public override bool Contains(object id)
        {
            if (masterAssets == null)
                return false;

            return masterAssets.ContainsKey(id);
        }

        public override bool Contains(AssetType asset)
        {
            if (asset == null)
                return false;

            return Contains(asset.id);
        }

        public override void Cleanup()
        {
            foreach (var kvp in masterAssets)
            {
                kvp.Value.Cleanup();
            }

            masterAssets.Clear();
        }

        public object AssetIdToPoolId(object assetId) { return salt == null ? assetId : $"{assetId}{GetSaltSuffix()}"; }

        public object PoolIdToAssetId(object poolId)
        {
            if (salt == null)
                return poolId;

            string id = poolId.ToString();
            string saltSuffix = GetSaltSuffix();
            if (id.EndsWith(saltSuffix))
            {
                return id.Remove(id.Length - saltSuffix.Length);
            }
            return poolId;
        }

        string GetSaltSuffix() { return  $"_{salt}"; }
    }
}