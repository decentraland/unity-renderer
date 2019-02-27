using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityGLTF;
using Newtonsoft.Json;
using NUnit.Framework;
using DCL.Models;
using DCL.Components;
using DCL.Configuration;
/*
* Play Mode Testing Highlights:
* - All Monobehaviour methods are invoked
* - Tests run in a standalone window
* - Tests may run slower, depending on the build target
*/

namespace Tests
{
    public class EntityTests
    {
        [UnityTest]
        public IEnumerator EntityCreation()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            Assert.IsTrue(sceneController != null);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            Assert.IsTrue(scene != null);

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

        [UnityTest]
        public IEnumerator EntityParenting()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            Assert.IsTrue(sceneController != null);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            Assert.IsTrue(scene != null);

            string entityId = "2";
            string parentEntityId = "3";

            TestHelpers.CreateSceneEntity(scene, entityId);
            TestHelpers.CreateSceneEntity(scene, parentEntityId);

            Assert.IsTrue(
              scene.entities[entityId].gameObject.transform.parent == scene.gameObject.transform,
              "parent is set to the scene root"
            );

            var parentEntityObject = scene.entities[parentEntityId];

            scene.SetEntityParent("{\"entityId\": \"" + entityId + "\"," + "\"parentId\": \"" + parentEntityId + "\"}");

            Assert.IsTrue(
              scene.entities[entityId].gameObject.transform.parent == parentEntityObject.gameObject.transform,
              "parent is set to parentId"
            );

            scene.SetEntityParent("{\"entityId\": \"" + entityId + "\"," + "\"parentId\": \"0\"}");

            Assert.IsTrue(
              scene.entities[entityId].gameObject.transform.parent == scene.gameObject.transform,
              "parent is set back to the scene root"
            );
        }

        [UnityTest]
        public IEnumerator EntityRemoval()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            string entityId = "2";

            Assert.IsTrue(sceneController != null);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            Assert.IsTrue(scene != null);

            TestHelpers.CreateSceneEntity(scene, entityId);

            var gameObjectReference = scene.entities[entityId].gameObject;

            TestHelpers.RemoveSceneEntity(scene, entityId);

            yield return new WaitForSeconds(0.01f);

            Assert.IsTrue(!scene.entities.ContainsKey(entityId));

            Assert.IsTrue(gameObjectReference == null, "Entity gameobject reference is not getting destroyed.");
        }
    }
}
