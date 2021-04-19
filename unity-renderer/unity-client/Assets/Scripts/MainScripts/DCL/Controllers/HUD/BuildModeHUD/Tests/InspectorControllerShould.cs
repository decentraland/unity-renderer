using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Tests.BuildModeHUDControllers
{
    public class InspectorControllerShould
    {
        private InspectorController inspectorController;

        [SetUp]
        public void SetUp()
        {
            inspectorController = new InspectorController();
            inspectorController.Initialize(Substitute.For<IInspectorView>());
        }

        [TearDown]
        public void TearDown()
        {
            inspectorController.Dispose();
        }

        [Test]
        public void OpenEntityListCorrectly()
        {
            // Act
            inspectorController.OpenEntityList();

            // Assert
            inspectorController.inspectorView.Received(1).SetActive(true);
        }

        [Test]
        public void SetEntityListCorrectly()
        {
            // Arrange
            inspectorController.inspectorView.entityList = new GameObject("_EntityListView").AddComponent<EntityListView>();
            List<DCLBuilderInWorldEntity> testEntities = new List<DCLBuilderInWorldEntity>();

            // Act
            inspectorController.SetEntityList(testEntities);

            // Assert
            inspectorController.inspectorView.Received(1).SetEntitiesList(testEntities);
        }

        [Test]
        public void ClearListCorrectly()
        {
            // Act
            inspectorController.ClearList();

            // Assert
            inspectorController.inspectorView.Received(1).ClearEntitiesList();
        }

        [Test]
        public void CloseListCorrectly()
        {
            // Act
            inspectorController.CloseList();

            // Assert
            inspectorController.inspectorView.Received().SetActive(false);
        }

        [Test]
        [TestCase(EntityAction.SELECT)]
        [TestCase(EntityAction.LOCK)]
        [TestCase(EntityAction.DELETE)]
        [TestCase(EntityAction.SHOW)]
        public void InvokeEntityActionCorrectly(EntityAction actionToInvoke)
        {
            // Arrange
            EntityAction testAction = actionToInvoke;
            DCLBuilderInWorldEntity testEntity = new GameObject("_DCLBuilderInWorldEntity").AddComponent<DCLBuilderInWorldEntity>();
            DCLBuilderInWorldEntity returnedEntity = null;

            switch (actionToInvoke)
            {
                case EntityAction.SELECT:

                    inspectorController.OnEntityClick += (entity) => { returnedEntity = entity; };
                    break;
                case EntityAction.LOCK:

                    inspectorController.OnEntityLock += (entity) => { returnedEntity = entity; };
                    break;
                case EntityAction.DELETE:

                    inspectorController.OnEntityDelete += (entity) => { returnedEntity = entity; };
                    break;
                case EntityAction.SHOW:
                    inspectorController.OnEntityChangeVisibility += (entity) => { returnedEntity = entity; };
                    break;
            }

            // Act
            inspectorController.EntityActionInvoked(testAction, testEntity, null);

            // Assert
            Assert.AreEqual(testEntity, returnedEntity, "The entity does not match!");
        }

        [Test]
        public void EntityRenameCorrectly()
        {
            // Arrange
            DCLBuilderInWorldEntity testEntity = new GameObject("_DCLBuilderInWorldEntity").AddComponent<DCLBuilderInWorldEntity>();
            string testText = "Test text";
            DCLBuilderInWorldEntity returnedEntity = null;
            string returnedText = "";

            inspectorController.OnEntityRename += (entity, name) => 
            { 
                returnedEntity = entity;
                returnedText = name;
            };

            // Act
            inspectorController.EntityRename(testEntity, testText);

            // Assert
            Assert.AreEqual(testEntity, returnedEntity, "The entity does not match!");
            Assert.AreEqual(testText, returnedText, "The text does not match!");
        }

        [Test]
        public void SetCloseButtonsActionCorrectly()
        {
            // Arrange
            UnityAction testAction = () => { };

            // Act
            inspectorController.SetCloseButtonsAction(testAction);

            // Assert
            inspectorController.inspectorView.Received(1).SetCloseButtonsAction(testAction);
        }
    }
}
