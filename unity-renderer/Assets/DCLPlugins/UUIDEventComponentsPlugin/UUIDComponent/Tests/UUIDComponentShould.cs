using System.Collections;
using System.Linq;
using DCL;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Environment = DCL.Environment;
using WaitUntil = UnityEngine.WaitUntil;

namespace Tests
{
    public class UUIDComponentShould : IntegrationTestSuite
    {
        private ParcelScene scene;
        private UUIDEventsPlugin uuidEventsPlugin;
        private CoreComponentsPlugin coreComponentsPlugin;

        protected override void InitializeServices(ServiceLocator serviceLocator)
        {
            serviceLocator.Register<ISceneController>(() => new SceneController());
            serviceLocator.Register<IWorldState>(() => new WorldState());
            serviceLocator.Register<IRuntimeComponentFactory>(() => new RuntimeComponentFactory());
            serviceLocator.Register<IWebRequestController>(WebRequestController.Create);
        }

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            scene = TestUtils.CreateTestScene();
            uuidEventsPlugin = new UUIDEventsPlugin();
            coreComponentsPlugin = new CoreComponentsPlugin();
        }

        [UnityTearDown]
        protected override IEnumerator TearDown()
        {
            uuidEventsPlugin.Dispose();
            coreComponentsPlugin.Dispose();
            yield return base.TearDown();
        }

        [UnityTest]
        public IEnumerator BeDestroyedCorrectlyWhenReceivingComponentDestroyMessage()
        {
            var shape = TestUtils.CreateEntityWithBoxShape(scene, Vector3.zero, true);
            IDCLEntity entity = shape.attachedEntities.First();

            yield return shape.routine;

            string onPointerId = "pointerevent-1";
            var model = new OnClick.Model()
            {
                type = OnClick.NAME,
                uuid = onPointerId
            };

            TestUtils.EntityComponentCreate<OnClick, OnClick.Model>(scene, entity,
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);

            model.type = OnPointerDown.NAME;

            TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene, entity,
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);

            model.type = OnPointerUp.NAME;

            TestUtils.EntityComponentCreate<OnPointerUp, OnPointerUp.Model>(scene, entity,
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);

            var hoverModel = new OnPointerHoverEvent.Model
            {
                type = OnPointerHoverEnter.NAME,
                uuid = onPointerId
            };

            TestUtils.EntityComponentCreate<OnPointerHoverEnter, OnPointerHoverEvent.Model>(scene, entity,
                hoverModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            hoverModel.type = OnPointerHoverExit.NAME;

            TestUtils.EntityComponentCreate<OnPointerHoverExit, OnPointerHoverEvent.Model>(scene, entity,
                hoverModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            Assert.IsTrue(scene.componentsManagerLegacy.HasComponent(entity, CLASS_ID_COMPONENT.UUID_ON_CLICK));
            Assert.IsTrue(scene.componentsManagerLegacy.HasComponent(entity, CLASS_ID_COMPONENT.UUID_ON_UP));
            Assert.IsTrue(scene.componentsManagerLegacy.HasComponent(entity, CLASS_ID_COMPONENT.UUID_ON_DOWN));
            Assert.IsTrue(scene.componentsManagerLegacy.HasComponent(entity, CLASS_ID_COMPONENT.UUID_ON_HOVER_EXIT));
            Assert.IsTrue(scene.componentsManagerLegacy.HasComponent(entity, CLASS_ID_COMPONENT.UUID_ON_HOVER_ENTER));

            scene.componentsManagerLegacy.EntityComponentRemove(entity.entityId, OnPointerDown.NAME);
            scene.componentsManagerLegacy.EntityComponentRemove(entity.entityId, OnPointerUp.NAME);
            scene.componentsManagerLegacy.EntityComponentRemove(entity.entityId, OnClick.NAME);
            scene.componentsManagerLegacy.EntityComponentRemove(entity.entityId, OnPointerHoverEnter.NAME);
            scene.componentsManagerLegacy.EntityComponentRemove(entity.entityId, OnPointerHoverExit.NAME);

            Assert.IsFalse(scene.componentsManagerLegacy.HasComponent(entity, CLASS_ID_COMPONENT.UUID_ON_CLICK));
            Assert.IsFalse(scene.componentsManagerLegacy.HasComponent(entity, CLASS_ID_COMPONENT.UUID_ON_UP));
            Assert.IsFalse(scene.componentsManagerLegacy.HasComponent(entity, CLASS_ID_COMPONENT.UUID_ON_DOWN));
            Assert.IsFalse(scene.componentsManagerLegacy.HasComponent(entity, (CLASS_ID_COMPONENT.UUID_ON_HOVER_EXIT)));
            Assert.IsFalse(scene.componentsManagerLegacy.HasComponent(entity, CLASS_ID_COMPONENT.UUID_ON_HOVER_ENTER));
        }

        [UnityTest]
        public IEnumerator NotCreateCollidersOnLoadingShape()
        {
            // 1. Instantiate entity and add an OnPointerDown component
            long entityId = 1;
            var entity = TestUtils.CreateSceneEntity(scene, entityId);

            string onPointerId = "pointerevent-1";
            var model = new OnPointerEvent.Model()
            {
                type = OnPointerDown.NAME,
                uuid = onPointerId
            };
            var onPointerDownComponent = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerEvent.Model>(scene,
                entity,
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);

            // 2. Attach a shape
            var shapeModel = new LoadableShape.Model();
            shapeModel.src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb";
            var componentId = TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
                JsonConvert.SerializeObject(shapeModel));

            // 3. Change a shape component property while it loads
            shapeModel.visible = false;
            TestUtils.UpdateShape(scene, componentId, JsonConvert.SerializeObject(shapeModel));

            var pointerEventColliders = onPointerDownComponent.pointerEventHandler.eventColliders.colliders;
            Assert.IsTrue(pointerEventColliders == null || pointerEventColliders.Length == 0);

            LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(scene.entities[entityId]);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            // 4. Check if the PointerEvent colliders were created
            pointerEventColliders = onPointerDownComponent.pointerEventHandler.eventColliders.colliders;
            Assert.IsTrue(pointerEventColliders != null && pointerEventColliders.Length > 0);
        }

