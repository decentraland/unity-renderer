using DCL.Helpers;
using NUnit.Framework;
using System.Collections;
using DCL.Controllers;
using UnityEngine.TestTools;

/*
* Play Mode Testing Highlights:
* - All Monobehaviour methods are invoked
* - Tests run in a standalone window
* - Tests may run slower, depending on the build target
*/

namespace Tests
{
    public class EntityTests : IntegrationTestSuite_Legacy
    {
        private ParcelScene scene;

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            scene = TestUtils.CreateTestScene() as ParcelScene;
        }


        [Test]
        public void EntityCreation()
        {
            // Create first entity
            long entityId = 1;

            TestUtils.CreateSceneEntity(scene, entityId);
            var entityObject = scene.entities[entityId];

            Assert.IsTrue(entityObject != null);

            Assert.AreEqual(entityId, entityObject.entityId);

            // Create second entity
            entityObject = null;
            entityId = 2;

            TestUtils.CreateSceneEntity(scene, entityId);
            scene.entities.TryGetValue(entityId, out entityObject);

            Assert.IsTrue(entityObject != null);

            Assert.AreEqual(entityId, entityObject.entityId);
        }

        [Test]
        public void EntityParenting()
        {
            long entityId = 20;
            long parentEntityId = 30;

            TestUtils.CreateSceneEntity(scene, entityId);
            TestUtils.CreateSceneEntity(scene, parentEntityId);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.transform.parent == scene.gameObject.transform,
                "parent is set to the scene root"
            );

            var parentEntityObject = scene.entities[parentEntityId];

            TestUtils.SetEntityParent(scene, entityId, parentEntityId);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.transform.parent == parentEntityObject.gameObject.transform,
                "parent is set to parentId"
            );

            TestUtils.SetEntityParent(scene, entityId, 0);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.transform.parent == scene.gameObject.transform,
                "parent is set back to the scene root"
            );
        }

        [UnityTest]
        public IEnumerator EntityRemoval()
        {
            Assert.IsTrue(scene != null);

            long entityId = 2;

            TestUtils.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(scene.entities.ContainsKey(entityId));

            var gameObjectReference = scene.entities[entityId].gameObject;

            TestUtils.RemoveSceneEntity(scene, entityId);

            yield return null;

            Assert.IsFalse(scene.entities.ContainsKey(entityId));

            bool isDestroyedOrPooled = gameObjectReference == null || !gameObjectReference.activeSelf;
            Assert.IsTrue(isDestroyedOrPooled, "Entity gameobject reference is not getting destroyed nor pooled.");
        }
    }
}