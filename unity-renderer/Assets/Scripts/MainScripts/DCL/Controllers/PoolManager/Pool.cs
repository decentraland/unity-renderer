using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.Configuration;
using UnityEngine;
using DCL.Helpers;
using UnityEngine.Assertions;

namespace DCL
{
    public interface IPooledObjectInstantiator
    {
        bool IsValid(GameObject original);

        GameObject Instantiate(GameObject gameObject);
    }

    public class Pool : ICleanable
    {
        public delegate void OnReleaseAllDlg(Pool pool);

        public const int PREWARM_ACTIVE_MULTIPLIER = 2;
        public object id;
        public GameObject original;
        public GameObject container;

        public bool persistent = false;

        /// <summary>
        /// If this is set to true, all Unity components in the poolable gameObject implementing ILifecycleHandler
        /// will be registered and called when necessary.
        ///
        /// The interface call responsibility lies in the PoolableObject class.
        /// </summary>
        public bool useLifecycleHandlers = false;

        public System.Action<Pool> OnCleanup;

        public IPooledObjectInstantiator instantiator;

        private readonly LinkedList<PoolableObject> unusedObjects = new LinkedList<PoolableObject>();
        private readonly LinkedList<PoolableObject> usedObjects = new LinkedList<PoolableObject>();

        private readonly int maxPrewarmCount;

        private bool isInitialized;

        public float lastGetTime { get; private set; }

        public int objectsCount => unusedObjectsCount + usedObjectsCount;

        public int unusedObjectsCount => unusedObjects.Count;

        public int usedObjectsCount => usedObjects.Count;

        public Pool(string name, int maxPrewarmCount)
        {
            if (PoolManager.USE_POOL_CONTAINERS)
            {
                container = new GameObject("Pool - " + name);
                container.transform.position = EnvironmentSettings.MORDOR;
            }

            this.maxPrewarmCount = maxPrewarmCount;
        }

        public void ForcePrewarm(bool forceActive = true)
        {
            if (maxPrewarmCount <= objectsCount)
                return;

            Assert.IsTrue(original != null, $"Original should never be null here ({id})");
            var parent = PoolManager.USE_POOL_CONTAINERS && container != null ? container.transform : null;

            int objectsToInstantiate = Mathf.Max(0, maxPrewarmCount - objectsCount);

            for (var i = 0; i < objectsToInstantiate; i++)
                InstantiateOnPrewarm(parent, forceActive);
        }

        /// <summary>
        /// Lightweight version of Instantiate() that doesn't activate/deactivate the gameObject and parent it straight away.
        /// </summary>
        private void InstantiateOnPrewarm(Transform parent, bool forceActive = true)
        {
            GameObject gameObject = Object.Instantiate(original, parent);

            PoolableObject poolable = new PoolableObject(this, gameObject);
            PoolManager.i.poolables.Add(gameObject, poolable);
            PoolManager.i.poolableValues.Add(poolable);

            poolable.node = unusedObjects.AddFirst(poolable);

            if (forceActive)
            {
                gameObject.SetActive(true);
                gameObject.SetActive(false);
            }
#if UNITY_EDITOR
            RefreshName();
#endif
        }

        public async UniTask PrewarmAsync(int createPerFrame, CancellationToken cancellationToken)
        {
            int objectsToInstantiate;

            // It is dynamic in case we instantiate manually between frames
            while ((objectsToInstantiate = Mathf.Max(0, maxPrewarmCount - objectsCount)) > 0)
            {
                objectsToInstantiate = Mathf.Min(createPerFrame, objectsToInstantiate);

                for (var i = 0; i < objectsToInstantiate; i++)
                    Instantiate();

                await UniTask.NextFrame(cancellationToken);
            }
        }

        /// <summary>
        /// This will return an instance of the poolable object
        /// </summary>
        /// <returns></returns>
        public PoolableObject Get()
        {
            // These extra instantiations during initialization are to populate pools that will be used a lot later
            if (PoolManager.i.initializing && !isInitialized)
            {
                isInitialized = true;

                for (int i = unusedObjectsCount; i < Mathf.Min(usedObjectsCount * PREWARM_ACTIVE_MULTIPLIER, maxPrewarmCount); i++)
                    Instantiate();

                Instantiate();
            }
            else if (unusedObjects.Count == 0) { Instantiate(); }

            PoolableObject poolable = Extract();

            EnablePoolableObject(poolable);
            poolable.OnPoolGet();

            return poolable;
        }

        public T Get<T>() where T: MonoBehaviour =>
            this.Get().gameObject.GetComponent<T>();

