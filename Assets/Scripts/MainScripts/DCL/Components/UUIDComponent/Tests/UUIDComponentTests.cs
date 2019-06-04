using DCL.Components;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class UUIDComponentTests : TestsBase
    {
        [UnityTest]
        public IEnumerator OnClickComponentInitializesWithBasicShape()
        {
            yield return InitScene();

            DecentralandEntity entity;
            BoxShape shape = TestHelpers.InstantiateEntityWithShape<BoxShape, BoxShape.Model>(
                scene,
                DCL.Models.CLASS_ID.BOX_SHAPE,
                Vector3.zero,
                out entity,
                new BoxShape.Model() { });

            yield return shape.routine;

            string onClickId = "click-1";
            var OnClickComponentModel = new OnClickComponent.Model()
            {
                type = "onClick",
                uuid = onClickId
            };
            var component = TestHelpers.EntityComponentCreate<OnClickComponent, OnClickComponent.Model>(scene, entity,
                OnClickComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(entity.gameObject.GetComponent<Rigidbody>() != null,
                "the root object should have a rigidbody attached to detect its children collisions for the OnClick functionality");

            var meshFilter = entity.gameObject.GetComponentInChildren<MeshFilter>();
            var onClickCollider = meshFilter.transform.Find("OnClickCollider");

            Assert.IsTrue(onClickCollider != null, "OnClickCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onClickCollider.GetComponent<MeshCollider>().sharedMesh,
                "OnClickCollider should have the same mesh info as the mesh renderer");
        }

        [UnityTest]
        public IEnumerator OnClickComponentInitializesWithGLTFShape()
        {
            yield return InitScene();

            string entityId = "1";

            TestHelpers.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            string shapeId = TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
                JsonConvert.SerializeObject(new
                {
                    src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
                }));

            GLTFLoader gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<GLTFLoader>();
            yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded, 7f);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            string clickUuid = "click-1";
            var OnClickComponentModel = new OnClickComponent.Model()
            {
                type = "onClick",
                uuid = clickUuid
            };
            TestHelpers.EntityComponentCreate<OnClickComponent, OnClickComponent.Model>(scene, scene.entities[entityId],
                OnClickComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponent<Rigidbody>() != null,
                "the root object should have a rigidbody attached to detect its children collisions for the OnClick functionality");

            foreach (var meshFilter in scene.entities[entityId].gameObject.GetComponentsInChildren<MeshFilter>())
            {
                var onClickCollider = meshFilter.transform.Find("OnClickCollider");

                Assert.IsTrue(onClickCollider != null, "OnClickCollider should exist under any rendeder");

                Assert.AreSame(meshFilter.sharedMesh, onClickCollider.GetComponent<MeshCollider>().sharedMesh,
                    "OnClickCollider should have the same mesh info as the mesh renderer");
            }
        }

        [UnityTest]
        public IEnumerator OnClickComponentInitializesWithGLTFShapeAsynchronously()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
                }));

            string clickUuid = "click-1";
            var OnClickComponentModel = new OnClickComponent.Model()
            {
                type = "onClick",
                uuid = clickUuid
            };
            TestHelpers.EntityComponentCreate<OnClickComponent, OnClickComponent.Model>(scene, scene.entities[entityId],
                OnClickComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            GLTFLoader gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<GLTFLoader>();
            yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded, 7f);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponent<Rigidbody>() != null,
                "the root object should have a rigidbody attached to detect its children collisions for the OnClick functionality");

            foreach (var meshFilter in scene.entities[entityId].gameObject.GetComponentsInChildren<MeshFilter>())
            {
                var onClickCollider = meshFilter.transform.Find("OnClickCollider");

                Assert.IsTrue(onClickCollider != null, "OnClickCollider should exist under any rendeder");

                Assert.AreSame(meshFilter.sharedMesh, onClickCollider.GetComponent<MeshCollider>().sharedMesh,
                    "OnClickCollider should have the same mesh info as the mesh renderer");
            }
        }

        [UnityTest]
        public IEnumerator OnClickComponentInitializesAfterBasicShapeIsAdded()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            string clickUuid = "click-1";
            var OnClickComponentModel = new OnClickComponent.Model()
            {
                type = "onClick",
                uuid = clickUuid
            };

            TestHelpers.EntityComponentCreate<OnClickComponent, OnClickComponent.Model>(scene, scene.entities[entityId],
                OnClickComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponent<Rigidbody>() == null,
                "the root object shouldn't have a rigidbody attached until a shape is added");

            Assert.IsTrue(scene.entities[entityId].gameObject.transform.Find("OnClickCollider") == null,
                "the OnClickCollider object shouldn't exist until a shape is added");

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE,
                JsonConvert.SerializeObject(new BoxShape.Model { })
            );

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponent<Rigidbody>() != null,
                "the root object should have a rigidbody attached to detect its children collisions for the OnClick functionality");

            var meshFilter = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>();
            var onClickCollider = meshFilter.transform.Find("OnClickCollider");

            Assert.IsTrue(onClickCollider != null, "OnClickCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onClickCollider.GetComponent<MeshCollider>().sharedMesh,
                "OnClickCollider should have the same mesh info as the mesh renderer");
        }

        [UnityTest]
        public IEnumerator OnClickEventIsTriggered()
        {
            yield return InitScene();

            DecentralandEntity entity;
            BoxShape shape = TestHelpers.InstantiateEntityWithShape<BoxShape, BoxShape.Model>(
                scene,
                DCL.Models.CLASS_ID.BOX_SHAPE,
                Vector3.zero,
                out entity,
                new BoxShape.Model() { });

            yield return shape.routine;

            string onClickId = "click-1";
            var OnClickComponentModel = new OnClickComponent.Model()
            {
                type = "onClick",
                uuid = onClickId
            };
            var component = TestHelpers.EntityComponentCreate<OnClickComponent, OnClickComponent.Model>(scene, entity,
                OnClickComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(component != null);

            string targetEventType = "SceneEvent";

            var onClickEvent = new WebInterface.OnClickEvent();
            onClickEvent.uuid = onClickId;

            var sceneEvent = new WebInterface.SceneEvent<WebInterface.OnClickEvent>();
            sceneEvent.sceneId = scene.sceneData.id;
            sceneEvent.payload = onClickEvent;
            sceneEvent.eventType = "uuidEvent";
            string eventJSON = JsonUtility.ToJson(sceneEvent);
            bool eventTriggered = false;

            yield return TestHelpers.WaitForMessageFromEngine(targetEventType, eventJSON,
                () =>
                {
                    component.OnPointerDown();
                },
                () =>
                {
                    eventTriggered = true;
                });

            Assert.IsTrue(eventTriggered);
        }
    }
}
