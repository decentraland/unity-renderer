using DCL;
using NUnit.Framework;
using System.Collections;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class PoolManagerTests : IntegrationTestSuite_Legacy
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

        [Test]
        public void PoolManagerShouldHandleNullArgsGracefully()
        {
            var thisShouldBeNull = PoolManager.i.GetPoolable(null);
            Assert.IsTrue(thisShouldBeNull == null);
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

            po1.Release();
            yield return null;

            Assert.AreEqual(1, pool.usedObjectsCount, "Alive objects count should be 1");
            Assert.AreEqual(1, pool.unusedObjectsCount, "Inactive objects count should be 1");

            PoolManager.i.Cleanup();

            Assert.AreEqual(0, pool.usedObjectsCount, "Alive objects count should be 0");
            Assert.AreEqual(0, pool.unusedObjectsCount, "Inactive objects count should be 0");
        }

        [UnityTest]
        public IEnumerator ShouldCloneObjectsCorrectlyWhenRenderingIsOff()
        {
            // 1. Turn off rendering
            CommonScriptableObjects.rendererState.Set(false);
            
            // 2. Instantiate entity
            string entity1Id = "1";
            var entity1 = TestHelpers.CreateSceneEntity(scene, entity1Id);
            
            // 3. Attach a shape
            var shapeModel = new LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>.Model();
            shapeModel.src = Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";
            var shapeComponentId = TestHelpers.CreateAndSetShape(scene, entity1Id, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(shapeModel));

            LoadWrapper gltfShapeLoader1 = GLTFShape.GetLoaderForEntity(scene.entities[entity1Id]);

            yield return new WaitUntil(() => gltfShapeLoader1.alreadyLoaded);
            yield return null;
            
            Pool pool = PoolManager.i.GetPool(shapeModel.src.ToLower());
            
            Assert.IsTrue(pool.unusedObjectsCount == 0);
            
            // 4. Remove shape
            entity1.RemoveSharedComponent(typeof(BaseShape));
            yield return null;
            
            // 5. Attach shape to new entity
            string entity2Id = "2";
            var entity2 = TestHelpers.CreateSceneEntity(scene, entity2Id);
            
            scene.SharedComponentAttach(
                entity2Id,
                shapeComponentId
            );
            LoadWrapper gltfShapeLoader2 = GLTFShape.GetLoaderForEntity(scene.entities[entity2Id]);
            yield return new WaitUntil(() => gltfShapeLoader2.alreadyLoaded);
            yield return null;
            
            Assert.IsTrue(pool.unusedObjectsCount == 0);
            
            // 6. Attach shape to new entity
            string entity3Id = "3";
            var entity3 = TestHelpers.CreateSceneEntity(scene, entity3Id);
            
            scene.SharedComponentAttach(
                entity3Id,
                shapeComponentId
            );
            LoadWrapper gltfShapeLoader3 = GLTFShape.GetLoaderForEntity(scene.entities[entity3Id]);
            yield return new WaitUntil(() => gltfShapeLoader3.alreadyLoaded);
            yield return null;
            
            Assert.IsTrue(pool.unusedObjectsCount == 0);
            
            // 7. Attach shape to new entity
            string entity4Id = "4";
            var entity4 = TestHelpers.CreateSceneEntity(scene, entity4Id);
            
            scene.SharedComponentAttach(
                entity4Id,
                shapeComponentId
            );
            LoadWrapper gltfShapeLoader4 = GLTFShape.GetLoaderForEntity(scene.entities[entity4Id]);
            yield return new WaitUntil(() => gltfShapeLoader4.alreadyLoaded);
            yield return null;
            
            Assert.IsTrue(pool.unusedObjectsCount == 0);
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