        private PoolableObject Extract()
        {
            PoolableObject po = null;
            po = unusedObjects.First.Value;
            unusedObjects.RemoveFirst();
            po.node = usedObjects.AddFirst(po);

#if UNITY_EDITOR
            RefreshName();
#endif
            return po;
        }

        private void Instantiate() =>
            SetupPoolableObject(
                InstantiateAsOriginal());

        public GameObject InstantiateAsOriginal()
        {
            Assert.IsTrue(original != null, $"Original should never be null here ({id})");

            GameObject gameObject = instantiator != null
                ? instantiator.Instantiate(original)
                : Object.Instantiate(original);

            gameObject.SetActive(true);

            return gameObject;
        }

        private void SetupPoolableObject(GameObject gameObject, bool active = false)
        {
            if (PoolManager.i.poolables.ContainsKey(gameObject))
                return;

            PoolableObject poolable = new PoolableObject(this, gameObject);
            PoolManager.i.poolables.Add(gameObject, poolable);
            PoolManager.i.poolableValues.Add(poolable);

            if (!active)
            {
                DisablePoolableObject(poolable);
                poolable.node = unusedObjects.AddFirst(poolable);
            }
            else
            {
                EnablePoolableObject(poolable);
                poolable.node = usedObjects.AddFirst(poolable);
            }

#if UNITY_EDITOR
            RefreshName();
#endif
        }

        public void Release(PoolableObject poolable)
        {
            if (poolable == null || !PoolManager.i.HasPoolable(poolable))
                return;

            DisablePoolableObject(poolable);

            poolable.node.List.Remove(poolable.node);
            poolable.node = unusedObjects.AddFirst(poolable);

#if UNITY_EDITOR
            RefreshName();
#endif
        }

        public void ReleaseAll()
        {
            while (usedObjects.Count > 0) { usedObjects.First.Value.Release(); }
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

            PoolableObject obj = PoolManager.i.GetPoolable(gameObject);

            if (obj != null)
            {
                Debug.LogError($"ERROR: gameObject is already being tracked by a pool! -- {gameObject.name}", gameObject);

                return;
            }

            SetupPoolableObject(gameObject, addActive);
        }

        public void RemoveFromPool(PoolableObject poolable)
        {
            if (poolable.node != null)
            {
                if (poolable.node.List != null)
                    poolable.node.List.Remove(poolable);

                poolable.node = null;
            }

            PoolManager.i.poolables.Remove(poolable.gameObject);
            PoolManager.i.poolableValues.Remove(poolable);
#if UNITY_EDITOR
            RefreshName();
#endif
        }

        public void Cleanup()
        {
            ReleaseAll();

            while (unusedObjects.Count > 0)
            {
                PoolManager.i.poolables.Remove(unusedObjects.First.Value.gameObject);
                PoolManager.i.poolableValues.Remove(unusedObjects.First.Value);
                unusedObjects.RemoveFirst();
            }

            while (usedObjects.Count > 0)
            {
                PoolManager.i.poolables.Remove(usedObjects.First.Value.gameObject);
                PoolManager.i.poolableValues.Remove(usedObjects.First.Value);
                usedObjects.RemoveFirst();
            }

            unusedObjects.Clear();
            usedObjects.Clear();

            Utils.SafeDestroy(this.original);

            Utils.SafeDestroy(this.container);;

            OnCleanup?.Invoke(this);
        }

        public void EnablePoolableObject(PoolableObject poolable)
        {
            GameObject go = poolable.gameObject;

            if (go == null)
                return;

            if (!go.activeSelf)
                go.SetActive(true);

            go.transform.ResetLocalTRS();

            lastGetTime = Time.unscaledTime;
        }

        public void DisablePoolableObject(PoolableObject poolable)
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            if (DataStore.i.common.isApplicationQuitting.Get())
                return;
#endif
            GameObject go = poolable.gameObject;

            if (go == null)
                return;

            if (go.activeSelf)
                go.SetActive(false);

            if (PoolManager.USE_POOL_CONTAINERS)
            {
                if (container != null) { go.transform.SetParent(container.transform); }
            }
            else { go.transform.SetParent(null); }
        }

#if UNITY_EDITOR
        private void RefreshName()
        {
            if (this.container != null)
                this.container.name = $"in: {unusedObjectsCount} out: {usedObjectsCount} id: {id} persistent: {persistent}";
        }
#endif
        public static bool FindPoolInGameObject(GameObject gameObject, out Pool pool)
        {
            pool = null;

            if (PoolManager.i.poolables.TryGetValue(gameObject, out PoolableObject poolable))
            {
                pool = poolable.pool;

                return true;
            }

            return false;
        }

        public bool IsValid()
        {
            return original != null;
        }
    }
};