        [UnityTest]
        public IEnumerator OnHoverEventComponentNotCreateCollidersOnLoadingShape()
        {
            // 1. Instantiate entity and add an OnPointerHoverEnter component
            long entityId = 1;
            var entity = TestUtils.CreateSceneEntity(scene, entityId);

            string onPointerId = "pointerevent-1";
            var model = new OnPointerHoverEvent.Model()
            {
                type = OnPointerHoverEnter.NAME,
                uuid = onPointerId
            };
            var onHoverEnterComponent = TestUtils.EntityComponentCreate<OnPointerHoverEnter, OnPointerHoverEvent.Model>(
                scene, entity,
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);

            // 2. Attach a shape
            var shapeModel = new LoadableShape.Model();
            shapeModel.src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb";
            var componentId = TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
                JsonConvert.SerializeObject(shapeModel));

            // 3. Change a shape component property while it loads
            shapeModel.visible = false;
            TestUtils.UpdateShape(scene, componentId, JsonConvert.SerializeObject(shapeModel));

            var pointerEventColliders = onHoverEnterComponent.pointerEventColliders.colliders;
            Assert.IsTrue(pointerEventColliders == null || pointerEventColliders.Length == 0);

            LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(scene.entities[entityId]);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            // 4. Check if the PointerEvent colliders were created
            pointerEventColliders = onHoverEnterComponent.pointerEventColliders.colliders;
            Assert.IsTrue(pointerEventColliders != null && pointerEventColliders.Length > 0);
        }

        [UnityTest]
        public IEnumerator NotRecreateCollidersWhenShapeDoesntChange()
        {
            // 1. Instantiate entity and add an OnPointerDown component
            long entityId = 1;
            var entity = TestUtils.CreateSceneEntity(scene, entityId);

            string onPointerId = "pointerevent-1";
            var model = new OnPointerDown.Model()
            {
                type = OnPointerDown.NAME,
                uuid = onPointerId
            };
            var onPointerDownComponent = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene,
                entity,
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);

            // 2. Attach a shape
            var shapeModel = new LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>.Model();
            shapeModel.src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb";
            var componentId = TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
                JsonConvert.SerializeObject(shapeModel));

            LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(scene.entities[entityId]);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            var pointerEventColliders = onPointerDownComponent.pointerEventHandler.eventColliders.colliders;
            Assert.IsTrue(pointerEventColliders != null || pointerEventColliders.Length > 0);

            // 3. Change a shape component property conserving the same glb
            shapeModel.visible = false;
            TestUtils.UpdateShape(scene, componentId, JsonConvert.SerializeObject(shapeModel));
            yield return null;

            // 4. Check the same colliders were kept
            Assert.IsTrue(pointerEventColliders == onPointerDownComponent.pointerEventHandler.eventColliders.colliders);
        }

        [UnityTest]
        public IEnumerator OnHoverEventNotRecreateCollidersWhenShapeDoesntChange()
        {
            // 1. Instantiate entity and add an OnPointerHoverEnter component
            long entityId = 1;
            var entity = TestUtils.CreateSceneEntity(scene, entityId);

            string onPointerId = "pointerevent-1";
            var model = new OnPointerHoverEvent.Model()
            {
                type = OnPointerHoverEnter.NAME,
                uuid = onPointerId
            };
            var onHoverEnterComponent = TestUtils.EntityComponentCreate<OnPointerHoverEnter, OnPointerHoverEvent.Model>(
                scene, entity,
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);

            // 2. Attach a shape
            var shapeModel = new LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>.Model();
            shapeModel.src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb";
            var componentId = TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
                JsonConvert.SerializeObject(shapeModel));

            LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(scene.entities[entityId]);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            var pointerEventColliders = onHoverEnterComponent.pointerEventColliders.colliders;
            Assert.IsTrue(pointerEventColliders != null || pointerEventColliders.Length > 0);

            // 3. Change a shape component property conserving the same glb
            shapeModel.visible = false;
            TestUtils.UpdateShape(scene, componentId, JsonConvert.SerializeObject(shapeModel));
            yield return null;

            // 4. Check the same colliders were kept
            Assert.IsTrue(pointerEventColliders == onHoverEnterComponent.pointerEventColliders.colliders);
        }

        [UnityTest]
        public IEnumerator NotLeaveCollidersOnRecycledMeshes()
        {
            // 1. Instantiate entity and add an OnPointerDown component
            long entityId = 1;
            var entity1 = TestUtils.CreateSceneEntity(scene, entityId);

            string onPointerId = "pointerevent-1";
            var onPointerEventModel = new OnPointerDown.Model()
            {
                type = OnPointerDown.NAME,
                uuid = onPointerId
            };
            var onPointerDownComponent = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene,
                entity1,
                onPointerEventModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            // 2. Attach a shape
            var shapeModel = new LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>.Model();
            shapeModel.src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb";
            var shapeComponentId = TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
                JsonConvert.SerializeObject(shapeModel));

            LoadWrapper gltfShapeLoader1 = Environment.i.world.state.GetLoaderForEntity(scene.entities[entityId]);

            yield return new WaitUntil(() => gltfShapeLoader1.alreadyLoaded);
            yield return null;

            // 3. Save the mesh GO reference
            Transform shapeInstanceRootTransform = entity1.meshRootGameObject.transform.GetChild(0);

            // 4. Remove shape so that it returns to its pool
            scene.componentsManagerLegacy.RemoveSharedComponent(entity1, typeof(BaseShape));
            yield return null;

            // 5. Check that the pooled mesh doesn't have the collider children and the onPointerEvent component
            // doesn't have any instantiated collider (since its entity doesn't have a mesh now)
            var childMeshColliders = shapeInstanceRootTransform.GetComponentsInChildren<MeshCollider>(true);
            foreach (MeshCollider collider in childMeshColliders)
            {
                Assert.IsTrue(collider.gameObject.layer != PhysicsLayers.onPointerEventLayer);
            }

            Assert.IsNull(onPointerDownComponent.pointerEventHandler.eventColliders.colliders);
        }

        [UnityTest]
        public IEnumerator OnHoverEventNotLeaveCollidersOnRecycledMeshes()
        {
            // 1. Instantiate entity and add an OnPointerHoverEnter component
            long entityId = 1;
            var entity1 = TestUtils.CreateSceneEntity(scene, entityId);

            string onPointerId = "pointerevent-1";
            var model = new OnPointerHoverEvent.Model()
            {
                type = OnPointerHoverEnter.NAME,
                uuid = onPointerId
            };
            var onHoverEnterComponent = TestUtils.EntityComponentCreate<OnPointerHoverEnter, OnPointerHoverEvent.Model>(
                scene, entity1,
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);

            // 2. Attach a shape
            var shapeModel = new LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>.Model();
            shapeModel.src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb";
            TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
                JsonConvert.SerializeObject(shapeModel));

            LoadWrapper gltfShapeLoader1 = Environment.i.world.state.GetLoaderForEntity(entity1);

            yield return new WaitUntil(() => gltfShapeLoader1.alreadyLoaded);
            yield return null;

            // 3. Save the mesh GO reference
            Transform shapeInstanceRootTransform = entity1.meshRootGameObject.transform.GetChild(0);

            // 4. Remove shape so that it returns to its pool
            scene.componentsManagerLegacy.RemoveSharedComponent(entity1, typeof(BaseShape));
            yield return null;

            // 5. Check that the pooled mesh doesn't have the collider children and the onPointerEvent component
            // doesn't have any instantiated collider (since its entity doesn't have a mesh now)
            var childMeshColliders = shapeInstanceRootTransform.GetComponentsInChildren<MeshCollider>(true);
            foreach (MeshCollider collider in childMeshColliders)
            {
                Assert.IsTrue(collider.gameObject.layer != PhysicsLayers.onPointerEventLayer);
            }

            Assert.IsNull(onHoverEnterComponent.pointerEventColliders.colliders);
        }

        [UnityTest]
        public IEnumerator UpdateParametShapeOnPointerDownCollidersMeshOnShapeUpdate()
        {
            // 1. Instantiate entity and add an OnPointerDown component
            long entityId = 1;
            var entity1 = TestUtils.CreateSceneEntity(scene, entityId);

            string onPointerId = "pointerevent-1";
            var onPointerEventModel = new OnPointerDown.Model()
            {
                type = OnPointerDown.NAME,
                uuid = onPointerId
            };
            var onPointerDownComponent = TestUtils.EntityComponentCreate<OnPointerDown, OnPointerDown.Model>(scene,
                entity1,
                onPointerEventModel, CLASS_ID_COMPONENT.UUID_CALLBACK);

            // 2. Attach a shape
            var planeShapeModel = new PlaneShape.Model { };
            var shapeId = TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.PLANE_SHAPE,
                JsonConvert.SerializeObject(planeShapeModel)
            );

            yield return null;

            // 3. Check onPointerEvent collider mesh is not null
            var pointerEventColliders = onPointerDownComponent.pointerEventHandler.eventColliders.colliders;
            Assert.IsTrue(pointerEventColliders != null || pointerEventColliders.Length > 0);
            foreach (MeshCollider pointerEventCollider in pointerEventColliders)
            {
                Assert.IsNotNull(pointerEventCollider.sharedMesh);
            }

            yield return null;

            // 4. Update shape uvs (to trigger entity's onShapeUpdate)
            planeShapeModel.uvs = new[]
            {
                0,
                0.75f,
                0.25f,
                0.75f,
                0,
                0.75f,
                0.25f,
                0.75f,
                0,
                0.75f,
                0.25f,
                0.75f,
                0,
                0.75f,
                0.25f,
                0.75f
            };
            TestUtils.UpdateShape(scene, shapeId, JsonConvert.SerializeObject(planeShapeModel));
            yield return null;

            // 5. Check that the onPointerEvent collider mesh is still not null
            Assert.IsTrue(pointerEventColliders != null || pointerEventColliders.Length > 0);
            foreach (MeshCollider pointerEventCollider in pointerEventColliders)
            {
                Assert.IsNotNull(pointerEventCollider.sharedMesh);
            }
        }

        [UnityTest]
        public IEnumerator UpdateParametShapeOnPointerHoverCollidersMeshOnShapeUpdate()
        {
            // 1. Instantiate entity and add an OnPointerDown component
            long entityId = 1;
            var entity1 = TestUtils.CreateSceneEntity(scene, entityId);

            string onPointerId = "pointerevent-1";
            var model = new OnPointerHoverEvent.Model()
            {
                type = OnPointerHoverEnter.NAME,
                uuid = onPointerId
            };
            var onHoverEnterComponent = TestUtils.EntityComponentCreate<OnPointerHoverEnter, OnPointerHoverEvent.Model>(
                scene, entity1,
                model, CLASS_ID_COMPONENT.UUID_CALLBACK);

            // 2. Attach a shape
            var planeShapeModel = new PlaneShape.Model { };
            var shapeId = TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.PLANE_SHAPE,
                JsonConvert.SerializeObject(planeShapeModel)
            );

            yield return null;

            // 3. Check onPointerEvent collider mesh is not null
            var pointerEventColliders = onHoverEnterComponent.pointerEventColliders.colliders;
            Assert.IsTrue(pointerEventColliders != null || pointerEventColliders.Length > 0);
            foreach (MeshCollider pointerEventCollider in pointerEventColliders)
            {
                Assert.IsNotNull(pointerEventCollider.sharedMesh);
            }

            yield return null;

            // 4. Update shape uvs (to trigger entity's onShapeUpdate)
            planeShapeModel.uvs = new[]
            {
                0,
                0.75f,
                0.25f,
                0.75f,
                0,
                0.75f,
                0.25f,
                0.75f,
                0,
                0.75f,
                0.25f,
                0.75f,
                0,
                0.75f,
                0.25f,
                0.75f
            };
            TestUtils.UpdateShape(scene, shapeId, JsonConvert.SerializeObject(planeShapeModel));
            yield return null;

            // 5. Check that the onPointerEvent collider mesh is still not null
            Assert.IsTrue(pointerEventColliders != null || pointerEventColliders.Length > 0);
            foreach (MeshCollider pointerEventCollider in pointerEventColliders)
            {
                Assert.IsNotNull(pointerEventCollider.sharedMesh);
            }
        }
    }
}
