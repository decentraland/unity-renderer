using DCL;
using NUnit.Framework;
using System.Collections;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class PoolManagerShould
    {
        public class PooledObjectInstantiator : IPooledObjectInstantiator
        {
            public GameObject Instantiate(GameObject go) { return GameObject.Instantiate(go); }

            public bool IsValid(GameObject original) { return original != null; }
        }

        [Test]
        public void HandleNullArgsGracefully()
        {
            var thisShouldBeNull = PoolManager.i.GetPoolable(null);
            Assert.IsTrue(thisShouldBeNull == null);
        }

        [UnityTest]
        public IEnumerator CountUsedAndUnusedObjectsWhenGetAndReleaseIsCalled()
        {
            PooledObjectInstantiator instantiator = new PooledObjectInstantiator();

            GameObject original = new GameObject("Original");

            object id = "testId";

            Pool pool = PoolManager.i.AddPool(id, original, instantiator);
            Assert.IsNotNull(pool, "Pool instance shouldn't be null.");

            PoolableObject po1 = PoolManager.i.Get(id);
            Assert.IsNotNull(po1, "Poolable object instance shouldn't be null.");
            po1.gameObject.SetActive(true);

            Assert.IsTrue(po1.gameObject.activeSelf, "Poolable object should be alive.");

            PoolableObject po2 = PoolManager.i.Get(id);
            Assert.IsNotNull(po2, "Poolable object instance shouldn't be null.");
            po2.gameObject.SetActive(true);

            Assert.IsTrue(po2.gameObject.activeSelf, "Poolable object should be alive.");

            pool = PoolManager.i.GetPool(id);
            Assert.IsNotNull(pool, "Pool instance shouldn't be null.");

            Assert.AreEqual(2, pool.usedObjectsCount, "Alive objects count should be 2");
            Assert.AreEqual(0, pool.unusedObjectsCount, "Inactive objects count should be 0");

            po1.Release();
            yield return null;

            Assert.AreEqual(1, pool.usedObjectsCount, "Alive objects count should be 1");
            Assert.AreEqual(1, pool.unusedObjectsCount, "Inactive objects count should be 1");

            PoolManager.i.Dispose();

            Assert.AreEqual(0, pool.usedObjectsCount, "Alive objects count should be 0");
            Assert.AreEqual(0, pool.unusedObjectsCount, "Inactive objects count should be 0");
        }

        [Test]
        public void GetPoolableObjectsWhenGetIsCalled()
        {
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

            Assert.AreEqual(3, pool.usedObjectsCount, "Alive objects count should be 3");
            Assert.AreEqual(0, pool.unusedObjectsCount, "Inactive objects count should be 0");

            po3.Release();

            Assert.AreEqual(2, pool.usedObjectsCount, "Alive objects count should be 2");
            Assert.AreEqual(1, pool.unusedObjectsCount, "Inactive objects count should be 1");

            PoolManager.i.ReleaseAllFromPool(id);

            Assert.AreEqual(0, pool.usedObjectsCount, "Alive objects count should be 0");
            Assert.AreEqual(3, pool.unusedObjectsCount, "Inactive objects count should be 3");

            PoolManager.i.RemovePool(id);

            Assert.IsFalse(PoolManager.i.ContainsPool(id), "Pool shouldn't exist after disposal");
        }

        [Test]
        public void InitializeOnlyOnceAfterRendererIsToggled()
        {
            PooledObjectInstantiator instantiator = new PooledObjectInstantiator();
            GameObject original = new GameObject("Original");

            Pool pool = PoolManager.i.AddPool("totallyNewPoolID", original, instantiator);
            Assert.IsNotNull(pool, "Pool instance shouldn't be null.");

            // When teleporting this is false
            CommonScriptableObjects.rendererState.Set(false);

            pool.Get().Release();
            pool.Get().Release();

            UnityEngine.Assertions.Assert.AreEqual(1, pool.unusedObjectsCount, "Unused objects pool");
        }
    }
}