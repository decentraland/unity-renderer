using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using Cinemachine;

namespace Tests
{
    public class UUIDComponentTests : TestsBase
    {
        void InstantiateEntityWithShape(out DecentralandEntity entity, out BoxShape shape)
        {
            shape = TestHelpers.InstantiateEntityWithShape<BoxShape, BoxShape.Model>(
                scene,
                DCL.Models.CLASS_ID.BOX_SHAPE,
                Vector3.zero,
                out entity,
                new BoxShape.Model() { });
        }

        [UnityTest]
        public IEnumerator OnClickComponentInitializesWithBasicShape()
        {
            yield return InitScene();

            DecentralandEntity entity;
            BoxShape shape;
            InstantiateEntityWithShape(out entity, out shape);

            yield return shape.routine;

            string onPointerId = "pointerevent-1";
            var OnClickComponentModel = new OnClickComponent.Model()
            {
                type = OnClickComponent.NAME,
                uuid = onPointerId
            };
            var component = TestHelpers.EntityComponentCreate<OnClickComponent, OnClickComponent.Model>(scene, entity,
                OnClickComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(entity.gameObject.GetComponent<Rigidbody>() != null,
                "the root object should have a rigidbody attached to detect its children collisions for the OnClick functionality");

            var meshFilter = entity.gameObject.GetComponentInChildren<MeshFilter>();
            var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

            Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                "OnPointerEventCollider should have the same mesh info as the mesh renderer");
        }

        [UnityTest]
        public IEnumerator OnPointerDownComponentInitializesWithBasicShape()
        {
            yield return InitScene();

            DecentralandEntity entity;
            BoxShape shape;
            InstantiateEntityWithShape(out entity, out shape);

            yield return shape.routine;

            string onPointerId = "pointerevent-1";
            var OnPointerDownComponentModel = new OnPointerDownComponent.Model()
            {
                type = "pointerUp",
                uuid = onPointerId
            };
            var component = TestHelpers.EntityComponentCreate<OnPointerDownComponent, OnPointerDownComponent.Model>(scene, entity,
                OnPointerDownComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(entity.gameObject.GetComponent<Rigidbody>() != null,
                "the root object should have a rigidbody attached to detect its children collisions for the OnPointerDown functionality");

            var meshFilter = entity.gameObject.GetComponentInChildren<MeshFilter>();
            var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

            Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                "OnPointerEventCollider should have the same mesh info as the mesh renderer");
        }

