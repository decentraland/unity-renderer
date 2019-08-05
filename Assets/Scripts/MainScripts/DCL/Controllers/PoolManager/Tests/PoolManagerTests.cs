using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityGLTF;

namespace Tests
{
    public class PoolManagerTests : TestsBase
    {
        public class PooledObjectInstantiator : IPooledObjectInstantiator
        {
            public GameObject Instantiate(GameObject go)
            {
                return GameObject.Instantiate(go);
            }
        }

        [UnityTest]
        public IEnumerator GetPoolableObject()
        {
            yield return InitScene();

            PooledObjectInstantiator instantiator = new PooledObjectInstantiator();

            GameObject original = new GameObject("Original");

            string id = "testId";

            PoolableObject po1 = PoolManager.i.Get<PooledObjectInstantiator>(id, original, instantiator);
            Assert.IsNotNull(po1, "Poolable object instance shouldn't be null.");

            PoolableObject po2 = PoolManager.i.GetIfPoolExists(id);
            Assert.IsNotNull(po2, "Poolable object instance shouldn't be null.");

            PoolableObject po3 = PoolManager.i.Get(id, original);
            Assert.IsNotNull(po3, "Poolable object instance shouldn't be null.");

            Pool pool = PoolManager.i.GetPool(id);
            Assert.IsNotNull(pool, "Pool instance shouldn't be null.");

            Assert.IsTrue(pool.objectsCount == 3, "Objects count should be 3");
            Assert.IsTrue(pool.activeCount == 3, "Alive objects count should be 3");
            Assert.IsTrue(pool.inactiveCount == 0, "Inactive objects count should be 0");

            po3.Release();

            Assert.IsTrue(pool.objectsCount == 3, "Objects count should be 3");
            Assert.IsTrue(pool.activeCount == 2, "Alive objects count should be 2");
            Assert.IsTrue(pool.inactiveCount == 1, "Inactive objects count should be 1");

            PoolManager.i.ReleaseAll(id);

            Assert.IsTrue(pool.objectsCount == 3, "Objects count should be 3");
            Assert.IsTrue(pool.activeCount == 0, "Alive objects count should be 0");
            Assert.IsTrue(pool.inactiveCount == 3, "Inactive objects count should be 3");

            PoolManager.i.CleanupPool(id);

            Assert.IsFalse(PoolManager.i.ContainsPool(id), "Pool shouldn't exist after disposal");

            po1 = PoolManager.i.Get<PooledObjectInstantiator>(id, original, instantiator);
            Assert.IsNotNull(po1, "Poolable object instance shouldn't be null.");
            po1.gameObject.SetActive(true);

            Assert.IsTrue(po1.isAlive, "Poolable object should be alive.");

            po2 = PoolManager.i.GetIfPoolExists(id);
            Assert.IsNotNull(po2, "Poolable object instance shouldn't be null.");
            po2.gameObject.SetActive(true);

            Assert.IsTrue(po2.isAlive, "Poolable object should be alive.");

            pool = PoolManager.i.GetPool(id);
            Assert.IsNotNull(pool, "Pool instance shouldn't be null.");

            Assert.IsTrue(pool.objectsCount == 2, "Objects count should be 2");
            Assert.IsTrue(pool.activeCount == 2, "Alive objects count should be 2");
            Assert.IsTrue(pool.inactiveCount == 0, "Inactive objects count should be 0");

            GameObject.Destroy(po1.gameObject);
            yield return new WaitForEndOfFrame();

            Assert.IsTrue(pool.objectsCount == 1, "Objects count should be 1");
            Assert.IsTrue(pool.activeCount == 1, "Alive objects count should be 1");
            Assert.IsTrue(pool.inactiveCount == 0, "Inactive objects count should be 0");
        }
    }
}