using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Tests.BuildModeHUDViews
{
    public class InspectorViewShould
    {
        private InspectorView inspectorView;

        [SetUp]
        public void SetUp() { inspectorView = InspectorView.Create(); }

        [TearDown]
        public void TearDown() { Object.Destroy(inspectorView.gameObject); }

        [Test]
        public void InvokeEntityActionCorrectly()
        {
            // Arrange
            BIWEntity testEntity = new BIWEntity();
            testEntity.entityUniqueId = "testId";
            EntityListAdapter testEntityListAdapter = new GameObject("_EntityListAdapter").AddComponent<EntityListAdapter>();
            testEntity.entityUniqueId = "testId";
            EntityAction testEntityAction = EntityAction.SHOW;
            BIWEntity returnedEntity = null;
            EntityListAdapter returnedEntityListAdapter = null;
            EntityAction retournedEntityAction = EntityAction.DELETE;
            inspectorView.OnEntityActionInvoked += (action, entityToApply, adapter) =>
            {
                retournedEntityAction = action;
                returnedEntity = entityToApply;
                returnedEntityListAdapter = adapter;
            };

            // Act
            inspectorView.EntityActionInvoked(testEntityAction, testEntity, testEntityListAdapter);

            // Assert
            Assert.AreEqual(testEntity.entityUniqueId, returnedEntity.entityUniqueId, "The entity does not match!!");
            Assert.AreEqual(testEntityListAdapter, returnedEntityListAdapter, "The entity list adapter does not match!!");
            Assert.AreEqual(testEntityAction, retournedEntityAction, "The entity action does not match!!");
        }

        [Test]
        public void RenameEntityCorrectly()
        {
            // Arrange
            BIWEntity testEntity = new BIWEntity();
            testEntity.entityUniqueId = "testId";
            string testText = "Test text";
            BIWEntity retournedEntity = null;
            string retournedText = "";
            inspectorView.OnEntityRename += (entity, newName) =>
            {
                retournedEntity = entity;
                retournedText = newName;
            };

            // Act
            inspectorView.EntityRename(testEntity, testText);

            // Assert
            Assert.AreEqual(testEntity.entityUniqueId, retournedEntity.entityUniqueId, "The entity does not match!!");
            Assert.AreEqual(testText, retournedText, "The text does not match!!");
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetActiveCorrectly(bool isActive)
        {
            // Arrange
            inspectorView.gameObject.SetActive(!isActive);

            // Act
            inspectorView.SetActive(isActive);

            // Assert
            Assert.AreEqual(isActive, inspectorView.gameObject.activeSelf, "The game object activation property does not match!");
        }

        [Test]
        public void SetEntitiesListCorrectly()
        {
            // Arrange
            List<BIWEntity> testList = new List<BIWEntity>();
            BIWEntity testEntity1 = new BIWEntity();
            testEntity1.entityUniqueId = "testId1";
            BIWEntity testEntity2 = new BIWEntity();
            testEntity1.entityUniqueId = "testId2";
            BIWEntity testEntity3 = new BIWEntity();
            testEntity1.entityUniqueId = "testId3";
            testList.Add(testEntity1);
            testList.Add(testEntity2);
            testList.Add(testEntity3);
            inspectorView.entitiesList = new List<BIWEntity>();

            // Act
            inspectorView.SetEntitiesList(testList);

            // Assert
            Assert.AreEqual(3, inspectorView.entitiesList.Count, "The number of set entities does not match!");
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(testList[i].entityUniqueId, inspectorView.entitiesList[i].entityUniqueId, "The added entity id does not match!");
            }
        }

        [Test]
        public void ClearEntitiesListCorrectly()
        {
            // Arrange
            List<BIWEntity> testList = new List<BIWEntity>();
            BIWEntity testEntity1 = new BIWEntity();
            testEntity1.entityUniqueId = "testId1";
            BIWEntity testEntity2 = new BIWEntity();
            testEntity1.entityUniqueId = "testId2";
            BIWEntity testEntity3 = new BIWEntity();
            testEntity1.entityUniqueId = "testId3";
            testList.Add(testEntity1);
            testList.Add(testEntity2);
            testList.Add(testEntity3);
            inspectorView.entitiesList = testList;

            // Act
            inspectorView.ClearEntitiesList();

            // Assert
            Assert.AreEqual(0, inspectorView.entitiesList.Count, "The number of set entities does not match!");
        }

        [Test]
        public void SetCloseButtonsActionCorrectly()
        {
            // Arrange
            bool actionsCalled = false;

            // Act
            inspectorView.SetCloseButtonsAction(() => { actionsCalled = true; });
            inspectorView.closeEntityListBtn.onClick.Invoke();

            // Assert
            Assert.IsTrue(actionsCalled, "");
        }

        [Test]
        public void ConfigureSceneLimitsCorrectly()
        {
            // Arrange
            ISceneLimitsController mockedSceneLimitsController = Substitute.For<ISceneLimitsController>();
            inspectorView.sceneLimitsController = null;


            // Act
            inspectorView.ConfigureSceneLimits(mockedSceneLimitsController);

            // Assert
            Assert.AreEqual(mockedSceneLimitsController, inspectorView.sceneLimitsController, "The scene limits controller does not match!");
            mockedSceneLimitsController.Received(1).Initialize(inspectorView.sceneLimitsView);
        }
    }
}