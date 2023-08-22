using DCL;
using DCL.Controllers;
using DCL.ECSComponents;
using DCL.Models;
using DCL.SettingsCommon;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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
            string expectedUrl = "http://fake/audio.mp4";
            PBAudioStream model = CreateAudioStreamModel();
            model.Url = "http://fake2/audio.mp4";
            sceneData.allowedMediaHostnames = new[] { "fake", "fake2" };
            sceneData.requiredPermissions = new[] { ScenePermissionNames.ALLOW_MEDIA_HOSTNAMES };

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
        public void DoNotPlayAudioIfConditionsAreMeet()
        {
            const string URL = "http://fake/audio.mp4";

            // Arrange
            sceneData.allowedMediaHostnames = new[] { "fake" };
            sceneData.requiredPermissions = new[] { ScenePermissionNames.ALLOW_MEDIA_HOSTNAMES };

            PBAudioStream model = CreateAudioStreamModel();
            model.Playing = true;
            model.Url = URL;
            audioSourceComponentHandler.isInsideScene = true;
            audioSourceComponentHandler.isRendererActive = true;
            audioSourceComponentHandler.hadUserInteraction = false;

            // Act
            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);

            // Assert
            Assert.IsFalse(audioSourceComponentHandler.isPlaying);
            Assert.AreEqual(URL, audioSourceComponentHandler.url);
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
        public void NotAllowExternalAudioStreamWithoutPermissionsSet()
        {
            PBAudioStream model = new PBAudioStream()
            {
                Url = "http://fake/audio.mp4",
            };

            sceneData.allowedMediaHostnames = new[] { "fake" };

            LogAssert.Expect(LogType.Error, "External media asset url error: 'allowedMediaHostnames' missing in scene.json file.");

            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);

            Assert.IsTrue(string.IsNullOrEmpty(audioSourceComponentHandler.url));
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

        [Test]
        public void NotAllowExternalAudioStreamWithWrongHostName()
        {
            PBAudioStream model = new PBAudioStream()
            {
                Url = "http://fake/audio.mp4",
                Playing = true
            };

            scene.sceneData.allowedMediaHostnames = new[] { "fakes" };
            scene.sceneData.requiredPermissions = new[] { ScenePermissionNames.ALLOW_MEDIA_HOSTNAMES };

            LogAssert.Expect(LogType.Error, $"External media asset url error: '{model.Url}' host name is not in 'allowedMediaHostnames' in scene.json file.");

            audioSourceComponentHandler.OnComponentModelUpdated(scene, entity, model);

            Assert.IsTrue(string.IsNullOrEmpty(audioSourceComponentHandler.url));
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
