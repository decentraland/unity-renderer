using System.Collections;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Tests.BuildModeHUDControllers
{
    public class SaveHUDControllerShould
    {
        private SaveHUDController saveHudController;

        [SetUp]
        public void SetUp()
        {
            saveHudController = new SaveHUDController();
            saveHudController.Initialize(Substitute.For<ISaveHUDView>());
        }

        [TearDown]
        public void TearDown() { saveHudController.Dispose(); }

        [Test]
        public void SaveStartCorrectly()
        {
            // Act
            saveHudController.SceneStateSave();

            // Assert
            saveHudController.view.Received(1).SceneStateSaved();
        }

        [Test]
        public void StopSaveAnimation()
        {
            // Act
            saveHudController.StopAnimation();

            // Assert
            saveHudController.view.Received(1).StopAnimation();
        }
    }
}