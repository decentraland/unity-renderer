using DCL;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
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

        protected override IEnumerator SetUp()
        {
            yield break;
        }

        [UnityTest]
        public IEnumerator PooledGameObjectDestroyed()
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

            GameObject.Destroy(po1.gameObject);
            yield return null;

            PoolManager.i.CleanPoolableReferences();

            Assert.AreEqual(1, pool.usedObjectsCount, "Alive objects count should be 1");
            Assert.AreEqual(0, pool.unusedObjectsCount, "Inactive objects count should be 0");
        }

        [UnityTest]
        public IEnumerator CleanPoolableReferences()
        {
            GameObject obj = new GameObject();
            GameObject obj2 = new GameObject();
            GameObject obj3 = new GameObject();

            PoolManager.i.poolables.Add(obj, new PoolableObject() { gameObject = obj });
            PoolManager.i.poolables.Add(obj2, new PoolableObject() { gameObject = obj2 });
            PoolManager.i.poolables.Add(obj3, new PoolableObject() { gameObject = obj3 });

            Object.Destroy(obj);
            Object.Destroy(obj2);

            yield return null;
            PoolManager.i.CleanPoolableReferences();

            Assert.AreEqual(1, PoolManager.i.poolables.Count);
            Assert.IsTrue(PoolManager.i.poolables[obj3].gameObject == obj3);

            Object.Destroy(obj3);
            yield return null;
            PoolManager.i.CleanPoolableReferences();

            Assert.AreEqual(0, PoolManager.i.poolables.Count);
        }

        [Test]
        public void GetPoolableObject()
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
    }
}
