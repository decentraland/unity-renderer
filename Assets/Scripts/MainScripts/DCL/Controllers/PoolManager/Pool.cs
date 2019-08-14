using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public interface IPooledObjectInstantiator
    {
        GameObject Instantiate(GameObject go);
    }

    public class Pool : MonoBehaviour, ICleanable
    {
        public object id;
        public GameObject original;
        public System.Action<Pool> OnReleaseAll;

        private readonly List<PoolableObject> inactiveObjects = new List<PoolableObject>();

        public float lastGetTime
        {
            get;
            private set;
        }

        public int objectsCount
        {
            get;
            private set;
        }

        public int inactiveCount
        {
            get { return inactiveObjects.Count; }
        }

        public int activeCount
        {
            get { return objectsCount - inactiveObjects.Count; }
        }

        // In production it will always be false
        private bool isQuitting = false;

        void Awake()
        {
#if UNITY_EDITOR
            // We need to check if application is quitting in editor
            // to prevent the pool from releasing objects that are
            // being destroyed 
            Application.quitting += OnIsQuitting;
#endif
        }


        public PoolableObject Get<T>(T instantiator) where T : IPooledObjectInstantiator
        {
            PoolableObject po = GetPoolableObject();

            if (!po)
            {
                po = Instantiate<T>(instantiator);
            }

            ActivatePoolableObject(po);

            RefreshName();

            return po;
        }

        public PoolableObject Get()
        {
            PoolableObject po = GetPoolableObject();

            if (!po)
            {
                po = Instantiate();
            }

            ActivatePoolableObject(po);

            RefreshName();

            return po;
        }

        private void ActivatePoolableObject(PoolableObject po)
        {
            lastGetTime = Time.realtimeSinceStartup;
            po.transform.parent = null;
            po.Init();
        }

        private PoolableObject GetPoolableObject()
        {
            PoolableObject po = null;
            int count = inactiveObjects.Count;

            // This is just a precaution to avoid getting an object
            // that has been destroyed
            while (!po && count > 0)
            {
                po = inactiveObjects[0];

                if (!po)
                    objectsCount--;

                inactiveObjects.RemoveAt(0);
                count--;
            }

            return po;
        }

        private new PoolableObject Instantiate<T>(T instantiator) where T : IPooledObjectInstantiator
        {
            GameObject go = instantiator.Instantiate(original);

            return SetupPoolableObject(go);
        }

        private PoolableObject Instantiate()
        {
            GameObject go = GameObject.Instantiate(original);

            return SetupPoolableObject(go);
        }

        private PoolableObject SetupPoolableObject(GameObject go)
        {
            PoolableObject po = go.AddComponent<PoolableObject>();
            po.pool = this;

            go.SetActive(false);

            objectsCount++;

            return po;
        }


#if UNITY_EDITOR
        // We need to check if application is quitting in editor
        // to prevent the pool from releasing objects that are
        // being destroyed 
        void OnIsQuitting()
        {
            Application.quitting -= OnIsQuitting;
            isQuitting = true;
        }
#endif

        public void Release(PoolableObject po)
        {
            if (!isQuitting)
            {
                lastGetTime = Time.realtimeSinceStartup;

                if (po)
                {
                    po.transform.parent = this.transform;
                    po.gameObject.SetActive(false);

                    if (!inactiveObjects.Contains(po))
                        inactiveObjects.Add(po);
                }
                else
                    objectsCount--;

                RefreshName();
            }
        }

        public void ReleaseAll()
        {
            OnReleaseAll?.Invoke(this);
        }

        public void Unregister(PoolableObject po)
        {
            if (inactiveObjects.Contains(po))
            {
                inactiveObjects.Remove(po);
            }

            objectsCount--;
        }

        public void Cleanup()
        {
            ReleaseAll();

            int count = inactiveObjects.Count;

            for (int i = 0; i < count; i++)
            {
                if (inactiveObjects[i])
                    Destroy(inactiveObjects[i].gameObject);
            }

            inactiveObjects.Clear();
            objectsCount = 0;

            Destroy(this.original);
            Destroy(this.gameObject);
        }

        private void RefreshName()
        {
            this.name = $"in: {inactiveCount} out: {activeCount} id: {id}";
        }

    }
};