        [UnityTest]
        public IEnumerator OnPointerUpComponentInitializesWithBasicShape()
        {
            yield return InitScene();

            DecentralandEntity entity;
            BoxShape shape;
            InstantiateEntityWithShape(out entity, out shape);

            yield return shape.routine;

            string onPointerId = "pointerevent-1";
            var OnPointerUpComponentModel = new OnPointerUpComponent.Model()
            {
                type = "pointerUp",
                uuid = onPointerId
            };
            var component = TestHelpers.EntityComponentCreate<OnPointerUpComponent, OnPointerUpComponent.Model>(scene, entity,
                OnPointerUpComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(entity.gameObject.GetComponent<Rigidbody>() != null,
                "the root object should have a rigidbody attached to detect its children collisions for the OnPointerUp functionality");

            var meshFilter = entity.gameObject.GetComponentInChildren<MeshFilter>();
            var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

            Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                "OnPointerEventCollider should have the same mesh info as the mesh renderer");
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
                    src = DCL.Helpers.Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
                }));

            LoadWrapper_GLTF gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>();
            yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded, 7f);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            string clickUuid = "pointerevent-1";
            var OnClickComponentModel = new OnClickComponent.Model()
            {
                type = OnClickComponent.NAME,
                uuid = clickUuid
            };
            TestHelpers.EntityComponentCreate<OnClickComponent, OnClickComponent.Model>(scene, scene.entities[entityId],
                OnClickComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponent<Rigidbody>() != null,
                "the root object should have a rigidbody attached to detect its children collisions for the OnClick functionality");

            foreach (var meshFilter in scene.entities[entityId].gameObject.GetComponentsInChildren<MeshFilter>())
            {
                var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

                Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

                Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                    "OnPointerEventCollider should have the same mesh info as the mesh renderer");
            }
        }

        [UnityTest]
        public IEnumerator OnPointerDownComponentInitializesWithGLTFShape()
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
                    src = DCL.Helpers.Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
                }));

            LoadWrapper_GLTF gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>();
            yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded, 7f);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            string clickUuid = "pointerevent-1";
            var OnPointerDownComponentModel = new OnPointerDownComponent.Model()
            {
                type = "pointerDown",
                uuid = clickUuid
            };
            TestHelpers.EntityComponentCreate<OnPointerDownComponent, OnPointerDownComponent.Model>(scene, scene.entities[entityId],
                OnPointerDownComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponent<Rigidbody>() != null,
                "the root object should have a rigidbody attached to detect its children collisions for the OnPointerDown functionality");

            foreach (var meshFilter in scene.entities[entityId].gameObject.GetComponentsInChildren<MeshFilter>())
            {
                var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

                Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

                Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                    "OnPointerEventCollider should have the same mesh info as the mesh renderer");
            }
        }

        [UnityTest]
        public IEnumerator OnPointerUpComponentInitializesWithGLTFShape()
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
                    src = DCL.Helpers.Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
                }));

            LoadWrapper_GLTF gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>();
            yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded, 7f);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            string clickUuid = "pointerevent-1";
            var OnPointerUpComponentModel = new OnPointerUpComponent.Model()
            {
                type = "pointerUp",
                uuid = clickUuid
            };
            TestHelpers.EntityComponentCreate<OnPointerUpComponent, OnPointerUpComponent.Model>(scene, scene.entities[entityId],
                OnPointerUpComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponent<Rigidbody>() != null,
                "the root object should have a rigidbody attached to detect its children collisions for the OnPointerDown functionality");

            foreach (var meshFilter in scene.entities[entityId].gameObject.GetComponentsInChildren<MeshFilter>())
            {
                var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

                Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

                Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                    "OnPointerEventCollider should have the same mesh info as the mesh renderer");
            }
        }

        [UnityTest]
        public IEnumerator OnClickComponentInitializesWithGLTFShapeAsynchronously()
        {
            yield return InitScene(reloadUnityScene: false);

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = DCL.Helpers.Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
                }));

            string clickUuid = "pointerevent-1";
            var OnClickComponentModel = new OnClickComponent.Model()
            {
                type = OnClickComponent.NAME,
                uuid = clickUuid
            };
            TestHelpers.EntityComponentCreate<OnClickComponent, OnClickComponent.Model>(scene, scene.entities[entityId],
                OnClickComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            LoadWrapper_GLTF gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>();
            yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded, 7f);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponent<Rigidbody>() != null,
                "the root object should have a rigidbody attached to detect its children collisions for the OnClick functionality");

            foreach (var meshFilter in scene.entities[entityId].gameObject.GetComponentsInChildren<MeshFilter>())
            {
                var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

                Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

                Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                    "OnPointerEventCollider should have the same mesh info as the mesh renderer");
            }
        }

        [UnityTest]
        public IEnumerator OnPointerDownComponentInitializesWithGLTFShapeAsynchronously()
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
                    src = DCL.Helpers.Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
                }));

            string clickUuid = "pointerevent-1";
            var OnPointerDownModel = new OnPointerDownComponent.Model()
            {
                type = "pointerDown",
                uuid = clickUuid
            };
            TestHelpers.EntityComponentCreate<OnPointerDownComponent, OnPointerDownComponent.Model>(scene, scene.entities[entityId],
                OnPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            LoadWrapper_GLTF gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>();
            yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded, 7f);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponent<Rigidbody>() != null,
                "the root object should have a rigidbody attached to detect its children collisions for the OnPointerDown functionality");

            foreach (var meshFilter in scene.entities[entityId].gameObject.GetComponentsInChildren<MeshFilter>())
            {
                var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

                Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

                Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                    "OnPointerEventCollider should have the same mesh info as the mesh renderer");
            }
        }

        [UnityTest]
        public IEnumerator OnPointerUpComponentInitializesWithGLTFShapeAsynchronously()
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
                    src = DCL.Helpers.Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
                }));

            string clickUuid = "pointerevent-1";
            var OnPointerUpModel = new OnPointerUpComponent.Model()
            {
                type = "pointerUp",
                uuid = clickUuid
            };
            TestHelpers.EntityComponentCreate<OnPointerUpComponent, OnPointerUpComponent.Model>(scene, scene.entities[entityId],
                OnPointerUpModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            LoadWrapper_GLTF gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>();
            yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded, 7f);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponent<Rigidbody>() != null,
                "the root object should have a rigidbody attached to detect its children collisions for the OnPointerUp functionality");

            foreach (var meshFilter in scene.entities[entityId].gameObject.GetComponentsInChildren<MeshFilter>())
            {
                var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

                Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

                Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                    "OnPointerEventCollider should have the same mesh info as the mesh renderer");
            }
        }

        [UnityTest]
        public IEnumerator OnClickComponentInitializesAfterBasicShapeIsAdded()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            string clickUuid = "pointerevent-1";
            var OnClickComponentModel = new OnClickComponent.Model()
            {
                type = OnClickComponent.NAME,
                uuid = clickUuid
            };

            var component = TestHelpers.EntityComponentCreate<OnClickComponent, OnClickComponent.Model>(scene, scene.entities[entityId],
                OnClickComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(component != null, "component is null?");

            yield return component.routine;

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponent<Rigidbody>() == null,
                "the root object shouldn't have a rigidbody attached until a shape is added");

            Assert.IsTrue(scene.entities[entityId].gameObject.transform.Find(OnPointerEventColliders.COLLIDER_NAME) == null,
                "the OnPointerEventCollider object shouldn't exist until a shape is added");

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE,
                JsonConvert.SerializeObject(new BoxShape.Model { })
            );

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponent<Rigidbody>() != null,
                "the root object should have a rigidbody attached to detect its children collisions for the OnClick functionality");

            var meshFilter = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>();
            var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

            Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                "OnPointerEventCollider should have the same mesh info as the mesh renderer");
        }

        [UnityTest]
        public IEnumerator OnPointerDownComponentInitializesAfterBasicShapeIsAdded()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            string clickUuid = "pointerevent-1";
            var OnPointerDownComponentModel = new OnPointerDownComponent.Model()
            {
                type = "pointerDown",
                uuid = clickUuid
            };

            TestHelpers.EntityComponentCreate<OnPointerDownComponent, OnPointerDownComponent.Model>(scene, scene.entities[entityId],
                OnPointerDownComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponent<Rigidbody>() == null,
                "the root object shouldn't have a rigidbody attached until a shape is added");

            Assert.IsTrue(scene.entities[entityId].gameObject.transform.Find(OnPointerEventColliders.COLLIDER_NAME) == null,
                "the OnPointerEventCollider object shouldn't exist until a shape is added");

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE,
                JsonConvert.SerializeObject(new BoxShape.Model { })
            );

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponent<Rigidbody>() != null,
                "the root object should have a rigidbody attached to detect its children collisions for the OnPointerDown functionality");

            var meshFilter = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>();
            var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

            Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                "OnPointerEventCollider should have the same mesh info as the mesh renderer");
        }

        [UnityTest]
        public IEnumerator OnPointerUpComponentInitializesAfterBasicShapeIsAdded()
        {
            yield return InitScene();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            string clickUuid = "pointerevent-1";
            var OnPointerUpComponentModel = new OnPointerUpComponent.Model()
            {
                type = "pointerUp",
                uuid = clickUuid
            };

            TestHelpers.EntityComponentCreate<OnPointerUpComponent, OnPointerUpComponent.Model>(scene, scene.entities[entityId],
                OnPointerUpComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponent<Rigidbody>() == null,
                "the root object shouldn't have a rigidbody attached until a shape is added");

            Assert.IsTrue(scene.entities[entityId].gameObject.transform.Find(OnPointerEventColliders.COLLIDER_NAME) == null,
                "the OnPointerEventCollider object shouldn't exist until a shape is added");

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE,
                JsonConvert.SerializeObject(new BoxShape.Model { })
            );

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponent<Rigidbody>() != null,
                "the root object should have a rigidbody attached to detect its children collisions for the OnPointerUp functionality");

            var meshFilter = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>();
            var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

            Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                "OnPointerEventCollider should have the same mesh info as the mesh renderer");
        }

        [UnityTest]
        [Explicit("This test is failing because retrieveCamera is failing in PointerEventsController. It may be related with the new camera setup. Please check MainTest scene setup.")]
        public IEnumerator OnClickEventIsTriggered()
        {
            yield return InitScene();

            DecentralandEntity entity;
            BoxShape shape;
            InstantiateEntityWithShape(out entity, out shape);
            TestHelpers.SetEntityTransform(scene, entity, new Vector3(3, 3, 3), Quaternion.identity, new Vector3(5, 5, 5));

            DCLCharacterController.i.SetPosition(new Vector3(3, 2, 12));

            yield return shape.routine;

            string onPointerId = "pointerevent-1";
            var OnClickComponentModel = new OnClickComponent.Model()
            {
                type = OnClickComponent.NAME,
                uuid = onPointerId
            };
            var component = TestHelpers.EntityComponentCreate<OnClickComponent, OnClickComponent.Model>(scene, entity,
                OnClickComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(component != null);

            yield return null;

            string targetEventType = "SceneEvent";

            var onPointerEvent = new WebInterface.OnClickEvent();
            onPointerEvent.uuid = onPointerId;

            var sceneEvent = new WebInterface.SceneEvent<WebInterface.OnClickEvent>();
            sceneEvent.sceneId = scene.sceneData.id;
            sceneEvent.payload = onPointerEvent;
            sceneEvent.eventType = "uuidEvent";
            bool eventTriggered = false;

            yield return TestHelpers.WaitForEventFromEngine(targetEventType, sceneEvent,
            () =>
                {
                    DCL.InputController_Legacy.i.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER, DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true);
                },
                (pointerEvent) =>
                {
                    if (pointerEvent.eventType == sceneEvent.eventType && pointerEvent.payload.uuid == sceneEvent.payload.uuid)
                    {
                        eventTriggered = true;
                        return true;
                    }

                    return false;
                });

            Assert.IsTrue(eventTriggered);
        }

        [UnityTest]
        [Explicit("This test is failing because retrieveCamera is failing in PointerEventsController. It may be related with the new camera setup. Please check MainTest scene setup.")]
        public IEnumerator OnPointerDownEventIsTriggered()
        {
            yield return InitScene();

            DecentralandEntity entity;
            BoxShape shape;
            InstantiateEntityWithShape(out entity, out shape);
            TestHelpers.SetEntityTransform(scene, entity, new Vector3(3, 3, 3), Quaternion.identity, new Vector3(5, 5, 5));

            DCLCharacterController.i.SetPosition(new Vector3(3, 2, 12));

            yield return shape.routine;

            string onPointerId = "pointerevent-1";
            var OnPointerDownComponentModel = new OnPointerDownComponent.Model()
            {
                type = OnPointerDownComponent.NAME,
                uuid = onPointerId
            };
            var component = TestHelpers.EntityComponentCreate<OnPointerDownComponent, OnPointerDownComponent.Model>(scene, entity,
                OnPointerDownComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(component != null);

            string targetEventType = "SceneEvent";

            var onPointerDownEvent = new WebInterface.OnPointerDownEvent();
            onPointerDownEvent.uuid = onPointerId;
            onPointerDownEvent.payload = new WebInterface.OnPointerEventPayload();
            onPointerDownEvent.payload.hit = new WebInterface.OnPointerEventPayload.Hit();
            onPointerDownEvent.payload.hit.entityId = component.entity.entityId;
            onPointerDownEvent.payload.hit.meshName = component.name;

            var sceneEvent = new WebInterface.SceneEvent<WebInterface.OnPointerDownEvent>();
            sceneEvent.sceneId = scene.sceneData.id;
            sceneEvent.payload = onPointerDownEvent;
            sceneEvent.eventType = "uuidEvent";
            bool eventTriggered = false;

            yield return TestHelpers.WaitForEventFromEngine(targetEventType, sceneEvent,
                () =>
                {
                    DCL.InputController_Legacy.i.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER, DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true);
                },
                (pointerEvent) =>
                {
                    if (pointerEvent.eventType == sceneEvent.eventType &&
                        pointerEvent.payload.uuid == sceneEvent.payload.uuid &&
                        pointerEvent.payload.payload.hit.entityId == sceneEvent.payload.payload.hit.entityId)
                    {
                        eventTriggered = true;
                        return true;
                    }
                    return false;
                });

            Assert.IsTrue(eventTriggered);
        }

        [UnityTest]
        [Explicit("This test is failing because retrieveCamera is failing in PointerEventsController. It may be related with the new camera setup. Please check MainTest scene setup.")]
        public IEnumerator OnPointerUpEventIsTriggered()
        {
            yield return InitScene();

            DecentralandEntity entity;
            BoxShape shape;
            InstantiateEntityWithShape(out entity, out shape);
            TestHelpers.SetEntityTransform(scene, entity, new Vector3(3, 3, 3), Quaternion.identity, new Vector3(5, 5, 5));

            DCLCharacterController.i.SetPosition(new Vector3(3, 2, 12));

            yield return shape.routine;

            string onPointerId = "pointerevent-1";
            var OnPointerUpComponentModel = new OnPointerUpComponent.Model()
            {
                type = OnPointerUpComponent.NAME,
                uuid = onPointerId
            };
            var component = TestHelpers.EntityComponentCreate<OnPointerUpComponent, OnPointerUpComponent.Model>(scene, entity,
                OnPointerUpComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(component != null);

            string targetEventType = "SceneEvent";

            var onPointerUpEvent = new WebInterface.OnPointerUpEvent();
            onPointerUpEvent.uuid = onPointerId;
            onPointerUpEvent.payload = new WebInterface.OnPointerEventPayload();
            onPointerUpEvent.payload.hit = new WebInterface.OnPointerEventPayload.Hit();
            onPointerUpEvent.payload.hit.entityId = component.entity.entityId;
            onPointerUpEvent.payload.hit.meshName = component.name;

            var sceneEvent = new WebInterface.SceneEvent<WebInterface.OnPointerUpEvent>();
            sceneEvent.sceneId = scene.sceneData.id;
            sceneEvent.payload = onPointerUpEvent;
            sceneEvent.eventType = "uuidEvent";
            bool eventTriggered = false;

            DCL.InputController_Legacy.i.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER, DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true);

            yield return TestHelpers.WaitForEventFromEngine(targetEventType, sceneEvent,
                () =>
                {
                    DCL.InputController_Legacy.i.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER, DCL.InputController_Legacy.EVENT.BUTTON_UP, true);
                },
                (pointerEvent) =>
                {
                    if (pointerEvent.eventType == sceneEvent.eventType &&
                        pointerEvent.payload.uuid == sceneEvent.payload.uuid &&
                        pointerEvent.payload.payload.hit.entityId == sceneEvent.payload.payload.hit.entityId)
                    {
                        eventTriggered = true;
                        return true;
                    }
                    return false;
                });

            Assert.IsTrue(eventTriggered);
        }

        [UnityTest]
        public IEnumerator OnPointerDownEventWhenEntityIsBehindOther()
        {
            yield return InitScene();

            CameraController cameraController = GameObject.FindObjectOfType<CameraController>();

            // Create blocking entity
            DecentralandEntity blockingEntity;
            BoxShape blockingShape;
            InstantiateEntityWithShape(out blockingEntity, out blockingShape);
            TestHelpers.SetEntityTransform(scene, blockingEntity, new Vector3(3, 3, 3), Quaternion.identity, new Vector3(1, 1, 1));
            yield return blockingShape.routine;

            // Create target entity for click
            DecentralandEntity clickTargetEntity;
            BoxShape clickTargetShape;
            InstantiateEntityWithShape(out clickTargetEntity, out clickTargetShape);
            TestHelpers.SetEntityTransform(scene, clickTargetEntity, new Vector3(3, 3, 5), Quaternion.identity, new Vector3(1, 1, 1));
            yield return clickTargetShape.routine;

            // Set character position and camera rotation
            DCLCharacterController.i.PauseGravity();
            DCLCharacterController.i.characterController.enabled = false;
            DCLCharacterController.i.Teleport(JsonConvert.SerializeObject(new
            {
                x = 3f,
                y = 2f,
                z = 1f
            }));

            CinemachineVirtualCamera camMode = cameraController.GetComponentInChildren<CinemachineVirtualCamera>();
            camMode.Follow = DCLCharacterController.i.transform;

            var cameraRotationPayload = new CameraController.SetRotationPayload()
            {
                x = 0, y = 0, z = 0,
                cameraTarget = new Vector3(0, 0, 1)
            };
            cameraController.SetRotation(JsonConvert.SerializeObject(cameraRotationPayload, Formatting.None, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));

            yield return null;

            // Create pointer down component and add it to target entity
            string onPointerId = "pointerevent-1";
            var OnPointerDownComponentModel = new OnPointerDownComponent.Model()
            {
                type = OnPointerDownComponent.NAME,
                uuid = onPointerId
            };
            var component = TestHelpers.EntityComponentCreate<OnPointerDownComponent, OnPointerDownComponent.Model>(scene, clickTargetEntity,
                OnPointerDownComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(component != null);

            string targetEventType = "SceneEvent";

            var onPointerDownEvent = new WebInterface.OnPointerDownEvent();
            onPointerDownEvent.uuid = onPointerId;
            onPointerDownEvent.payload = new WebInterface.OnPointerEventPayload();
            onPointerDownEvent.payload.hit = new WebInterface.OnPointerEventPayload.Hit();
            onPointerDownEvent.payload.hit.entityId = component.entity.entityId;
            onPointerDownEvent.payload.hit.meshName = component.name;

            var sceneEvent = new WebInterface.SceneEvent<WebInterface.OnPointerDownEvent>();
            sceneEvent.sceneId = scene.sceneData.id;
            sceneEvent.payload = onPointerDownEvent;
            sceneEvent.eventType = "uuidEvent";

            // Check if target entity is hit behind other entity
            bool targetEntityHit = false;
            yield return TestHelpers.WaitForEventFromEngine(targetEventType, sceneEvent,
                () =>
                {
                    DCL.InputController_Legacy.i.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER, DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true);
                },
                (pointerEvent) =>
                {
                    if (pointerEvent.eventType == "uuidEvent" &&
                        pointerEvent.payload.uuid == onPointerId &&
                        pointerEvent.payload.payload.hit.entityId == clickTargetEntity.entityId)
                    {
                        targetEntityHit = true;
                    }
                    return true;
                });

            Assert.IsTrue(!targetEntityHit, "Target entity was hit but other entity was blocking it");

            // Move character in front of target entity and rotate camera
            DCLCharacterController.i.SetPosition(new Vector3(3, 2, 6));
            cameraRotationPayload.cameraTarget = new Vector3(0, 0, -1);
            cameraController.SetRotation(JsonConvert.SerializeObject(cameraRotationPayload, Formatting.None, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));
            yield return null;

            // Check if target entity is hit in front of the camera without being blocked
            targetEntityHit = false;
            yield return TestHelpers.WaitForEventFromEngine(targetEventType, sceneEvent,
                () =>
                {
                    DCL.InputController_Legacy.i.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER, DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true);
                },
                (pointerEvent) =>
                {
                    if (pointerEvent.eventType == "uuidEvent" &&
                        pointerEvent.payload.uuid == onPointerId &&
                        pointerEvent.payload.payload.hit.entityId == clickTargetEntity.entityId)
                    {
                        targetEntityHit = true;
                    }
                    return true;
                });

            Assert.IsTrue(targetEntityHit, "Target entity wasn't hit and no other entity is blocking it");
        }
    }
}
