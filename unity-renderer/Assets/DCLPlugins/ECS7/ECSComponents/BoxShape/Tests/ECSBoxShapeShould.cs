using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.ECSComponents.Test
{
    public class ECSBoxShapeShould
    {
        private IDCLEntity entity;
        private IParcelScene scene;
        private ECSBoxShapeComponentHandler boxShapeComponentHandler;
        private GameObject gameObject;

        [SetUp]
        protected void SetUp()
        {
            gameObject = new GameObject();
            entity = Substitute.For<IDCLEntity>();
            scene = Substitute.For<IParcelScene>();
            boxShapeComponentHandler = new ECSBoxShapeComponentHandler();

            entity.entityId.Returns(1);
            entity.gameObject.Returns(gameObject);
            LoadParcelScenesMessage.UnityParcelScene sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            sceneData.id = "1";
            scene.sceneData.Configure().Returns(sceneData);
        }

        [TearDown]
        protected void TearDown()
        {
            boxShapeComponentHandler.OnComponentRemoved(scene, entity);
            GameObject.Destroy(gameObject);
        }

        [Test]
        public void UpdateComponentCorrectly()
        {
            // Arrange
            ECSBoxShape model = new ECSBoxShape();

            // Act
            boxShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.IsNotNull(boxShapeComponentHandler.meshesInfo);
        }

        [Test]
        public void DisposeComponentCorrectly()
        {
            // Arrange
            ECSBoxShape model = new ECSBoxShape();
            boxShapeComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Act
            boxShapeComponentHandler.OnComponentRemoved(scene, entity);

            // Assert
            Assert.IsNull(boxShapeComponentHandler.meshesInfo);
        }
    }
}
