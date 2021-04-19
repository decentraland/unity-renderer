using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class AssetLibrary_Poolable<AssetType> : AssetLibrary<AssetType>
        where AssetType : Asset_WithPoolableContainer
    {
        IPooledObjectInstantiator instantiator;
        public Dictionary<object, AssetType> masterAssets = new Dictionary<object, AssetType>();

        public AssetLibrary_Poolable(IPooledObjectInstantiator instantiator)
        {
            this.instantiator = instantiator;
        }

        private void OnPoolRemoved(Pool pool)
        {
            pool.OnCleanup -= OnPoolRemoved;
            if (masterAssets.ContainsKey(pool.id))
            {
                masterAssets[pool.id].Cleanup();
                masterAssets.Remove(pool.id);
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

            Pool pool = PoolManager.i.AddPool(asset.id, asset.container, instantiator);

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

            if (PoolManager.i.ContainsPool(clone.id))
            {
                clone.container = PoolManager.i.Get(clone.id).gameObject;
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

            if (PoolManager.i.ContainsPool(clone.id))
            {
                clone.container = PoolManager.i.GetPool(id).InstantiateAsOriginal();
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
    }
}
