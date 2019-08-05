using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace DCL
{
    public class PoolManager : MonoBehaviour
    {
        public Dictionary<object, Pool> pools = new Dictionary<object, Pool>();
        private static PoolManager instance = null;

        public static PoolManager i
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<PoolManager>();

                    if (instance == null)
                        Debug.LogError("PoolManager instance doesn't exist.");
                }

                return instance;
            }
        }

        public static bool IsAvailable()
        {
            if (instance)
                return true;

            return false;
        }

        void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(this.gameObject);
        }

        public PoolableObject Get<T>(object id, GameObject original, T instantiator) where T : IPooledObjectInstantiator
        {
            Pool pool;

            if (pools.ContainsKey(id))
                pool = pools[id];
            else
                pool = AddPool<T>(id, original, instantiator);

            return pool.Get<T>(instantiator);
        }

        public PoolableObject Get(object id, GameObject original)
        {
            Pool pool;

            if (pools.ContainsKey(id))
                pool = pools[id];
            else
                pool = AddPool(id, original);

            return pool.Get();
        }

        public PoolableObject GetIfPoolExists(object id)
        {
            if (pools.ContainsKey(id))
            {
                return pools[id].Get();
            }

            return null;
        }

        public void ReleaseAll(object id)
        {
            if (pools.ContainsKey(id))
            {
                pools[id].ReleaseAll();
            }
        }

        public void CleanupPool(object id)
        {
            if (pools.ContainsKey(id))
            {
                pools[id].Cleanup();
                pools.Remove(id);
            }
        }

        public bool ContainsPool(object id)
        {
            return pools.ContainsKey(id);
        }

        public Pool GetPool(object id)
        {
            if (pools.ContainsKey(id))
                return pools[id];

            return null;
        }

        public Pool AddPool<T>(object id, GameObject original, T instantiator) where T : IPooledObjectInstantiator
        {
            GameObject originalGo = instantiator.Instantiate(original);
            Pool pool = SetupAndAddPool(id, originalGo);

            return pool;
        }

        public Pool AddPool(object id, GameObject original)
        {
            GameObject originalGo = GameObject.Instantiate(original);
            Pool pool = SetupAndAddPool(id, originalGo);

            return pool;
        }

        private Pool SetupAndAddPool(object id, GameObject original)
        {
            GameObject poolGo = new GameObject(id.ToString());

            poolGo.transform.parent = this.transform;
            Pool pool = poolGo.AddComponent<Pool>();

            pool.original = original;
            pool.original.name = "Original";
            pool.original.transform.parent = pool.transform;
            pool.original.SetActive(false);

            pool.id = id;

            pools.Add(id, pool);

            return pool;
        }

    }
}
