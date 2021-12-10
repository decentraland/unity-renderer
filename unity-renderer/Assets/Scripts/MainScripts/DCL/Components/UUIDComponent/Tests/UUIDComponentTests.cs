﻿using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using DCL.Camera;
using DCL.Controllers;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests
{
    public class UUIDComponentTests : IntegrationTestSuite_Legacy
    {
        private ParcelScene scene;
        private Camera mainCamera;

        protected override List<GameObject> SetUp_LegacySystems()
        {
            List<GameObject> result = new List<GameObject>();
            result.Add(MainSceneFactory.CreateEnvironment());
            result.Add(MainSceneFactory.CreateEventSystem());
            return result;
        }

        protected override WorldRuntimeContext CreateRuntimeContext()
        {
            return DCL.Tests.WorldRuntimeContextFactory.CreateWithGenericMocks
            (
                new PointerEventsController(),
                new RuntimeComponentFactory(),
                new WorldState()
            );
        }

        protected override PlatformContext CreatePlatformContext()
        {
            return DCL.Tests.PlatformContextFactory.CreateWithGenericMocks
            (
                new UpdateEventHandler(),
                WebRequestController.Create()
            );
        }

        protected override MessagingContext CreateMessagingContext()
        {
            return DCL.Tests.MessagingContextFactory.CreateMocked();
        }

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            scene = TestUtils.CreateTestScene() as ParcelScene;

            Physics.autoSyncTransforms = true;
            CommonScriptableObjects.rendererState.Set(true);

            mainCamera = TestUtils.CreateComponentWithGameObject<Camera>("Main Camera");
            mainCamera.tag = "MainCamera";
            mainCamera.transform.position = Vector3.zero;
            mainCamera.transform.forward = Vector3.forward;

            //Debug.Log($"Setting my ID... enabled = {Environment.i.world.sceneController.enabled}");
            DCL.Environment.i.world.state.currentSceneId = scene.sceneData.id;
        }

        protected override IEnumerator TearDown()
        {
            Object.Destroy(mainCamera.gameObject);
            yield return base.TearDown();
        }

        void InstantiateEntityWithShape(out IDCLEntity entity, out BoxShape shape)
        {
            shape = TestUtils.InstantiateEntityWithShape<BoxShape, BoxShape.Model>(
                scene,
                DCL.Models.CLASS_ID.BOX_SHAPE,
                Vector3.zero,
                out entity,
                new BoxShape.Model() { });
        }

        [UnityTest]
        public IEnumerator OnClickComponentInitializesWithBasicShape()
        {
            IDCLEntity entity;
            BoxShape shape;
            InstantiateEntityWithShape(out entity, out shape);

            yield return shape.routine;

            string onPointerId = "pointerevent-1";
            var OnClickComponentModel = new OnClick.Model()
            {
                type = OnClick.NAME,
                uuid = onPointerId
            };
            var component = TestUtils.EntityComponentCreate<OnClick, OnClick.Model>(scene, entity,
                OnClickComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            var componentGO = component.gameObject;

            var meshFilter = component.entity.gameObject.GetComponentInChildren<MeshFilter>();
            var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

            Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                "OnPointerEventCollider should have the same mesh info as the mesh renderer");
        }

        [UnityTest]
        public IEnumerator OnPointerDownInitializesWithBasicShape()
        {
            IDCLEntity entity;
            BoxShape shape;
            InstantiateEntityWithShape(out entity, out shape);

            yield return shape.routine;

            string onPointerId = "pointerevent-1";
            var OnPointerDownModel = new OnPointerDown.Model()
            {
                type = "pointerUp",
                uuid = onPointerId
            };

            var component = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, entity,
                OnPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            var meshFilter = entity.gameObject.GetComponentInChildren<MeshFilter>();
            var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

            Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                "OnPointerEventCollider should have the same mesh info as the mesh renderer");
        }

        [UnityTest]
        public IEnumerator OnPointerUpComponentInitializesWithBasicShape()
        {
            IDCLEntity entity;
            BoxShape shape;
            InstantiateEntityWithShape(out entity, out shape);

            yield return shape.routine;

            string onPointerId = "pointerevent-1";
            var OnPointerUpComponentModel = new OnPointerUp.Model()
            {
                type = "pointerUp",
                uuid = onPointerId
            };
            var component = TestUtils.EntityComponentCreate<OnPointerUp, OnPointerUp.Model>(scene, entity,
                OnPointerUpComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            var meshFilter = entity.gameObject.GetComponentInChildren<MeshFilter>();
            var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

            Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                "OnPointerEventCollider should have the same mesh info as the mesh renderer");
        }

        [UnityTest]
        public IEnumerator OnClickComponentInitializesWithGLTFShape()
        {
            string entityId = "1";

            TestUtils.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            string shapeId = TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
                JsonConvert.SerializeObject(new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb"
                }));

            LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(scene.entities[entityId]);
            yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded, 7f);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            string clickUuid = "pointerevent-1";
            var OnClickComponentModel = new OnClick.Model()
            {
                type = OnClick.NAME,
                uuid = clickUuid
            };

            var component = TestUtils.EntityComponentCreate<OnClick, OnClick.Model>(scene, scene.entities[entityId],
                OnClickComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            var componentGO = component.gameObject;

            foreach (var meshFilter in componentGO.GetComponentsInChildren<MeshFilter>())
            {
                var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

                Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

                Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                    "OnPointerEventCollider should have the same mesh info as the mesh renderer");
            }
        }

        [UnityTest]
        public IEnumerator OnPointerDownInitializesWithGLTFShape()
        {
            string entityId = "1";

            TestUtils.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            string shapeId = TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
                JsonConvert.SerializeObject(new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb"
                }));

            LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(scene.entities[entityId]);
            yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded, 7f);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            string clickUuid = "pointerevent-1";
            var OnPointerDownModel = new OnPointerDown.Model()
            {
                type = "pointerDown",
                uuid = clickUuid
            };

            var component = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, scene.entities[entityId],
                OnPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

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
            string entityId = "1";

            TestUtils.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            string shapeId = TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
                JsonConvert.SerializeObject(new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb"
                }));

            LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(scene.entities[entityId]);
            yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded, 7f);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            string clickUuid = "pointerevent-1";
            var OnPointerUpComponentModel = new OnPointerUp.Model()
            {
                type = "pointerUp",
                uuid = clickUuid
            };

            var component = TestUtils.EntityComponentCreate<OnPointerUp, OnPointerUp.Model>(scene, scene.entities[entityId],
                OnPointerUpComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

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
            string entityId = "1";
            TestUtils.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb"
                }));

            string clickUuid = "pointerevent-1";
            var OnClickComponentModel = new OnClick.Model()
            {
                type = OnClick.NAME,
                uuid = clickUuid
            };

            var uuidComponent = TestUtils.EntityComponentCreate<OnClick, OnClick.Model>(scene, scene.entities[entityId],
                OnClickComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(scene.entities[entityId]);
            yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded, 7f);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

            foreach (var meshFilter in scene.entities[entityId].gameObject.GetComponentsInChildren<MeshFilter>())
            {
                var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

                Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

                Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                    "OnPointerEventCollider should have the same mesh info as the mesh renderer");
            }
        }

        [UnityTest]
        public IEnumerator OnPointerDownInitializesWithGLTFShapeAsynchronously()
        {
            string entityId = "1";
            TestUtils.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb"
                }));

            string clickUuid = "pointerevent-1";
            var OnPointerDownModel = new OnPointerDown.Model()
            {
                type = "pointerDown",
                uuid = clickUuid
            };
            TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, scene.entities[entityId],
                OnPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(scene.entities[entityId]);
            yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded, 7f);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

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
            string entityId = "1";
            TestUtils.CreateSceneEntity(scene, entityId);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb"
                }));

            string clickUuid = "pointerevent-1";
            var OnPointerUpModel = new OnPointerUp.Model()
            {
                type = "pointerUp",
                uuid = clickUuid
            };

            var component = TestUtils.EntityComponentCreate<OnPointerUp, OnPointerUp.Model>(scene, scene.entities[entityId],
                OnPointerUpModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(scene.entities[entityId]);
            yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded, 7f);

            Assert.IsTrue(
                scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null,
                "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

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
            string entityId = "1";
            TestUtils.CreateSceneEntity(scene, entityId);

            string clickUuid = "pointerevent-1";
            var OnClickComponentModel = new OnClick.Model()
            {
                type = OnClick.NAME,
                uuid = clickUuid
            };

            var component = TestUtils.EntityComponentCreate<OnClick, OnClick.Model>(scene, scene.entities[entityId],
                OnClickComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(component != null, "component is null?");

            yield return component.routine;

            Assert.IsTrue(component.gameObject.GetComponent<Rigidbody>() == null,
                "the root object shouldn't have a rigidbody attached until a shape is added");

            Assert.IsTrue(component.gameObject.transform.Find(OnPointerEventColliders.COLLIDER_NAME) == null,
                "the OnPointerEventCollider object shouldn't exist until a shape is added");

            TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE,
                JsonConvert.SerializeObject(new BoxShape.Model { })
            );

            var meshFilter = component.entity.gameObject.GetComponentInChildren<MeshFilter>();
            var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

            Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                "OnPointerEventCollider should have the same mesh info as the mesh renderer");
        }

        [UnityTest]
        public IEnumerator OnPointerDownInitializesAfterBasicShapeIsAdded()
        {
            string entityId = "1";
            TestUtils.CreateSceneEntity(scene, entityId);

            string clickUuid = "pointerevent-1";
            var OnPointerDownModel = new OnPointerDown.Model()
            {
                type = "pointerDown",
                uuid = clickUuid
            };

            var uuidComponent = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, scene.entities[entityId],
                OnPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(uuidComponent.gameObject.GetComponent<Rigidbody>() == null,
                "the root object shouldn't have a rigidbody attached until a shape is added");

            Assert.IsTrue(scene.entities[entityId].gameObject.transform.Find(OnPointerEventColliders.COLLIDER_NAME) == null,
                "the OnPointerEventCollider object shouldn't exist until a shape is added");

            TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE,
                JsonConvert.SerializeObject(new BoxShape.Model { })
            );

            var meshFilter = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>();
            var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

            Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                "OnPointerEventCollider should have the same mesh info as the mesh renderer");

            yield break;
        }

        [UnityTest]
        public IEnumerator OnPointerUpComponentInitializesAfterBasicShapeIsAdded()
        {
            string entityId = "1";
            TestUtils.CreateSceneEntity(scene, entityId);

            string clickUuid = "pointerevent-1";
            var OnPointerUpComponentModel = new OnPointerUp.Model()
            {
                type = "pointerUp",
                uuid = clickUuid
            };

            var component = TestUtils.EntityComponentCreate<OnPointerUp, OnPointerUp.Model>(scene, scene.entities[entityId],
                OnPointerUpComponentModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(component.gameObject.GetComponent<Rigidbody>() == null,
                "the root object shouldn't have a rigidbody attached until a shape is added");

            Assert.IsTrue(scene.entities[entityId].gameObject.transform.Find(OnPointerEventColliders.COLLIDER_NAME) == null,
                "the OnPointerEventCollider object shouldn't exist until a shape is added");

            TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE,
                JsonConvert.SerializeObject(new BoxShape.Model { })
            );

            var meshFilter = scene.entities[entityId].gameObject.GetComponentInChildren<MeshFilter>();
            var onPointerEventCollider = meshFilter.transform.Find(OnPointerEventColliders.COLLIDER_NAME);

            Assert.IsTrue(onPointerEventCollider != null, "OnPointerEventCollider should exist under any rendeder");

            Assert.AreSame(meshFilter.sharedMesh, onPointerEventCollider.GetComponent<MeshCollider>().sharedMesh,
                "OnPointerEventCollider should have the same mesh info as the mesh renderer");

            yield break;
        }

        [UnityTest]
        public IEnumerator OnClickEventIsTriggered()
        {
            InstantiateEntityWithShape(out IDCLEntity entity, out BoxShape shape);
            TestUtils.SetEntityTransform(scene, entity, new Vector3(9f, 1.5f, 11.0f), Quaternion.identity, new Vector3(5, 5, 5));

            mainCamera.transform.position = new Vector3(3, 2, 12);
            mainCamera.transform.forward = Vector3.forward;

            yield return shape.routine;

            string onPointerId = "pointerevent-1";
            var OnClickComponentModel = new OnPointerEvent.Model()
            {
                type = OnClick.NAME,
                uuid = onPointerId
            };

            var component = TestUtils.EntityComponentCreate<OnClick, OnPointerEvent.Model>(
                scene,
                entity,
                OnClickComponentModel,
                CLASS_ID_COMPONENT.UUID_CALLBACK
            );

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

            Debug.Log("Current Scene " + DCL.Environment.i.world.state.currentSceneId);

            yield return TestUtils.ExpectMessageToKernel(
                targetEventType,
                sceneEvent,
                () =>
                {
                    InputController_Legacy.i.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                        DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true, true);
                },
                (eventObj) =>
                {
                    if (eventTriggered)
                        return true;

                    if (eventObj.eventType != sceneEvent.eventType)
                        return false;

                    if (eventObj.payload.uuid != sceneEvent.payload.uuid)
                        return false;

                    eventTriggered = true;
                    return true;
                });

            Debug.Log("Fail time");

            Assert.IsTrue(eventTriggered);
        }

        [UnityTest]
        public IEnumerator OnPointerDownEventIsTriggered()
        {
            InstantiateEntityWithShape(out IDCLEntity entity, out BoxShape shape);
            TestUtils.SetEntityTransform(scene, entity, new Vector3(9f, 1.5f, 11.0f), Quaternion.identity, new Vector3(5, 5, 5));

            mainCamera.transform.position = new Vector3(3, 2, 12);
            mainCamera.transform.forward = Vector3.right;

            yield return shape.routine;

            string onPointerId = "pointerevent-1";
            var OnPointerDownModel = new OnPointerEvent.Model()
            {
                type = OnPointerDown.NAME,
                uuid = onPointerId
            };
            var component = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, entity,
                OnPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

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

            yield return TestUtils.ExpectMessageToKernel(targetEventType, sceneEvent,
                () =>
                {
                    DCL.InputController_Legacy.i.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                        DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true, true);
                },
                (pointerEvent) =>
                {
                    if (eventTriggered)
                        return true;

                    //Debug.Log($"triggered? \npointerEvent {JsonUtility.ToJson(pointerEvent, true)}\nsceneEvent {JsonUtility.ToJson(sceneEvent, true)}");

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
        public IEnumerator OnPointerUpEventIsTriggered()
        {
            IDCLEntity entity;
            BoxShape shape;
            InstantiateEntityWithShape(out entity, out shape);
            TestUtils.SetEntityTransform(scene, entity, new Vector3(9f, 1.5f, 11.0f), Quaternion.identity, new Vector3(5, 5, 5));

            mainCamera.transform.position = new Vector3(3, 2, 12);
            mainCamera.transform.forward = Vector3.right;

            yield return shape.routine;

            string onPointerId = "pointerevent-1";
            var OnPointerUpComponentModel = new OnPointerUp.Model()
            {
                type = OnPointerUp.NAME,
                uuid = onPointerId
            };
            var component = TestUtils.EntityComponentCreate<OnPointerUp, OnPointerUp.Model>(scene, entity,
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

            DCL.InputController_Legacy.i.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true, true);

            yield return TestUtils.ExpectMessageToKernel(targetEventType, sceneEvent,
                () =>
                {
                    DCL.InputController_Legacy.i.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                        DCL.InputController_Legacy.EVENT.BUTTON_UP, true, true);
                },
                (pointerEvent) =>
                {
                    if (eventTriggered)
                        return true;

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
        public IEnumerator OnPointerUpEventNotTriggeredOnInvisibles()
        {
            IDCLEntity entity;
            BoxShape shape;
            InstantiateEntityWithShape(out entity, out shape);
            TestUtils.SetEntityTransform(scene, entity, new Vector3(9f, 1.5f, 11.0f), Quaternion.identity, new Vector3(5, 5, 5));

            mainCamera.transform.position = new Vector3(3, 2, 12);
            mainCamera.transform.forward = Vector3.right;

            yield return shape.routine;

            string onPointerId = "pointerevent-1";
            var OnPointerUpComponentModel = new OnPointerUp.Model()
            {
                type = OnPointerUp.NAME,
                uuid = onPointerId
            };
            var component = TestUtils.EntityComponentCreate<OnPointerUp, OnPointerUp.Model>(scene, entity,
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

            bool eventTriggered1 = false;
            DCL.InputController_Legacy.i.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true, true);

            yield return TestUtils.ExpectMessageToKernel(targetEventType, sceneEvent,
                () =>
                {
                    DCL.InputController_Legacy.i.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                        DCL.InputController_Legacy.EVENT.BUTTON_UP, true, true);
                },
                (pointerEvent) =>
                {
                    if (eventTriggered1)
                        return true;

                    if (pointerEvent.eventType == sceneEvent.eventType &&
                        pointerEvent.payload.uuid == sceneEvent.payload.uuid &&
                        pointerEvent.payload.payload.hit.entityId == sceneEvent.payload.payload.hit.entityId)
                    {
                        eventTriggered1 = true;
                        return true;
                    }

                    return false;
                });

            Assert.IsTrue(eventTriggered1);

            // turn shape invisible
            TestUtils.UpdateShape(scene, shape.id, JsonConvert.SerializeObject(new { visible = false }));
            DCL.InputController_Legacy.i.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true, true);

            var pointerUpReceived = false;

            void MsgFromEngineCallback(string eventType, string eventPayload)
            {
                if (string.IsNullOrEmpty(eventPayload) || eventType != targetEventType)
                    return;

                var pointerEvent = JsonUtility.FromJson<WebInterface.SceneEvent<WebInterface.OnPointerUpEvent>>(eventPayload);
                if (pointerEvent.eventType == sceneEvent.eventType
                    && pointerEvent.payload.uuid == sceneEvent.payload.uuid
                    && pointerEvent.payload.payload.hit.entityId == sceneEvent.payload.payload.hit.entityId)
                {
                    pointerUpReceived = true;
                }
            }

            // Hook up to web interface engine message reporting
            WebInterface.OnMessageFromEngine += MsgFromEngineCallback;
            InputController_Legacy.i.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                InputController_Legacy.EVENT.BUTTON_UP, true, true);
            WebInterface.OnMessageFromEngine -= MsgFromEngineCallback;

            Assert.IsFalse(pointerUpReceived);
        }

        [UnityTest]
        public IEnumerator OnPointerDownEventWhenEntityIsBehindOther()
        {
            // Create blocking entity
            IDCLEntity blockingEntity;
            BoxShape blockingShape;
            InstantiateEntityWithShape(out blockingEntity, out blockingShape);
            TestUtils.SetEntityTransform(scene, blockingEntity, new Vector3(3, 3, 3), Quaternion.identity, new Vector3(1, 1, 1));
            yield return blockingShape.routine;

            // Create target entity for click
            IDCLEntity clickTargetEntity;
            BoxShape clickTargetShape;
            InstantiateEntityWithShape(out clickTargetEntity, out clickTargetShape);
            TestUtils.SetEntityTransform(scene, clickTargetEntity, new Vector3(3, 3, 5), Quaternion.identity, new Vector3(1, 1, 1));
            yield return clickTargetShape.routine;

            // Set character position and camera rotation
            mainCamera.transform.position = new Vector3(3, 2, 1);

            yield return null;

            // Create pointer down component and add it to target entity
            string onPointerId = "pointerevent-1";
            var OnPointerDownModel = new OnPointerDown.Model()
            {
                type = OnPointerDown.NAME,
                uuid = onPointerId
            };
            var component = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, clickTargetEntity,
                OnPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

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
            yield return TestUtils.ExpectMessageToKernel(targetEventType, sceneEvent,
                () =>
                {
                    DCL.InputController_Legacy.i.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                        DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true, true);
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
            mainCamera.transform.position = new Vector3(3, 2, 6);
            mainCamera.transform.forward = Vector3.back;

            yield return null;

            // Check if target entity is hit in front of the camera without being blocked
            targetEntityHit = false;
            yield return TestUtils.ExpectMessageToKernel(targetEventType, sceneEvent,
                () =>
                {
                    DCL.InputController_Legacy.i.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                        DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true, true);
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

            yield return null;
            Assert.IsTrue(targetEntityHit, "Target entity wasn't hit and no other entity is blocking it");
        }

        [UnityTest]
        public IEnumerator OnPointerDownEventAndPointerBlockerShape()
        {
            // Create blocking entity
            IDCLEntity blockingEntity;
            BoxShape blockingShape;
            InstantiateEntityWithShape(out blockingEntity, out blockingShape);
            TestUtils.SetEntityTransform(scene, blockingEntity, new Vector3(3, 3, 3), Quaternion.identity, new Vector3(1, 1, 1));

            yield return blockingShape.routine;

            // Create target entity for click
            IDCLEntity clickTargetEntity;
            BoxShape clickTargetShape;
            InstantiateEntityWithShape(out clickTargetEntity, out clickTargetShape);
            TestUtils.SetEntityTransform(scene, clickTargetEntity, new Vector3(3, 3, 5), Quaternion.identity, new Vector3(1, 1, 1));

            yield return clickTargetShape.routine;

            // Set character position and camera rotation
            mainCamera.transform.position = new Vector3(3, 3, 1);

            yield return null;

            // Create pointer down component and add it to target entity
            string onPointerId = "pointerevent-1";
            var OnPointerDownModel = new OnPointerDown.Model()
            {
                type = OnPointerDown.NAME,
                uuid = onPointerId
            };

            var component = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, clickTargetEntity,
                OnPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            yield return component.routine;

            Assert.IsTrue(component != null);
            Assert.IsTrue(clickTargetEntity != null);
            Assert.IsTrue(component.entity != null);

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

            // Check the target entity is not hit behind the 'isPointerBlocker' shape
            bool targetEntityHit = false;
            yield return TestUtils.ExpectMessageToKernel(targetEventType, sceneEvent,
                () =>
                {
                    DCL.InputController_Legacy.i.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                        DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true, true);
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

            Assert.IsFalse(targetEntityHit, "Target entity was hit but other entity was blocking it");

            // Toggle 'isPointerBlocker' property
            yield return TestUtils.SharedComponentUpdate(blockingShape, new BoxShape.Model
            {
                isPointerBlocker = false
            });

            // Check the target entity is hit behind the 'isPointerBlocker' shape now
            targetEntityHit = false;
            yield return TestUtils.ExpectMessageToKernel(targetEventType, sceneEvent,
                () =>
                {
                    DCL.InputController_Legacy.i.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                        DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true, true);
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

            yield return null;

            yield return new WaitForSeconds(5.0f);

            Assert.IsTrue(targetEntityHit, "Target entity wasn't hit and no other entity is blocking it");
        }

        [UnityTest]
        public IEnumerator PointerEventNotTriggeredByParent()
        {
            // Create parent entity
            InstantiateEntityWithShape(out IDCLEntity blockingEntity, out BoxShape blockingShape);
            TestUtils.SetEntityTransform(scene, blockingEntity, new Vector3(3, 3, 3), Quaternion.identity, new Vector3(1, 1, 1));
            yield return blockingShape.routine;

            // Create target entity for click
            IDCLEntity clickTargetEntity;
            BoxShape clickTargetShape;
            InstantiateEntityWithShape(out clickTargetEntity, out clickTargetShape);
            TestUtils.SetEntityTransform(scene, clickTargetEntity, new Vector3(0, 0, 5), Quaternion.identity, new Vector3(1, 1, 1));
            yield return clickTargetShape.routine;

            // Enparent target entity as a child of the blocking entity
            TestUtils.SetEntityParent(scene, clickTargetEntity, blockingEntity);

            // Set character position and camera rotation
            mainCamera.transform.position = new Vector3(3, 2, 1);

            yield return null;

            // Create pointer down component and add it to target entity
            string onPointerId = "pointerevent-1";
            var OnPointerDownModel = new OnPointerDown.Model()
            {
                type = OnPointerDown.NAME,
                uuid = onPointerId
            };
            var component = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, clickTargetEntity,
                OnPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

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

            // Check if target entity is triggered by hitting the parent entity
            bool targetEntityHit = false;
            yield return TestUtils.ExpectMessageToKernel(targetEventType, sceneEvent,
                () =>
                {
                    DCL.InputController_Legacy.i.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                        DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true, true);
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

            Assert.IsFalse(targetEntityHit, "Target entity was hit but other entity was blocking it");

            // Move character in front of target entity and rotate camera
            mainCamera.transform.position = new Vector3(3, 2, 12);
            mainCamera.transform.forward = Vector3.back;

            yield return null;

            // Check if target entity is triggered when hit directly
            targetEntityHit = false;
            yield return TestUtils.ExpectMessageToKernel(targetEventType, sceneEvent,
                () =>
                {
                    DCL.InputController_Legacy.i.RaiseEvent(WebInterface.ACTION_BUTTON.POINTER,
                        DCL.InputController_Legacy.EVENT.BUTTON_DOWN, true, true);
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

            yield return null;
            Assert.IsTrue(targetEntityHit, "Target entity wasn't hit and no other entity is blocking it");
        }

        [UnityTest]
        public IEnumerator OnPointerHoverFeedbackPropertiesAreAppliedCorrectly()
        {
            IDCLEntity entity;
            BoxShape shape;
            InstantiateEntityWithShape(out entity, out shape);
            TestUtils.SetEntityTransform(scene, entity, new Vector3(8, 2, 10), Quaternion.identity, new Vector3(3, 3, 3));
            yield return shape.routine;

            var onPointerDownModel = new OnPointerDown.Model()
            {
                type = OnPointerDown.NAME,
                uuid = "pointerevent-1"
            };
            var component = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, entity,
                onPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);
            Assert.IsTrue(component != null);

            yield return null;

            mainCamera.transform.position = new Vector3(8, 1, 7);

            // Rotate camera towards the interactive object
            mainCamera.transform.position = new Vector3(3, 2, 12);
            mainCamera.transform.rotation = Quaternion.Euler(45, 0, 0);

            yield return null;

            var hoverCanvasController = InteractionHoverCanvasController.i;
            Assert.IsNotNull(hoverCanvasController);
            Assert.IsTrue(hoverCanvasController.canvas.enabled);

            // Check default properties
            Assert.AreEqual("AnyButtonHoverIcon", hoverCanvasController.GetCurrentHoverIcon().name);
            Assert.AreEqual("Interact", hoverCanvasController.text.text);
            yield return null;

            onPointerDownModel.button = "PRIMARY";
            onPointerDownModel.hoverText = "Click!";

            // we can't use TestHelpers.EntityComponentUpdate() to update UUIDComponents until we separate every UUIComponent to their own new CLASS_ID_COMPONENT
            component = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, entity,
                onPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            yield return null;

            Assert.AreEqual("PrimaryButtonHoverIcon", hoverCanvasController.GetCurrentHoverIcon().name);
            Assert.AreEqual("Click!", hoverCanvasController.text.text);
        }

        [UnityTest]
        public IEnumerator OnPointerHoverDistanceIsAppliedCorrectly()
        {
            IDCLEntity entity;
            BoxShape shape;
            InstantiateEntityWithShape(out entity, out shape);
            TestUtils.SetEntityTransform(scene, entity, new Vector3(8, 2, 10), Quaternion.identity, new Vector3(3, 3, 3));
            yield return shape.routine;

            var onPointerDownModel = new OnPointerDown.Model()
            {
                type = OnPointerDown.NAME,
                uuid = "pointerevent-1"
            };
            var component = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, entity,
                onPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);
            Assert.IsTrue(component != null);

            yield return null;

            mainCamera.transform.position = new Vector3(8, 1, 7);

            // Rotate camera towards the interactive object
            mainCamera.transform.rotation = Quaternion.Euler(45, 0, 0);

            yield return null;

            var hoverCanvas = InteractionHoverCanvasController.i.canvas;
            Assert.IsNotNull(hoverCanvas);

            Assert.IsTrue(hoverCanvas.enabled);
            yield return null;

            onPointerDownModel.distance = 1f;
            // we can't use TestHelpers.EntityComponentUpdate() to update UUIDComponents until we separate every UUIComponent to their own new CLASS_ID_COMPONENT
            component = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, entity,
                onPointerDownModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            yield return null;

            Assert.IsFalse(hoverCanvas.enabled);
            Object.Destroy(component);
        }
    }
}