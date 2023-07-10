using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Configuration;
using DCL.CRDT;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class MeshColliderHandlerShould
    {
        private struct KeepEntityAliveModel : IInternalComponent { public bool dirty { get; set; } }

        private MeshColliderHandler handler;
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private ECS7TestEntity entity;
        private InternalECSComponents internalComponents;

        [SetUp]
        public void SetUp()
        {
            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();

            internalComponents = new InternalECSComponents(componentsManager, componentsFactory, executors);

            handler = new MeshColliderHandler(
                internalComponents.onPointerColliderComponent,
                internalComponents.physicColliderComponent,
                internalComponents.customLayerColliderComponent);

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);
            scene = testUtils.CreateScene(666);
            entity = scene.CreateEntity(1101);

            var keepEntityAliveComponent = new InternalECSComponent<KeepEntityAliveModel>(
                0, componentsManager, componentsFactory, null,
                executors, Substitute.For<IComponentDirtySystem>());
            keepEntityAliveComponent.PutFor(scene, entity, new KeepEntityAliveModel());
        }

        [TearDown]
        public void TearDown()
        {
            testUtils.Dispose();
            AssetPromiseKeeper_PrimitiveMesh.i.Cleanup();
        }

        [Test]
        public void BeCreatedCorrectly()
        {
            Assert.IsNull(handler.colliderGameObject);
            handler.OnComponentCreated(scene, entity);

            Assert.NotNull(handler.colliderGameObject);
            Assert.IsNull(handler.colliderGameObject.GetComponent<Collider>());
        }

        [UnityTest]
        public IEnumerator CreateBoxCorrectly()
        {
            handler.OnComponentCreated(scene, entity);
            handler.OnComponentModelUpdated(scene, entity, new PBMeshCollider() { Box = new PBMeshCollider.Types.BoxMesh() });

            BoxCollider boxCollider = handler.colliderGameObject.GetComponent<BoxCollider>();
            Assert.IsNotNull(boxCollider);
            Assert.AreEqual(UnityEngine.Vector3.one, boxCollider.size);
            Assert.AreEqual(UnityEngine.Vector3.zero, boxCollider.center);

            handler.OnComponentRemoved(scene, entity);
            yield return null;
            Assert.IsFalse(handler.colliderGameObject);
        }

        [UnityTest]
        public IEnumerator CreatePlaneCorrectly()
        {
            handler.OnComponentCreated(scene, entity);
            handler.OnComponentModelUpdated(scene, entity, new PBMeshCollider() { Plane = new PBMeshCollider.Types.PlaneMesh() });

            BoxCollider boxCollider = handler.colliderGameObject.GetComponent<BoxCollider>();
            Assert.IsNotNull(boxCollider);
            Assert.AreEqual(new UnityEngine.Vector3(1, 1, 0.01f), boxCollider.size);
            Assert.AreEqual(UnityEngine.Vector3.zero, boxCollider.center);

            handler.OnComponentRemoved(scene, entity);
            yield return null;
            Assert.IsFalse(handler.colliderGameObject);
        }

        [UnityTest]
        public IEnumerator CreateSphereCorrectly()
        {
            handler.OnComponentCreated(scene, entity);
            handler.OnComponentModelUpdated(scene, entity, new PBMeshCollider() { Sphere = new PBMeshCollider.Types.SphereMesh() });

            MeshCollider meshCollider = handler.colliderGameObject.GetComponent<MeshCollider>();
            Assert.IsNotNull(meshCollider);
            Assert.IsNotNull(meshCollider.sharedMesh);

            handler.OnComponentRemoved(scene, entity);
            yield return null;
            Assert.IsFalse(handler.colliderGameObject);
        }

        [UnityTest]
        public IEnumerator CreateCylinderCorrectly()
        {
            handler.OnComponentCreated(scene, entity);
            handler.OnComponentModelUpdated(scene, entity, new PBMeshCollider() { Cylinder = new PBMeshCollider.Types.CylinderMesh() });
            yield return null;

            MeshCollider meshCollider = handler.colliderGameObject.GetComponent<MeshCollider>();
            Assert.IsNotNull(meshCollider);
            Assert.IsNotNull(meshCollider.sharedMesh);

            handler.OnComponentRemoved(scene, entity);
            yield return null;
            Assert.IsFalse(handler.colliderGameObject);
        }

        [UnityTest]
        public IEnumerator UpdateCylinderCorrectly()
        {
            handler.OnComponentCreated(scene, entity);
            handler.OnComponentModelUpdated(scene, entity, new PBMeshCollider() { Cylinder = new PBMeshCollider.Types.CylinderMesh() });
            yield return null;

            MeshCollider meshCollider01 = handler.colliderGameObject.GetComponent<MeshCollider>();
            Assert.IsNotNull(meshCollider01);
            Assert.IsNotNull(meshCollider01.sharedMesh);
            Mesh mesh01 = meshCollider01.sharedMesh;

            handler.OnComponentModelUpdated(scene, entity, new PBMeshCollider() { Cylinder = new PBMeshCollider.Types.CylinderMesh() { RadiusBottom = 5 } });
            yield return null;

            MeshCollider meshCollider02 = handler.colliderGameObject.GetComponent<MeshCollider>();
            Assert.IsNotNull(meshCollider02);
            Assert.IsNotNull(meshCollider02.sharedMesh);
            Mesh mesh02 = meshCollider02.sharedMesh;

            Assert.AreNotEqual(mesh01, mesh02);

            handler.OnComponentRemoved(scene, entity);
            yield return null;
            Assert.IsFalse(handler.colliderGameObject);
            Assert.IsFalse(mesh01);
            Assert.IsFalse(mesh02);
        }

        [UnityTest]
        public IEnumerator UpdateColliderLayerCorrectly()
        {
            handler.OnComponentCreated(scene, entity);
            handler.OnComponentModelUpdated(scene, entity, new PBMeshCollider() { Box = new PBMeshCollider.Types.BoxMesh() });

            BoxCollider boxCollider = handler.colliderGameObject.GetComponent<BoxCollider>();

            // default layer
            Assert.AreEqual(PhysicsLayers.defaultLayer, handler.colliderGameObject.layer);

            handler.OnComponentModelUpdated(scene, entity, new PBMeshCollider()
            {
                Box = new PBMeshCollider.Types.BoxMesh(),
                CollisionMask = (int)ColliderLayer.ClPhysics
            });

            // physics layer
            Assert.AreEqual(PhysicsLayers.characterOnlyLayer, handler.colliderGameObject.layer);
            Assert.AreEqual(boxCollider, handler.colliderGameObject.GetComponent<BoxCollider>());

            handler.OnComponentModelUpdated(scene, entity, new PBMeshCollider()
            {
                Box = new PBMeshCollider.Types.BoxMesh(),
                CollisionMask = (int)ColliderLayer.ClPointer
            });

            // pointer layer
            Assert.AreEqual(PhysicsLayers.onPointerEventLayer, handler.colliderGameObject.layer);
            Assert.AreEqual(boxCollider, handler.colliderGameObject.GetComponent<BoxCollider>());

            handler.OnComponentModelUpdated(scene, entity, new PBMeshCollider()
            {
                Box = new PBMeshCollider.Types.BoxMesh(),
                CollisionMask = (int)ColliderLayer.ClNone
            });

            // "none" layer
            Assert.IsFalse(handler.colliderGameObject.activeSelf);
            Assert.AreEqual(boxCollider, handler.colliderGameObject.GetComponent<BoxCollider>());

            handler.OnComponentModelUpdated(scene, entity, new PBMeshCollider()
            {
                Box = new PBMeshCollider.Types.BoxMesh(),
                CollisionMask = (int)ColliderLayer.ClPointer | (int)ColliderLayer.ClPhysics
            });

            // physics and pointer layer
            Assert.AreEqual(PhysicsLayers.defaultLayer, handler.colliderGameObject.layer);
            Assert.AreEqual(boxCollider, handler.colliderGameObject.GetComponent<BoxCollider>());
            Assert.IsTrue(handler.colliderGameObject.activeSelf);

            yield return null;
            handler.OnComponentRemoved(scene, entity);
        }

        [UnityTest]
        public IEnumerator CreateInternalCollidersCorrectly()
        {
            IInternalECSComponent<InternalColliders> physicColliders = internalComponents.physicColliderComponent;
            IInternalECSComponent<InternalColliders> pointerColliders = internalComponents.onPointerColliderComponent;

            handler.OnComponentCreated(scene, entity);

            // physics collider
            handler.OnComponentModelUpdated(scene, entity, new PBMeshCollider()
            {
                Plane = new PBMeshCollider.Types.PlaneMesh(),
                CollisionMask = (int)ColliderLayer.ClPhysics
            });

            BoxCollider boxCollider = handler.colliderGameObject.GetComponent<BoxCollider>();

            Assert.NotNull(physicColliders.GetFor(scene, entity));
            Assert.IsNull(pointerColliders.GetFor(scene, entity));
            Assert.AreEqual(boxCollider, physicColliders.GetFor(scene, entity).Value.model.colliders.Pairs[0].key);

            // pointer collider
            handler.OnComponentModelUpdated(scene, entity, new PBMeshCollider()
            {
                Plane = new PBMeshCollider.Types.PlaneMesh(),
                CollisionMask = (int)ColliderLayer.ClPointer
            });

            boxCollider = handler.colliderGameObject.GetComponent<BoxCollider>();

            Assert.IsNull(physicColliders.GetFor(scene, entity));
            Assert.NotNull(pointerColliders.GetFor(scene, entity));
            Assert.AreEqual(boxCollider, pointerColliders.GetFor(scene, entity).Value.model.colliders.Pairs[0].key);

            // physic and pointer collider
            handler.OnComponentModelUpdated(scene, entity, new PBMeshCollider()
            {
                Plane = new PBMeshCollider.Types.PlaneMesh(),
                CollisionMask = (int)ColliderLayer.ClPointer | (int)ColliderLayer.ClPhysics
            });

            boxCollider = handler.colliderGameObject.GetComponent<BoxCollider>();

            Assert.NotNull(physicColliders.GetFor(scene, entity));
            Assert.NotNull(pointerColliders.GetFor(scene, entity));
            Assert.AreEqual(boxCollider, physicColliders.GetFor(scene, entity).Value.model.colliders.Pairs[0].key);
            Assert.AreEqual(boxCollider, pointerColliders.GetFor(scene, entity).Value.model.colliders.Pairs[0].key);

            // remove component, internal colliders should be removed too
            handler.OnComponentRemoved(scene, entity);
            yield return null;

            Assert.IsFalse(handler.colliderGameObject);
            Assert.IsNull(physicColliders.GetFor(scene, entity));
            Assert.IsNull(pointerColliders.GetFor(scene, entity));
        }
    }
}
