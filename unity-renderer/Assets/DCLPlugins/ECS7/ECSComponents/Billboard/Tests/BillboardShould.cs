using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DCL.Configuration;
using DCL.Controllers;
using DCL.CRDT;
using DCL.ECS7;
using DCL.ECSRuntime;
using DCL.Models;
using ECSSystems.BillboardSystem;
using Google.Protobuf;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.ECSComponents.Test
{
    public class BillboardShould
    {
        private IDCLEntity testEntity;
        private GameObject testGameObject;
        private GameObject cameraGameObject;

        private IList<IParcelScene> scenes;
        private BillboardRegister billboardRegister;
        private ECSBillboardSystem systemUpdate;
        private DataStore_Camera dataStoreCamera;
        private ECSComponent<PBBillboard> billboards;

        [SetUp]
        protected void SetUp()
        {
            scenes = DataStore.i.ecs7.scenes;
            scenes.Add(Substitute.For<IParcelScene>());
            scenes[0]
                .sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene()
                {
                    id = "temptation", basePosition = new Vector2Int(1, 0), sceneNumber = 1
                });

            cameraGameObject = new GameObject("GO");
            cameraGameObject.transform.position = new UnityEngine.Vector3(ParcelSettings.PARCEL_SIZE, 0, 0);

            CommonScriptableObjects.rendererState.Set(true);
            CommonScriptableObjects.worldOffset.Set(UnityEngine.Vector3.zero);

            testGameObject = new GameObject();
            testEntity = Substitute.For<IDCLEntity>();

            testEntity.entityId.Returns(1);
            testEntity.gameObject.Returns(testGameObject);

            ECSComponentsFactory componentFactory = new ECSComponentsFactory();
            ECSComponentsManager componentsManager = new ECSComponentsManager(componentFactory.componentBuilders);
            var internalComponents = new InternalECSComponents(componentsManager, componentFactory, new Dictionary<int, ICRDTExecutor>());
            var componentsComposer = new ECS7ComponentsComposer(componentFactory,
                Substitute.For<IECSComponentWriter>(), internalComponents);

            billboards = (ECSComponent<PBBillboard>)componentsManager.GetOrCreateComponent(ComponentID.BILLBOARD);

            dataStoreCamera = new DataStore_Camera();
            dataStoreCamera.transform.Set(cameraGameObject.transform);

            systemUpdate = new ECSBillboardSystem(
                billboards,
                dataStoreCamera);
        }

        [TearDown]
        protected void TearDown()
        {
            GameObject.Destroy(testGameObject);
            GameObject.Destroy(cameraGameObject);
            CommonScriptableObjects.UnloadAll();
        }

        [Test]
        public void UpdateComponentCorrectly()
        {
            billboards.Create(scenes[0], testEntity);
            billboards.SetModel(scenes[0], testEntity, CreateModel());

            var currentRotation = testGameObject.transform.rotation;
            cameraGameObject.transform.position = new UnityEngine.Vector3(30, 2, 15);
            systemUpdate.Update();

            // Assert
            Assert.AreNotEqual(currentRotation,  testGameObject.transform.rotation);
        }

        [Test]
        public void SerializeAndDeserialzeCorrectly()
        {
            // Arrange
            var model = CreateModel();

            // Act
            var newModel = SerializaAndDeserialize(model);

            // Assert
            Assert.AreEqual(model.BillboardMode, newModel.BillboardMode);
            // Assert.AreEqual(model.OppositeDirection, newModel.OppositeDirection);
        }

        private PBBillboard SerializaAndDeserialize(PBBillboard pb)
        {
            var serialized = BillboardSerializer.Serialize(pb);
            return BillboardSerializer.Deserialize(serialized);
        }

        private PBBillboard CreateModel()
        {
            PBBillboard model = new PBBillboard();
            model.BillboardMode = BillboardMode.BmY;
            // model.OppositeDirection = true;
            return model;
        }
    }
}
