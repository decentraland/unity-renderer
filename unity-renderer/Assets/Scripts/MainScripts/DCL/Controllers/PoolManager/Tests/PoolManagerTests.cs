using DCL;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

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

            public bool IsValid(GameObject original)
            {
                return original != null;
            }
        }

        [UnityTest]
        public IEnumerator PooledGameObjectDestroyed()
        {
            yield return InitScene(false, false, false, false, false);

            PooledObjectInstantiator instantiator = new PooledObjectInstantiator();

            GameObject original = new GameObject("Original");

            object id = "testId";

            Pool pool = PoolManager.i.AddPool(id, original, instantiator);
            Assert.IsNotNull(pool, "Pool instance shouldn't be null.");

            PoolableObject po1 = PoolManager.i.Get(id);
            Assert.IsNotNull(po1, "Poolable object instance shouldn't be null.");
            po1.gameObject.SetActive(true);

            Assert.IsTrue(po1.isActiveAndEnabled, "Poolable object should be alive.");

            PoolableObject po2 = PoolManager.i.Get(id);
            Assert.IsNotNull(po2, "Poolable object instance shouldn't be null.");
            po2.gameObject.SetActive(true);

            Assert.IsTrue(po2.isActiveAndEnabled, "Poolable object should be alive.");

            pool = PoolManager.i.GetPool(id);
            Assert.IsNotNull(pool, "Pool instance shouldn't be null.");

            Assert.AreEqual(2, pool.objectsCount, "Objects count should be 2");
            Assert.AreEqual(2, pool.activeCount, "Alive objects count should be 2");
            Assert.AreEqual(0, pool.inactiveCount, "Inactive objects count should be 0");

            GameObject.Destroy(po1.gameObject);
            yield return null;

            Assert.AreEqual(1, pool.objectsCount, "Objects count should be 1");
            Assert.AreEqual(1, pool.activeCount, "Alive objects count should be 1");
            Assert.AreEqual(0, pool.inactiveCount, "Inactive objects count should be 0");
        }

        [UnityTest]
        public IEnumerator GetPoolableObject()
        {
            yield return InitScene();

            PooledObjectInstantiator instantiator = new PooledObjectInstantiator();

            GameObject original = new GameObject("Original");

            object id = "testId";

            Pool pool = PoolManager.i.AddPool(id, original, instantiator);
            Assert.IsNotNull(pool, "Pool instance shouldn't be null.");

            PoolableObject po1 = PoolManager.i.Get(id);
            Assert.IsNotNull(po1, "Poolable object instance shouldn't be null.");

            PoolableObject po2 = PoolManager.i.Get(id);
            Assert.IsNotNull(po2, "Poolable object instance shouldn't be null.");

            PoolableObject po3 = PoolManager.i.Get(id);
            Assert.IsNotNull(po3, "Poolable object instance shouldn't be null.");

            Assert.AreEqual(3, pool.objectsCount, "Objects count should be 3");
            Assert.AreEqual(3, pool.activeCount, "Alive objects count should be 3");
            Assert.AreEqual(0, pool.inactiveCount, "Inactive objects count should be 0");

            po3.Release();

            Assert.AreEqual(3, pool.objectsCount, "Objects count should be 3");
            Assert.AreEqual(2, pool.activeCount, "Alive objects count should be 2");
            Assert.AreEqual(1, pool.inactiveCount, "Inactive objects count should be 1");

            PoolManager.i.ReleaseAllFromPool(id);

            Assert.AreEqual(3, pool.objectsCount, "Objects count should be 3");
            Assert.AreEqual(0, pool.activeCount, "Alive objects count should be 0");
            Assert.AreEqual(3, pool.inactiveCount, "Inactive objects count should be 3");

            PoolManager.i.RemovePool(id);

            Assert.IsFalse(PoolManager.i.ContainsPool(id), "Pool shouldn't exist after disposal");
        }
    }
}
