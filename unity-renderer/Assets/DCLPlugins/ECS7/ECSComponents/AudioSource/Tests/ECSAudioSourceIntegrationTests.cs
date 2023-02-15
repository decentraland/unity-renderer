using System.Collections;
using DCL.Controllers;
using DCL.ECS7.InternalComponents;
using DCL.Helpers;
using DCL.Models;
using DCL.SettingsCommon;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.ECSComponents.Test
{
    public class ECSAudioSourceIntegrationTest : IntegrationTestSuite
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
            Settings.CreateSharedInstance(new DefaultSettingsFactory());

            gameObject = new GameObject();
            entity = Substitute.For<IDCLEntity>();
            scene = Substitute.For<IParcelScene>();
            audioSourceComponentHandler = new ECSAudioSourceComponentHandler(DataStore.i,Settings.i, AssetPromiseKeeper_AudioClip.i, CommonScriptableObjects.sceneNumber, Substitute.For<IInternalECSComponent<InternalAudioSource>>());

            entity.entityId.Returns(1);
            entity.gameObject.Returns(gameObject);
            LoadParcelScenesMessage.UnityParcelScene sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            sceneData.sceneNumber = 1;

            ContentProvider_Dummy providerDummy = new ContentProvider_Dummy();

            scene.sceneData.Configure().Returns(sceneData);
            scene.Configure().contentProvider.Returns(providerDummy);
            scene.Configure().isPersistent.Returns(false);
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
        public void VolumeWhenAudioCreatedWithNoUserInScene()
        {
            // Arrange
            CommonScriptableObjects.sceneNumber.Set(6);

            PBAudioSource model = CreateAudioSourceModel();
            model.Volume = 1f;

            // Act
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.AreEqual(0f, audioSourceComponentHandler.audioSource.volume);
        }

        [Test]
        public void VolumeWhenAudioCreatedWithUserInScene()
        {
            // Arrange
            CommonScriptableObjects.sceneNumber.Set(scene.sceneData.sceneNumber);

            PBAudioSource model = CreateAudioSourceModel();
            model.Volume = 1f;

            // Act
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.AreEqual(1f, audioSourceComponentHandler.audioSource.volume);
        }

        [Test]
        public void VolumeIsMutedWhenUserLeavesScene()
        {
            // Arrange
            CommonScriptableObjects.sceneNumber.Set(scene.sceneData.sceneNumber);

            PBAudioSource model = CreateAudioSourceModel();
            model.Volume = 1f;
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Act
            CommonScriptableObjects.sceneNumber.Set(6);

            // Assert
            Assert.AreEqual(0f, audioSourceComponentHandler.audioSource.volume);
        }

        [Test]
        public void VolumeIsUnmutedWhenUserEntersScene()
        {
            // Arrange
            CommonScriptableObjects.sceneNumber.Set(6);

            PBAudioSource model = CreateAudioSourceModel();
            model.Volume = 1f;
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Act
            CommonScriptableObjects.sceneNumber.Set(scene.sceneData.sceneNumber);

            // Assert
            Assert.AreEqual(1f, audioSourceComponentHandler.audioSource.volume);
        }

        [Test]
        public void VolumeIsNotMutedForPersistentScenes()
        {
            // Arrange
            scene.Configure().isPersistent.Returns(true);
            CommonScriptableObjects.sceneNumber.Set(scene.sceneData.sceneNumber);

            PBAudioSource model = CreateAudioSourceModel();
            model.Volume = 1f;
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Act
            CommonScriptableObjects.sceneNumber.Set(6);

            // Assert
            Assert.AreEqual(1f, audioSourceComponentHandler.audioSource.volume);
        }

        private PBAudioSource CreateAudioSourceModel()
        {
            PBAudioSource model = new PBAudioSource();
            model.AudioClipUrl = TestAssetsUtils.GetPath() + "/Audio/short_effect.ogg";
            return model;
        }

    }
}
