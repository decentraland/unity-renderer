using DCL.Helpers;
using NUnit.Framework;
using System.Collections;
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
        [Test]
        public void EntityCreation()
        {
            // Create first entity
            string entityId = "1";

            TestHelpers.CreateSceneEntity(scene, entityId);
            var entityObject = scene.entities[entityId];

            Assert.IsTrue(entityObject != null);

            Assert.AreEqual(entityId, entityObject.entityId);

            // Create second entity
            entityObject = null;
            entityId = "2";

            TestHelpers.CreateSceneEntity(scene, entityId);
            scene.entities.TryGetValue(entityId, out entityObject);

            Assert.IsTrue(entityObject != null);

            Assert.AreEqual(entityId, entityObject.entityId);
        }

        [Test]
        public void EntityParenting()
        {
            string entityId = "2";
            string parentEntityId = "3";

            TestHelpers.CreateSceneEntity(scene, entityId);
            TestHelpers.CreateSceneEntity(scene, parentEntityId);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.transform.parent == scene.gameObject.transform,
                "parent is set to the scene root"
            );

            var parentEntityObject = scene.entities[parentEntityId];

            TestHelpers.SetEntityParent(scene, entityId, parentEntityId);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.transform.parent == parentEntityObject.gameObject.transform,
                "parent is set to parentId"
            );

            TestHelpers.SetEntityParent(scene, entityId, "0");

            Assert.IsTrue(
                scene.entities[entityId].gameObject.transform.parent == scene.gameObject.transform,
                "parent is set back to the scene root"
            );
        }

        [UnityTest]
        public IEnumerator EntityRemoval()
        {
            Assert.IsTrue(scene != null);

            string entityId = "2";

            TestHelpers.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(scene.entities.ContainsKey(entityId));

            var gameObjectReference = scene.entities[entityId].gameObject;

            TestHelpers.RemoveSceneEntity(scene, entityId);

            yield return null;

            Assert.IsFalse(scene.entities.ContainsKey(entityId));

            bool isDestroyedOrPooled = gameObjectReference == null || !gameObjectReference.activeSelf;
            Assert.IsTrue(isDestroyedOrPooled, "Entity gameobject reference is not getting destroyed nor pooled.");
        }
    }
}
