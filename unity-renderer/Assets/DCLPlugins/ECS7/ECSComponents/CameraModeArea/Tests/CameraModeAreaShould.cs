using DCL.Components;
using DCL.Controllers;
using DCL.Models;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace DCL.ECSComponents.Test
{
    public class CameraModeAreaShould
    {
        private IDCLEntity entity;
        private IParcelScene scene;
        private CameraModeAreaComponentHandler componentHandler;
        private GameObject gameObject;
        private DataStore_Player dataStorePlayer;
        private ICameraModeAreasController areasController;

        [SetUp]
        protected void SetUp()
        {
            gameObject = new GameObject();
            entity = Substitute.For<IDCLEntity>();
            scene = Substitute.For<IParcelScene>();
            dataStorePlayer = new DataStore_Player();
            componentHandler = new CameraModeAreaComponentHandler(Substitute.For<IUpdateEventHandler>(), dataStorePlayer);

            entity.entityId.Returns(1);
            entity.gameObject.Returns(gameObject);
            LoadParcelScenesMessage.UnityParcelScene sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            sceneData.id = "1";
            scene.sceneData.Configure().Returns(sceneData);

            componentHandler.OnComponentCreated(scene, entity);
            areasController = Substitute.For<ICameraModeAreasController>();
            componentHandler.areasController = areasController;
        }

        [TearDown]
        protected void TearDown()
        {
            componentHandler.OnComponentRemoved(scene, entity);
            GameObject.Destroy(gameObject);
        }

        [Test]
        public void UpdateComponentCorrectly()
        {
            // Arrange
            var model = CreateModel();
            componentHandler.isPlayerInside = true;

            // Act
            componentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            areasController.Received(1).ChangeAreaMode(componentHandler.cameraModeRepresentantion);
        }
        
        [Test]
        public void NotUpdateComponentIfSameMode()
        {
            // Arrange
            var model = CreateModel();  
            componentHandler.lastModel = CreateModel();
            componentHandler.lastModel.Mode = model.Mode;

            // Act
            componentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            areasController.DidNotReceive().ChangeAreaMode(componentHandler.cameraModeRepresentantion);
        }

        [Test]
        public void DisposeComponentCorrectly()
        {
            // Arrange
            var model = CreateModel();
            componentHandler.OnComponentModelUpdated(scene, entity, model);
            componentHandler.isPlayerInside = true;

            // Act
            componentHandler.OnComponentRemoved(scene, entity);

            // Assert
            areasController.Received(1).RemoveInsideArea(componentHandler.cameraModeRepresentantion);
            Assert.IsFalse(componentHandler.isPlayerInside);
        }

        [Test]
        public void RemoveInsideAreaCorrectly()
        {
            // Arrange
            var model = CreateModel();
            componentHandler.isPlayerInside = true;
            componentHandler.OnComponentModelUpdated(scene, entity, model);
            
            // Act
            componentHandler.Update();

            // Assert
            areasController.Received(1).RemoveInsideArea(componentHandler.cameraModeRepresentantion);
        }

        [Test]
        public void SerializeAndDeserialzeCorrectly()
        {
            // Arrange
            var model = CreateModel();

            // Act
            var newModel = SerializaAndDeserialize(model);

            // Assert
            Assert.AreEqual(model.Area, newModel.Area);
            Assert.AreEqual(model.Mode, newModel.Mode);
        }

        private PBCameraModeArea SerializaAndDeserialize(PBCameraModeArea pb)
        {
            var serialized = CameraModeAreaSerializer.Serialize(pb);
            return CameraModeAreaSerializer.Deserialize(serialized);
        }

        private PBCameraModeArea CreateModel()
        {
            PBCameraModeArea model = new PBCameraModeArea();
            model.Area = new Vector3();
            model.Area.X = 2f;
            model.Area.Y = 2f;
            model.Area.Z = 2f;

            model.Mode = CameraMode.FirstPerson;

            return model;
        }
    }
}