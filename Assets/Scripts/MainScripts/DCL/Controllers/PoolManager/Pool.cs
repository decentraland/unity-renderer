using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DCL.Helpers;

namespace DCL
{
    public interface IPooledObjectInstantiator
    {
        bool IsValid(GameObject original);
        GameObject Instantiate(GameObject gameObject);
    }

    public class Pool : ICleanable
    {
        public object id;
        public GameObject original;
        public GameObject container;

        public System.Action<Pool> OnReleaseAll;
        public System.Action<Pool> OnCleanup;

        public IPooledObjectInstantiator instantiator;

        private readonly List<PoolableObject> inactiveObjects = new List<PoolableObject>();
        private readonly List<PoolableObject> activeObjects = new List<PoolableObject>();

        public float lastGetTime
        {
            get;
            private set;
        }

        public int objectsCount => inactiveCount + activeCount;

        public int inactiveCount
        {
            get { return inactiveObjects.Count; }
        }

        public int activeCount
        {
            get { return activeObjects.Count; }
        }

        public Pool(string name)
        {
            container = new GameObject("Pool - " + name);
        }


        public PoolableObject Get()
        {
            PoolableObject poolable = null;

            if (inactiveObjects.Count > 0)
                poolable = inactiveObjects[0];
            else
                poolable = Instantiate();

            EnablePoolableObject(poolable);

            return poolable;
        }

        private PoolableObject Instantiate()
        {
            GameObject gameObject = null;

            if (instantiator != null)
                gameObject = instantiator.Instantiate(original);
            else
                gameObject = GameObject.Instantiate(original);

            return SetupPoolableObject(gameObject);
        }

        private PoolableObject SetupPoolableObject(GameObject gameObject, bool active = false)
        {
            if (gameObject.GetComponent<PoolableObject>() != null)
                return null;

            PoolableObject poolable = gameObject.AddComponent<PoolableObject>();
            poolable.pool = this;

            if (!active)
            {
                DisablePoolableObject(poolable);
            }
            else
            {
                EnablePoolableObject(poolable);
            }

            return poolable;
        }

        public void Release(PoolableObject poolable)
        {
#if UNITY_EDITOR
            if (isQuitting)
                return;
#endif

            if (poolable == null || inactiveObjects.Contains(poolable))
                return;

            DisablePoolableObject(poolable);
#if UNITY_EDITOR
            RefreshName();
#endif
        }

        public void ReleaseAll()
        {
            OnReleaseAll?.Invoke(this);
        }

        /// <summary>
        /// This will add a gameObject that is not on any pool to this pool.
        /// </summary>
        /// <param name="gameObject"></param>
        public void AddToPool(GameObject gameObject, bool addActive = true)
        {
            if (instantiator != null && !instantiator.IsValid(gameObject))
            {
                Debug.LogError($"ERROR: Trying to add invalid gameObject to pool! -- {gameObject.name}", gameObject);
                return;
            }

            PoolableObject obj = gameObject.GetComponent<PoolableObject>();

            if (obj != null)
            {
                Debug.LogError($"ERROR: gameObject is already being tracked by a pool! -- {gameObject.name}", gameObject);
                return;
            }

            SetupPoolableObject(gameObject, addActive);
        }

        public void RemoveFromPool(PoolableObject poolable)
        {
            if (inactiveObjects.Contains(poolable))
                inactiveObjects.Remove(poolable);

            if (activeObjects.Contains(poolable))
                activeObjects.Remove(poolable);

#if UNITY_EDITOR
            RefreshName();
#endif
        }

        public void Cleanup()
        {
            ReleaseAll();

            {
                int count = inactiveObjects.Count;

                for (int i = 0; i < count; i++)
                {
                    if (inactiveObjects[i])
                        Object.Destroy(inactiveObjects[i].gameObject);
                }
            }

            {
                int count = activeObjects.Count;

                for (int i = 0; i < count; i++)
                {
                    if (activeObjects[i])
                        Object.Destroy(activeObjects[i].gameObject);
                }
            }

            inactiveObjects.Clear();
            activeObjects.Clear();

            Object.Destroy(this.original);
            Object.Destroy(this.container);

            OnCleanup?.Invoke(this);

        }

        public void EnablePoolableObject(PoolableObject poolable)
        {
            if (poolable.gameObject != null)
            {
                poolable.gameObject.SetActive(true);
                poolable.gameObject.transform.SetParent(null);
            }

            lastGetTime = Time.realtimeSinceStartup;

            if (inactiveObjects.Contains(poolable))
                inactiveObjects.Remove(poolable);

            if (!activeObjects.Contains(poolable))
                activeObjects.Add(poolable);
#if UNITY_EDITOR
            RefreshName();
#endif
        }

        public void DisablePoolableObject(PoolableObject poolable)
        {
#if UNITY_EDITOR
            if (isQuitting)
                return;
#endif
            if (poolable.gameObject != null)
            {
                poolable.gameObject.SetActive(false);

                if (container != null && container.transform != null)
                {
                    poolable.gameObject.transform.SetParent(container.transform);
                    poolable.gameObject.transform.ResetLocalTRS();
                }
            }

            if (!inactiveObjects.Contains(poolable))
                inactiveObjects.Add(poolable);

            if (activeObjects.Contains(poolable))
                activeObjects.Remove(poolable);

#if UNITY_EDITOR
            RefreshName();
#endif
        }

        private void RefreshName()
        {
            if (this.container != null)
                this.container.name = $"in: {inactiveCount} out: {activeCount} id: {id}";
        }

        public static bool FindPoolInGameObject(GameObject gameObject, out Pool pool)
        {
            pool = null;
            var poolable = gameObject.GetComponentInChildren<PoolableObject>(true);

            if (poolable != null)
            {
                if (poolable.pool.activeObjects.Contains(poolable))
                {
                    pool = poolable.pool;
                    return true;
                }

                if (poolable.pool.inactiveObjects.Contains(poolable))
                {
                    pool = poolable.pool;
                    return true;
                }
            }

            return false;
        }

#if UNITY_EDITOR
        // In production it will always be false
        private bool isQuitting = false;


        // We need to check if application is quitting in editor
        // to prevent the pool from releasing objects that are
        // being destroyed 
        void Awake()
        {
            Application.quitting += OnIsQuitting;
        }

        void OnIsQuitting()
        {
            Application.quitting -= OnIsQuitting;
            isQuitting = true;
        }
#endif
    }
};
