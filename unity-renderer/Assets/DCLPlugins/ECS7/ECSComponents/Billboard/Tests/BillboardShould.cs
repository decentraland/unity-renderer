using DCL;
using DCL.Controllers;
using DCL.ECS7;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using ECSSystems.BillboardSystem;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Tests
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
            scenes = new List<IParcelScene>();
            scenes.Add(Substitute.For<IParcelScene>());

            scenes[0]
               .sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene()
                {
                    id = "temptation", basePosition = new Vector2Int(1, 0), sceneNumber = 1
                });

            cameraGameObject = new GameObject("GO");
            cameraGameObject.transform.position = new Vector3(16, 0, 0);

            testGameObject = new GameObject();
            testEntity = Substitute.For<IDCLEntity>();

            testEntity.entityId.Returns(1);
            testEntity.gameObject.Returns(testGameObject);

            ECSComponentsFactory componentFactory = new ECSComponentsFactory();
            ECSComponentsManager componentsManager = new ECSComponentsManager(componentFactory.componentBuilders);

            var registerBillboard = new BillboardRegister(
                ComponentID.BILLBOARD,
                componentFactory,
                Substitute.For<IECSComponentWriter>());

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
            Object.Destroy(testGameObject);
            Object.Destroy(cameraGameObject);
        }

        [Test]
        public void UpdateComponentCorrectly()
        {
            billboards.Create(scenes[0], testEntity);
            billboards.SetModel(scenes[0], testEntity, CreateModel());

            var currentRotation = testGameObject.transform.rotation;
            cameraGameObject.transform.rotation = Quaternion.Euler(0, 3, 0);
            systemUpdate.Update();

            // Assert
            Assert.AreNotEqual(currentRotation, testGameObject.transform.rotation);
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
