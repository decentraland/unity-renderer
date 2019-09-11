using System.Collections.Generic;
using UnityEngine;

namespace DCL
{


    public class PoolManager : Singleton<PoolManager>
    {
        public Dictionary<object, Pool> pools = new Dictionary<object, Pool>();

        GameObject container
        {
            get
            {
                EnsureContainer();
                return containerValue;
            }

            set { containerValue = value; }
        }

        GameObject containerValue = null;

        void EnsureContainer()
        {
            if (containerValue == null)
                containerValue = new GameObject("_PoolManager");
        }

        public PoolManager()
        {
            EnsureContainer();
        }

        public Pool AddPool(object id, GameObject original, IPooledObjectInstantiator instantiator = null)
        {
            if (ContainsPool(id))
            {
                if (Pool.FindPoolInGameObject(original, out Pool poolAlreadyExisting))
                {
                    Debug.LogWarning("WARNING: Object is already being contained in an existing pool!. Returning it.");
                    return poolAlreadyExisting;
                }

                Pool result = GetPool(id);
                result.AddToPool(original);
                return result;
            }

            Pool pool = new Pool(id.ToString());

#if UNITY_EDITOR
            pool.container.transform.parent = container.transform;
#endif

            pool.original = original;
#if UNITY_EDITOR
            pool.original.name = "Original";
            pool.original.transform.parent = pool.container.transform;
#endif
            pool.original.SetActive(false);


            pool.instantiator = instantiator;
            pool.id = id;

            pools.Add(id, pool);

            return pool;
        }


        public Pool GetPool(object id)
        {
            if (id == null)
            {
                Debug.LogError("GetPool >>> id cannot be null!");
                return null;
            }

            if (pools.ContainsKey(id))
                return pools[id];

            return null;
        }

        public void RemovePool(object id)
        {
            if (id == null)
            {
                Debug.LogError("RemovePool >>> id cannot be null!");
                return;
            }

            if (pools.ContainsKey(id))
            {
                pools[id].Cleanup();
                pools.Remove(id);
            }
        }

        public bool ContainsPool(object id)
        {
            if (id == null)
            {
                Debug.LogError("ContainsPool >>> id cannot be null!");
                return false;
            }

            return pools.ContainsKey(id);
        }

        public bool Release(GameObject gameObject)
        {
            if (gameObject == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("Release >>> gameObject cannot be null!");
#endif
                return false;
            }

            var poolableObject = gameObject.GetComponent<PoolableObject>();

            if (poolableObject)
            {
                poolableObject.Release();
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogWarning("Release >>> poolable not found in object, destroying it instead!");
#endif
                Object.Destroy(gameObject);
                return false;
            }

            return true;
        }

        public PoolableObject Get(object id)
        {
            if (id == null)
            {
                Debug.LogError("Get >>> id cannot be null!");
                return null;
            }

            Pool pool;

            if (pools.ContainsKey(id))
            {
                pool = pools[id];
            }
            else
            {
                Debug.LogError($"Pool doesn't exist for id {id}!");
                return null;
            }

            return pool.Get();
        }

        public void Cleanup()
        {
            if (pools == null)
                return;

            List<object> idsToRemove = new List<object>(10);

            using (var iterator = pools.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    idsToRemove.Add(iterator.Current.Key);
                }
            }

            for (int i = 0; i < idsToRemove.Count; i++)
            {
                RemovePool(idsToRemove[i]);
            }
        }

        public void ReleaseAllFromPool(object id)
        {
            if (pools.ContainsKey(id))
            {
                pools[id].ReleaseAll();
            }
        }
    }
}
