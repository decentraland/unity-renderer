using System.Collections;
using DCL;
using DCL.Configuration;
using DCL.ECSComponents;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class MeshColliderHandlerShould
    {
        private MeshColliderHandler handler;
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private ECS7TestEntity entity;

        [SetUp]
        public void SetUp()
        {
            handler = new MeshColliderHandler();
            testUtils = new ECS7TestUtilsScenesAndEntities();
            scene = testUtils.CreateScene("temptation");
            entity = scene.CreateEntity(1101);
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

            SphereCollider sphereCollider = handler.colliderGameObject.GetComponent<SphereCollider>();
            Assert.IsNotNull(sphereCollider);
            Assert.AreEqual(1, sphereCollider.radius);
            Assert.AreEqual(UnityEngine.Vector3.zero, sphereCollider.center);

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
                CollisionMask = (int)ColliderLayer.Physics
            });

            // physics layer
            Assert.AreEqual(PhysicsLayers.characterOnlyLayer, handler.colliderGameObject.layer);
            Assert.AreEqual(boxCollider, handler.colliderGameObject.GetComponent<BoxCollider>());

            handler.OnComponentModelUpdated(scene, entity, new PBMeshCollider()
            {
                Box = new PBMeshCollider.Types.BoxMesh(),
                CollisionMask = (int)ColliderLayer.Pointer
            });

            // pointer layer
            Assert.AreEqual(PhysicsLayers.onPointerEventLayer, handler.colliderGameObject.layer);
            Assert.AreEqual(boxCollider, handler.colliderGameObject.GetComponent<BoxCollider>());

            handler.OnComponentModelUpdated(scene, entity, new PBMeshCollider()
            {
                Box = new PBMeshCollider.Types.BoxMesh(),
                CollisionMask = (int)ColliderLayer.None
            });

            // "none" layer
            Assert.IsFalse(handler.colliderGameObject.activeSelf);
            Assert.AreEqual(boxCollider, handler.colliderGameObject.GetComponent<BoxCollider>());

            handler.OnComponentModelUpdated(scene, entity, new PBMeshCollider()
            {
                Box = new PBMeshCollider.Types.BoxMesh(),
                CollisionMask = (int)ColliderLayer.Pointer | (int)ColliderLayer.Physics
            });

            // physics and pointer layer
            Assert.AreEqual(PhysicsLayers.defaultLayer, handler.colliderGameObject.layer);
            Assert.AreEqual(boxCollider, handler.colliderGameObject.GetComponent<BoxCollider>());
            Assert.IsTrue(handler.colliderGameObject.activeSelf);

            yield return null;
            handler.OnComponentRemoved(scene, entity);
        }
    }
}