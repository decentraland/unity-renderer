using System.Collections;
using DCL;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class MeshRendererHandlerShould
    {
        private ECS7TestScene scene;
        private ECS7TestEntity entity;
        private ECS7TestUtilsScenesAndEntities testUtils;
        private MeshRendererHandler handler;
        private IInternalECSComponent<InternalTexturizable> texturizableComponent;

        [SetUp]
        public void SetUp()
        {
            testUtils = new ECS7TestUtilsScenesAndEntities();
            scene = testUtils.CreateScene("temptation");
            entity = scene.CreateEntity(100);
            texturizableComponent = Substitute.For<IInternalECSComponent<InternalTexturizable>>();
            handler = new MeshRendererHandler(new DataStore_ECS7(), texturizableComponent);
        }

        [TearDown]
        public void TearDown()
        {
            testUtils.Dispose();
        }

        [UnityTest]
        public IEnumerator CreatePrimitiveMesh()
        {
            yield return CreatePrimitiveMesh(new PBMeshRenderer() { Box = new BoxMesh() });
            yield return CreatePrimitiveMesh(new PBMeshRenderer() { Plane = new PlaneMesh() });
            yield return CreatePrimitiveMesh(new PBMeshRenderer() { Cylinder = new CylinderMesh() });
            yield return CreatePrimitiveMesh(new PBMeshRenderer() { Sphere = new SphereMesh() });
        }

        [UnityTest]
        public IEnumerator HandleMissingData()
        {
            PBMeshRenderer model = new PBMeshRenderer(); // no shape configured

            handler.OnComponentCreated(scene, entity);
            handler.OnComponentModelUpdated(scene, entity, model);
            yield return null;

            Assert.IsNull(entity.gameObject.GetComponentInChildren<MeshFilter>().sharedMesh, $"model = {model}");

            handler.OnComponentRemoved(scene, entity);
            yield return null;

            Assert.AreEqual(0, entity.gameObject.transform.childCount, $"model = {model}");
        }

        [UnityTest]
        public IEnumerator PutTexturizableComponent()
        {
            PBMeshRenderer model = new PBMeshRenderer() { Box = new BoxMesh() };

            handler.OnComponentCreated(scene, entity);
            handler.OnComponentModelUpdated(scene, entity, model);
            yield return null;

            Renderer renderer = entity.gameObject.GetComponentInChildren<Renderer>();
            texturizableComponent.Received(1)
                                 .PutFor(scene, entity,
                                     Arg.Is<InternalTexturizable>(x => x.renderers.Contains(renderer)));

            texturizableComponent.ClearReceivedCalls();

            handler.OnComponentRemoved(scene, entity);
            yield return null;

            texturizableComponent.Received(1)
                                 .PutFor(scene, entity,
                                     Arg.Is<InternalTexturizable>(x => x.renderers.Count == 0));
        }

        [UnityTest]
        public IEnumerator ShareMeshes()
        {
            PBMeshRenderer model = new PBMeshRenderer() { Box = new BoxMesh() };

            handler.OnComponentCreated(scene, entity);
            handler.OnComponentModelUpdated(scene, entity, model);
            yield return null;

            Mesh firstEntityMesh = entity.gameObject.GetComponentInChildren<MeshFilter>().sharedMesh;

            ECS7TestEntity secondEntity = scene.CreateEntity(1002);
            handler.OnComponentCreated(scene, secondEntity);
            handler.OnComponentModelUpdated(scene, secondEntity, model);
            yield return null;

            Mesh secondEntityMesh = secondEntity.gameObject.GetComponentInChildren<MeshFilter>().sharedMesh;

            Assert.AreEqual(firstEntityMesh, secondEntityMesh);

            handler.OnComponentRemoved(scene, entity);
            yield return null;

            Assert.IsNotNull(firstEntityMesh);
            Assert.IsNotNull(secondEntityMesh);

            handler.OnComponentRemoved(scene, secondEntity);
            yield return null;

            Assert.IsTrue(firstEntityMesh == null); //Assert.IsNull will fail as it compare with a different type of null
        }

        private IEnumerator CreatePrimitiveMesh(PBMeshRenderer model)
        {
            handler.OnComponentCreated(scene, entity);
            handler.OnComponentModelUpdated(scene, entity, model);
            yield return null;

            Assert.IsNotNull(entity.gameObject.GetComponentInChildren<MeshFilter>().sharedMesh, $"model = {model}");

            handler.OnComponentRemoved(scene, entity);
            yield return null;

            Assert.AreEqual(0, entity.gameObject.transform.childCount, $"model = {model}");
        }
    }
}