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
    public class ECSAudioSourceShould : IntegrationTestSuite
    {
        private IDCLEntity entity;
        private IParcelScene scene;
        private ECSAudioSourceComponentHandler audioSourceComponentHandler;
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
            gameObject = new GameObject();
            entity = Substitute.For<IDCLEntity>();
            scene = Substitute.For<IParcelScene>();
            audioSourceComponentHandler = new ECSAudioSourceComponentHandler(DataStore.i,Settings.i, AssetPromiseKeeper_AudioClip.i, CommonScriptableObjects.sceneID);

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
        
        [UnityTest]
        public IEnumerator UpdatePlayingModelComponentCorrectly()
        {
            // Arrange
            ECSAudioSource model = CreateAudioSourceModel();
            model.playing = true;
            model.loop = false;

            // Act
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);

            yield return new WaitUntil( () => audioSourceComponentHandler.promiseAudioClip.state == AssetPromiseState.FINISHED);

            // Assert
            Assert.AreEqual(audioSourceComponentHandler.audioSource.isPlaying, true);
        }

        [Test]
        public void UpdateLoopModelComponentCorrectly()
        {
            // Arrange
            ECSAudioSource model = CreateAudioSourceModel();
            model.loop = false;
            
            ECSAudioSource model2 = CreateAudioSourceModel();
            model2.loop = true;
            
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);
            
            // Act
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model2);

            // Assert
            Assert.AreEqual(audioSourceComponentHandler.audioSource.loop, model2.loop);
        }

        [Test]
        public void UpdatePitchModelComponentCorrectly()
        {
            // Arrange
            ECSAudioSource model = CreateAudioSourceModel();
            model.pitch = 0f;
            
            ECSAudioSource model2 = CreateAudioSourceModel();
            model2.pitch = 1f;
            
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);
            
            // Act
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model2);

            // Assert
            Assert.AreEqual(audioSourceComponentHandler.audioSource.pitch, model2.pitch);
        }
        
        [Test]
        public void UpdateVolumeModelComponentCorrectly()
        {
            // Arrange
            Settings.CreateSharedInstance(new DefaultSettingsFactory());
            CommonScriptableObjects.sceneID.Set("1");
            
            ECSAudioSource model = CreateAudioSourceModel();
            model.volume = 0f;
            
            ECSAudioSource model2 = CreateAudioSourceModel();
            model2.volume = 1f;
            
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);
            
            // Act
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model2);

            // Assert
            Assert.AreEqual(audioSourceComponentHandler.audioSource.volume, model2.volume);
        }

        [UnityTest]
        public IEnumerator DisposeComponentCorrectly()
        {
            // Arrange
            ECSAudioSource model = CreateAudioSourceModel();
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Act
            audioSourceComponentHandler.OnComponentRemoved(scene, entity);
            yield return null;

            // Assert
            Assert.IsNull(audioSourceComponentHandler.audioSource);
            Assert.IsTrue(audioSourceComponentHandler.promiseAudioClip.isForgotten);
        }

        private ECSAudioSource CreateAudioSourceModel()
        {
            ECSAudioSource model = new ECSAudioSource();
            model.audioClipUrl = TestAssetsUtils.GetPath() + "/Audio/short_effect.ogg";
            return model;
        }
        
    }
}