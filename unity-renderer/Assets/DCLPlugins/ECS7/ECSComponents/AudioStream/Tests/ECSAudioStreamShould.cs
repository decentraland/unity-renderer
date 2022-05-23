using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Models;
using DCL.SettingsCommon;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;
using AudioSettings = DCL.SettingsCommon.AudioSettings;

namespace DCL.ECSComponents.Test
{
    public class ECSAudioStreamShould : IntegrationTestSuite
    {
        private IDCLEntity entity;
        private IParcelScene scene;
        private ECSAudioStreamComponentHandler audioSourceComponentHandler;
        private GameObject gameObject;

        protected override void InitializeServices(ServiceLocator serviceLocator)
        {
            base.InitializeServices(serviceLocator);
            serviceLocator.Register<IWebRequestController>( WebRequestController.Create );
        }

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            
            Settings.CreateSharedInstance(new DefaultSettingsFactory());
            gameObject = new GameObject();
            entity = Substitute.For<IDCLEntity>();
            scene = Substitute.For<IParcelScene>();
            audioSourceComponentHandler = new ECSAudioStreamComponentHandler();

            entity.entityId.Returns(1);
            entity.gameObject.Returns(gameObject);
            LoadParcelScenesMessage.UnityParcelScene sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            sceneData.id = "1";

            ContentProvider_Dummy providerDummy = new ContentProvider_Dummy();

            scene.sceneData.Configure().Returns(sceneData);
            scene.Configure().contentProvider.Returns(providerDummy);

            audioSourceComponentHandler.OnComponentCreated(scene, entity);
        }

        [UnityTearDown]
        protected override IEnumerator TearDown()
        {
            yield return base.TearDown();
            audioSourceComponentHandler.OnComponentRemoved(scene, entity);
            GameObject.Destroy(gameObject);
        }
        
        [Test]
        public void UpdatePlayingModelComponentCorrectly()
        {
            // Arrange
            
            // We prepare the componentHandler
            audioSourceComponentHandler.isInsideBoundaries = true;
            audioSourceComponentHandler.isInsideScene = true;
            audioSourceComponentHandler.isRendererActive = true;

            // We prepare the models
            ECSAudioStream model = CreateAudioStreamModel();
            model.playing = false;
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);
            
            ECSAudioStream model2 = CreateAudioStreamModel();
            model2.playing = true;
            
            // Act
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model2);

            // Assert
            Assert.AreEqual(audioSourceComponentHandler.isPlaying, true);
        }
        
        [Test]
        public void UpdateVolumeModelComponentCorrectly()
        {
            // Arrange
            ECSAudioStream model = CreateAudioStreamModel();
            model.volume = 0f;
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);
            
            ECSAudioStream model2 = CreateAudioStreamModel();
            model2.volume = 1f;
            
            // Act
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model2);

            // Assert
            Assert.AreEqual(1f,audioSourceComponentHandler.currentVolume);
        }
        
        [Test]
        public void UpdateUrlModelComponentCorrectly()
        {
            // Arrange
            string expectedUrl = "NewUrl";
            ECSAudioStream model = CreateAudioStreamModel();
            model.url = "OldUrl";
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);
            
            ECSAudioStream model2 = CreateAudioStreamModel();
            model2.url = expectedUrl;
            
            // Act
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model2);

            // Assert
            Assert.AreEqual(expectedUrl,audioSourceComponentHandler.model.url);
        }
        
        [Test]
        public void PlayAudioIfConditionsAreMeet()
        {
            // Arrange
            ECSAudioStream model = CreateAudioStreamModel();
            model.playing = true;
            audioSourceComponentHandler.isInsideBoundaries = true;
            audioSourceComponentHandler.isInsideScene = true;
            audioSourceComponentHandler.isRendererActive = true;
            
            // Act
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.IsTrue(audioSourceComponentHandler.isPlaying);
        }
        
        [Test]
        public void StopAudioIfRendererIsDisable()
        {
            // Arrange
            ECSAudioStream model = CreateAudioStreamModel();
            model.playing = true;
            audioSourceComponentHandler.isInsideBoundaries = true;
            audioSourceComponentHandler.isInsideScene = true;
            audioSourceComponentHandler.isRendererActive = false;
            
            // Act
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.IsFalse(audioSourceComponentHandler.isPlaying);
        }
        
        [Test]
        public void StopAudioIfItsOutsideScene()
        {
            // Arrange
            ECSAudioStream model = CreateAudioStreamModel();
            model.playing = true;
            audioSourceComponentHandler.isInsideBoundaries = true;
            audioSourceComponentHandler.isInsideScene = false;
            audioSourceComponentHandler.isRendererActive = true;
            
            // Act
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.IsFalse(audioSourceComponentHandler.isPlaying);
        }
        
        [Test]
        public void StopAudioIfItsOutsideBoundaries()
        {
            // Arrange
            ECSAudioStream model = CreateAudioStreamModel();
            model.playing = true;
            audioSourceComponentHandler.isInsideBoundaries = false;
            audioSourceComponentHandler.isInsideScene = true;
            audioSourceComponentHandler.isRendererActive = true;
            
            // Act
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.IsFalse(audioSourceComponentHandler.isPlaying);
        }
        
        [UnityTest]
        public IEnumerator DisposeComponentCorrectly()
        {
            // Arrange
            ECSAudioStream model = CreateAudioStreamModel();
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);
        
            // Act
            audioSourceComponentHandler.OnComponentRemoved(scene, entity);
            yield return null;
        
            // Assert
            Assert.IsNull(audioSourceComponentHandler.audioSource);
        }

        private ECSAudioStream CreateAudioStreamModel()
        {
            ECSAudioStream model = new ECSAudioStream()
            {
                url = "https://audio.dcl.guru/radio/8110/radio.mp3",
                playing = true,
                volume = 1f
            };
            return model;
        }
        
    }
}