﻿using DCL;
using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Models;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using Tests;
using UnityEngine;

namespace MainScripts.DCL.WorldRuntime.Tests
{
    public class SceneLifecycleHandlerShoul : IntegrationTestSuite
    {
        private SceneLifecycleHandler sceneLifecycleHandler;
        private ParcelScene parcelScene;
        private IDCLEntity entity;

        [SetUp]
        public void SetUp()
        {
            // Configure Scene
            parcelScene = TestUtils.CreateTestScene();

            // Configure entity
            entity = Substitute.For<IDCLEntity>();
            entity.Configure().entityId.Returns(5555);

            sceneLifecycleHandler = new SceneLifecycleHandler(parcelScene);
        }

        [TearDown]
        public void TearDown() { GameObject.Destroy(parcelScene); }

        [Test]
        public void ChangeToTheNewTrackerIfECS7IsPresent()
        {
            // Arrange
            var initialTracker = sceneLifecycleHandler.sceneResourcesLoadTracker.tracker;

            // Act
            DataStore.i.ecs7.pendingSceneResources.IncreaseRefCount((parcelScene.sceneData.id, new Object()));

            // Assert
            Assert.AreNotEqual(sceneLifecycleHandler.sceneResourcesLoadTracker.tracker, initialTracker);
        }
    }
}