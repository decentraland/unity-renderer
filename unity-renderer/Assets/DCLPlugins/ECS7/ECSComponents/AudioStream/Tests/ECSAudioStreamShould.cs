﻿using DCL;
using DCL.Controllers;
using DCL.ECSComponents;
using DCL.Models;
using DCL.SettingsCommon;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;

namespace Tests
{
    public class ECSAudioStreamShould
    {
        private IDCLEntity entity;
        private IParcelScene scene;
        private ECSAudioStreamComponentHandler audioSourceComponentHandler;
        private LoadParcelScenesMessage.UnityParcelScene sceneData;
        private ContentProvider contentProvider;

        [SetUp]
        public void SetUp()
        {
            Settings.CreateSharedInstance(new DefaultSettingsFactory());

            entity = Substitute.For<IDCLEntity>();
            scene = Substitute.For<IParcelScene>();
            audioSourceComponentHandler = new ECSAudioStreamComponentHandler();

            entity.entityId.Returns(1);

            sceneData = new LoadParcelScenesMessage.UnityParcelScene
            {
                sceneNumber = 1
            };

            scene.sceneData.Configure().Returns(sceneData);

            contentProvider = new ContentProvider();

            contentProvider.contents.Add(new ContentServerUtils.MappingPair()
            {
                file = "https://audio.dcl.guru/radio/8110/radio.mp3", hash = "https://audio.dcl.guru/radio/8110/radio.mp3"
            });

            contentProvider.BakeHashes();
            scene.contentProvider.Returns(contentProvider);

            audioSourceComponentHandler.OnComponentCreated(scene, entity);
        }

        [TearDown]
        public void TearDown()
        {
            audioSourceComponentHandler.OnComponentRemoved(scene, entity);
        }

        [Test]
        public void UpdatePlayingModelComponentCorrectly()
        {
            // Arrange

            // We prepare the componentHandler
            audioSourceComponentHandler.isInsideScene = true;
            audioSourceComponentHandler.isRendererActive = true;
            audioSourceComponentHandler.hadUserInteraction = true;

            // We prepare the models
            PBAudioStream model = CreateAudioStreamModel();
            model.Playing = false;
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);

            PBAudioStream model2 = CreateAudioStreamModel();
            model2.Playing = true;

            // Act
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model2);

            // Assert
            Assert.AreEqual(audioSourceComponentHandler.isPlaying, true);
        }

        [Test]
        public void UpdateVolumeModelComponentCorrectly()
        {
            // Arrange
            PBAudioStream model = CreateAudioStreamModel();
            model.Volume = 0f;
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);

            PBAudioStream model2 = CreateAudioStreamModel();
            model2.Volume = 1f;

            // Act
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model2);

            // Assert
            Assert.AreEqual(1f, audioSourceComponentHandler.currentVolume);
        }

        [Test]
        public void UpdateUrlModelComponentCorrectly()
        {
            // Arrange
            string expectedUrl = "NewUrl";
            PBAudioStream model = CreateAudioStreamModel();
            model.Url = "OldUrl";
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);

            PBAudioStream model2 = CreateAudioStreamModel();
            model2.Url = expectedUrl;

            // Act
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model2);

            // Assert
            Assert.AreEqual(expectedUrl, audioSourceComponentHandler.model.Url);
        }

        [Test]
        public void PlayAudioIfConditionsAreMeet()
        {
            // Arrange
            PBAudioStream model = CreateAudioStreamModel();
            model.Playing = true;
            audioSourceComponentHandler.isInsideScene = true;
            audioSourceComponentHandler.isRendererActive = true;
            audioSourceComponentHandler.hadUserInteraction = true;

            // Act
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.IsTrue(audioSourceComponentHandler.isPlaying);
        }

        [Test]
        public void StopAudioIfRendererIsDisable()
        {
            // Arrange
            PBAudioStream model = CreateAudioStreamModel();
            model.Playing = true;
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
            PBAudioStream model = CreateAudioStreamModel();
            model.Playing = true;
            audioSourceComponentHandler.isInsideScene = false;
            audioSourceComponentHandler.isRendererActive = true;

            // Act
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.IsFalse(audioSourceComponentHandler.isPlaying);
        }

        [Test]
        public void DontAllowExternalAudioStreamWithoutPermissionsSet()
        {
            PBAudioStream model = new PBAudioStream()
            {
                Url = "http://fake/audio.mp4",
            };

            sceneData.allowedMediaHostnames = new[] { "fake" };

            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);

            Assert.IsNull(audioSourceComponentHandler.url);
        }

        [Test]
        public void AllowExternalAudioStreamWithPermissionsSet()
        {
            PBAudioStream model = new PBAudioStream()
            {
                Url = "http://fake/audio.mp4",
            };

            sceneData.allowedMediaHostnames = new[] { "fake" };
            sceneData.requiredPermissions = new[] { ScenePermissionNames.ALLOW_MEDIA_HOSTNAMES };

            audioSourceComponentHandler.isInsideScene = true;
            audioSourceComponentHandler.isRendererActive = true;
            audioSourceComponentHandler.hadUserInteraction = true;

            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);

            Assert.AreEqual(model.Url, audioSourceComponentHandler.url);
        }

        private PBAudioStream CreateAudioStreamModel()
        {
            PBAudioStream model = new PBAudioStream()
            {
                Url = "https://audio.dcl.guru/radio/8110/radio.mp3",
                Playing = true,
                Volume = 1f
            };

            return model;
        }
    }
}
