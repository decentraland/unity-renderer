using System;
using DCL;
using DCL.Helpers;
using DCL.Components;
using DCL.Models;
using System.Collections;
using DCL.Components.Video.Plugin;
using UnityEngine;
using UnityEngine.TestTools;
using DCL.Controllers;
using DCL.Interface;
using DCL.SettingsCommon;
using DCL.Shaders;
using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;
using AudioSettings = DCL.SettingsCommon.AudioSettings;

namespace Tests
{
    public class VideoTextureShould : IntegrationTestSuite_Legacy
    {
        private Func<IVideoPluginWrapper> originalVideoPluginBuilder;

        private ISceneController sceneController => DCL.Environment.i.world.sceneController;
        private ParcelScene scene;
        private CoreComponentsPlugin coreComponentsPlugin;
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

            coreComponentsPlugin = new CoreComponentsPlugin();
            scene = TestUtils.CreateTestScene() as ParcelScene;
            CommonScriptableObjects.sceneNumber.Set(scene.sceneData.sceneNumber);

            originalVideoPluginBuilder = DCLVideoTexture.videoPluginWrapperBuilder;
            DCLVideoTexture.videoPluginWrapperBuilder = () => new VideoPluginWrapper_Mock();
        }

        protected override IEnumerator TearDown()
        {
            DCLVideoTexture.videoPluginWrapperBuilder = originalVideoPluginBuilder;
            coreComponentsPlugin.Dispose();
            sceneController.enabled = true;
            return base.TearDown();
        }

        [UnityTest]
        public IEnumerator BeCreatedCorrectly()
        {
            DCLVideoTexture videoTexture = CreateDCLVideoTexture(scene, "it-wont-load-during-test");
            yield return videoTexture.routine;
            Assert.IsTrue(videoTexture.attachedMaterials.Count == 0, "DCLVideoTexture started with attachedMaterials != 0");
        }

        [UnityTest]
        public IEnumerator SendMessageWhenVideoPlays()
        {
            var id = CreateDCLVideoClip(scene, "http://it-wont-load-during-test").id;
            DCLVideoTexture.Model model = new DCLVideoTexture.Model()
            {
                videoClipId = id,
                playing = true,
                seek = 10
            };
            var component = CreateDCLVideoTextureWithCustomTextureModel(scene, model);
            yield return component.routine;

            var expectedEvent = new WebInterface.SendVideoProgressEvent()
            {
                sceneNumber = scene.sceneData.sceneNumber,
                componentId = component.id,
                videoLength = 0,
                videoTextureId = id,
                currentOffset = 0,
                status = (int)VideoState.LOADING
            };

            var json = JsonUtility.ToJson(expectedEvent);
            var wasEventSent = false;
            yield return TestUtils.WaitForMessageFromEngine("VideoProgressEvent", json,
                () => { },
                () => wasEventSent = true);

            Assert.IsTrue(wasEventSent, $"Event of type {expectedEvent.GetType()} was not sent or its incorrect.");
        }

        [UnityTest]
        public IEnumerator SendMessageWhenVideoStops()
        {
            var id = CreateDCLVideoClip(scene, "http://it-wont-load-during-test").id;
            DCLVideoTexture.Model model = new DCLVideoTexture.Model()
            {
                videoClipId = id,
                playing = false
            };
            var component = CreateDCLVideoTextureWithCustomTextureModel(scene, model);

            var expectedEvent = new WebInterface.SendVideoProgressEvent()
            {
                sceneNumber = scene.sceneData.sceneNumber,
                componentId = component.id,
                videoLength = 0,
                videoTextureId = id,
                currentOffset = 0,
                status = (int)VideoState.LOADING
            };

            var json = JsonUtility.ToJson(expectedEvent);
            var wasEventSent = false;
            yield return TestUtils.WaitForMessageFromEngine("VideoProgressEvent", json,
                () => { },
                () => wasEventSent = true);
            yield return component.routine;

            Assert.IsTrue(wasEventSent, $"Event of type {expectedEvent.GetType()} was not sent or its incorrect.");
        }

        [UnityTest]
        public IEnumerator SendMessageWhenVideoIsUpdatedAfterTime()
        {
            var id = CreateDCLVideoClip(scene, "http://it-wont-load-during-test").id;
            DCLVideoTexture.Model model = new DCLVideoTexture.Model()
            {
                videoClipId = id,
                playing = true
            };
            var component = CreateDCLVideoTextureWithCustomTextureModel(scene, model);
            yield return component.routine;

            var expectedEvent = new WebInterface.SendVideoProgressEvent()
            {
                sceneNumber = scene.sceneData.sceneNumber,
                componentId = component.id,
                videoLength = 0,
                videoTextureId = id,
                currentOffset = 0,
                status = (int)VideoState.LOADING
            };

            var json = JsonUtility.ToJson(expectedEvent);
            var wasEventSent = false;
            yield return TestUtils.WaitForMessageFromEngine("VideoProgressEvent", json,
                () => { },
                () => wasEventSent = true);

            Assert.IsTrue(wasEventSent, $"Event of type {expectedEvent.GetType()} was not sent or its incorrect.");
        }

        [UnityTest]
        public IEnumerator VideoTextureReplaceOtherTextureCorrectly()
        {
            DCLVideoTexture videoTexture = CreateDCLVideoTexture(scene, "it-wont-load-during-test");
            yield return videoTexture.routine;
            Assert.IsTrue(videoTexture.attachedMaterials.Count == 0, "DCLVideoTexture started with attachedMaterials != 0");

            DCLTexture dclTexture = TestUtils.CreateDCLTexture(scene, TestAssetsUtils.GetPath() + "/Images/atlas.png");

            yield return dclTexture.routine;

            BasicMaterial mat = TestUtils.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>
            (scene, CLASS_ID.BASIC_MATERIAL,
                new BasicMaterial.Model
                {
                    texture = dclTexture.id
                });

            yield return mat.routine;

            yield return TestUtils.SharedComponentUpdate(mat, new BasicMaterial.Model() { texture = videoTexture.id });

            Assert.IsTrue(videoTexture.attachedMaterials.Count == 1, $"did DCLVideoTexture attach to material? {videoTexture.attachedMaterials.Count} expected 1");
        }

        [UnityTest]
        public IEnumerator AttachAndDetachCorrectly()
        {
            DCLVideoTexture videoTexture = CreateDCLVideoTexture(scene, "it-wont-load-during-test");
            yield return videoTexture.routine;
            Assert.IsTrue(videoTexture.attachedMaterials.Count == 0, "DCLVideoTexture started with attachedMaterials != 0");

            BasicMaterial mat2 = TestUtils.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>
            (
                scene,
                CLASS_ID.BASIC_MATERIAL,
                new BasicMaterial.Model
                {
                    texture = videoTexture.id
                }
            );

            yield return mat2.routine;

            Assert.IsTrue(videoTexture.attachedMaterials.Count == 1, $"did DCLVideoTexture attach to material? {videoTexture.attachedMaterials.Count} expected 1");

            // TEST: DCLVideoTexture detach on material disposed
            mat2.Dispose();
            Assert.IsTrue(videoTexture.attachedMaterials.Count == 0, $"did DCLVideoTexture detach from material? {videoTexture.attachedMaterials.Count} expected 0");

            videoTexture.Dispose();

            yield return null;
            Assert.IsTrue(videoTexture.texture == null, "DCLVideoTexture didn't dispose correctly?");
        }

        [UnityTest]
        public IEnumerator SetVisibleStateCorrectlyWhenAddedToAMaterialNotAttachedToShape()
        {
            DCLVideoTexture videoTexture = CreateDCLVideoTexture(scene, "it-wont-load-during-test");
            yield return videoTexture.routine;

            var ent1 = TestUtils.CreateSceneEntity(scene);
            BasicMaterial ent1Mat = TestUtils.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>(scene, CLASS_ID.BASIC_MATERIAL, new BasicMaterial.Model() { texture = videoTexture.id });
            TestUtils.SharedComponentAttach(ent1Mat, ent1);
            yield return ent1Mat.routine;

            Assert.IsTrue(!videoTexture.isVisible, "DCLVideoTexture should not be visible without a shape");
        }

        [UnityTest]
        public IEnumerator SetVisibleStateCorrectly()
        {
            DCLVideoTexture videoTexture = CreateDCLVideoTexture(scene, "it-wont-load-during-test");
            yield return videoTexture.routine;

            var ent1 = TestUtils.CreateSceneEntity(scene);
            BasicMaterial ent1Mat = TestUtils.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>(scene, CLASS_ID.BASIC_MATERIAL, new BasicMaterial.Model() { texture = videoTexture.id });
            TestUtils.SharedComponentAttach(ent1Mat, ent1);
            yield return ent1Mat.routine;

            BoxShape ent1Shape = TestUtils.SharedComponentCreate<BoxShape, BoxShape.Model>(scene, CLASS_ID.BOX_SHAPE, new BoxShape.Model());
            yield return ent1Shape.routine;

            TestUtils.SharedComponentAttach(ent1Shape, ent1);
            yield return new WaitForAllMessagesProcessed();
            Assert.IsTrue(videoTexture.isVisible, "DCLVideoTexture should be visible");

            yield return TestUtils.SharedComponentUpdate<BoxShape, BoxShape.Model>(ent1Shape, new BoxShape.Model() { visible = false });
            yield return new WaitForAllMessagesProcessed();

            Assert.IsTrue(!videoTexture.isVisible, "DCLVideoTexture should not be visible ");
        }

        [UnityTest]
        public IEnumerator SetVisibleStateCorrectlyWhenAddedToAlreadyAttachedMaterial()
        {
            DCLVideoTexture videoTexture = CreateDCLVideoTexture(scene, "it-wont-load-during-test");
            yield return videoTexture.routine;

            var ent1 = TestUtils.CreateSceneEntity(scene);
            BasicMaterial ent1Mat = TestUtils.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>(scene, CLASS_ID.BASIC_MATERIAL, new BasicMaterial.Model());
            TestUtils.SharedComponentAttach(ent1Mat, ent1);
            yield return ent1Mat.routine;

            BoxShape ent1Shape = TestUtils.SharedComponentCreate<BoxShape, BoxShape.Model>(scene, CLASS_ID.BOX_SHAPE, new BoxShape.Model());
            yield return ent1Shape.routine;

            TestUtils.SharedComponentAttach(ent1Shape, ent1);

            yield return TestUtils.SharedComponentUpdate<BasicMaterial, BasicMaterial.Model>(ent1Mat, new BasicMaterial.Model() { texture = videoTexture.id });
            yield return new WaitForAllMessagesProcessed();
            Assert.IsTrue(videoTexture.isVisible, "DCLVideoTexture should be visible");

            yield return TestUtils.SharedComponentUpdate<BoxShape, BoxShape.Model>(ent1Shape, new BoxShape.Model() { visible = false });
            yield return new WaitForAllMessagesProcessed();

            Assert.IsTrue(!videoTexture.isVisible, "DCLVideoTexture should not be visible ");
        }

        [UnityTest]
        public IEnumerator SetVisibleStateCorrectlyWhenEntityIsRemoved()
        {
            DCLVideoTexture videoTexture = CreateDCLVideoTexture(scene, "it-wont-load-during-test");
            yield return videoTexture.routine;

            var ent1 = TestUtils.CreateSceneEntity(scene);
            BasicMaterial ent1Mat = TestUtils.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>(scene, CLASS_ID.BASIC_MATERIAL, new BasicMaterial.Model() { texture = videoTexture.id });
            TestUtils.SharedComponentAttach(ent1Mat, ent1);
            yield return ent1Mat.routine;

            BoxShape ent1Shape = TestUtils.SharedComponentCreate<BoxShape, BoxShape.Model>(scene, CLASS_ID.BOX_SHAPE, new BoxShape.Model());
            yield return ent1Shape.routine;

            TestUtils.SharedComponentAttach(ent1Shape, ent1);
            yield return new WaitForAllMessagesProcessed();
            Assert.IsTrue(videoTexture.isVisible, "DCLVideoTexture should be visible");

            scene.RemoveEntity(ent1.entityId, true);
            yield return new WaitForAllMessagesProcessed();

            Assert.IsTrue(!videoTexture.isVisible, "DCLVideoTexture should not be visible ");
        }

        [UnityTest]
        public IEnumerator MuteWhenCreatedAndNoUserIsInTheScene()
        {
            // We disable SceneController monobehaviour to avoid its current scene id update
            sceneController.enabled = false;
            scene.isPersistent = false;

            // Set current scene as a different one
            CommonScriptableObjects.sceneNumber.Set(666666);

            DCLVideoTexture videoTexture = CreateDCLVideoTexture(scene, "it-wont-load-during-test");
            yield return videoTexture.routine;

            var ent1 = TestUtils.CreateSceneEntity(scene);
            BasicMaterial ent1Mat = TestUtils.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>(scene, CLASS_ID.BASIC_MATERIAL, new BasicMaterial.Model() { texture = videoTexture.id });
            TestUtils.SharedComponentAttach(ent1Mat, ent1);
            yield return ent1Mat.routine;

            BoxShape ent1Shape = TestUtils.SharedComponentCreate<BoxShape, BoxShape.Model>(scene, CLASS_ID.BOX_SHAPE, new BoxShape.Model());
            yield return ent1Shape.routine;

            TestUtils.SharedComponentAttach(ent1Shape, ent1);
            yield return new WaitForAllMessagesProcessed();

            // Check the volume
            Assert.AreEqual(0f, videoTexture.texturePlayer.volume);
        }

        [UnityTest]
        public IEnumerator UpdateTexturePlayerVolumeWhenAudioSettingsChange()
        {
            var id = CreateDCLVideoClip(scene, "http://it-wont-load-during-test").id;

            DCLVideoTexture.Model model = new DCLVideoTexture.Model()
            {
                videoClipId = id,
                playing = true,
                volume = 1
            };
            var component = CreateDCLVideoTextureWithCustomTextureModel(scene, model);

            yield return component.routine;

            Assert.AreApproximatelyEqual(1f, component.texturePlayer.volume, 0.01f);

            AudioSettings settings = Settings.i.audioSettings.Data;
            settings.sceneSFXVolume = 0.5f;
            Settings.i.audioSettings.Apply(settings);

            var expectedVolume = Utils.ToVolumeCurve(0.5f);
            Assert.AreApproximatelyEqual(expectedVolume, component.texturePlayer.volume, 0.01f);

            settings.sceneSFXVolume = 1f;
            Settings.i.audioSettings.Apply(settings);

            Assert.AreApproximatelyEqual(1, component.texturePlayer.volume, 0.01f);

            DCLVideoTexture.videoPluginWrapperBuilder = originalVideoPluginBuilder;
        }

        [UnityTest]
        public IEnumerator UnmuteWhenVideoIsCreatedWithUserInScene()
        {
            // We disable SceneController monobehaviour to avoid its current scene id update
            sceneController.enabled = false;

            // Set current scene with this scene's id
            CommonScriptableObjects.sceneNumber.Set(scene.sceneData.sceneNumber);

            DCLVideoTexture videoTexture = CreateDCLVideoTexture(scene, "it-wont-load-during-test");
            yield return videoTexture.routine;

            var ent1 = TestUtils.CreateSceneEntity(scene);
            BasicMaterial ent1Mat = TestUtils.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>(scene, CLASS_ID.BASIC_MATERIAL, new BasicMaterial.Model() { texture = videoTexture.id });
            TestUtils.SharedComponentAttach(ent1Mat, ent1);
            yield return ent1Mat.routine;

            BoxShape ent1Shape = TestUtils.SharedComponentCreate<BoxShape, BoxShape.Model>(scene, CLASS_ID.BOX_SHAPE, new BoxShape.Model());
            yield return ent1Shape.routine;

            TestUtils.SharedComponentAttach(ent1Shape, ent1);
            yield return new WaitForAllMessagesProcessed();

            // Check the volume
            Assert.AreEqual(videoTexture.GetVolume(), videoTexture.texturePlayer.volume);
        }

        [UnityTest]
        public IEnumerator MuteWhenUserLeavesScene()
        {
            // We disable SceneController monobehaviour to avoid its current scene id update
            sceneController.enabled = false;
            scene.isPersistent = false;

            // Set current scene with this scene's id
            CommonScriptableObjects.sceneNumber.Set(scene.sceneData.sceneNumber);
            yield return null;

            DCLVideoTexture videoTexture = CreateDCLVideoTexture(scene, "it-wont-load-during-test");
            yield return videoTexture.routine;

            var ent1 = TestUtils.CreateSceneEntity(scene);
            BasicMaterial ent1Mat = TestUtils.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>(scene, CLASS_ID.BASIC_MATERIAL, new BasicMaterial.Model() { texture = videoTexture.id });
            TestUtils.SharedComponentAttach(ent1Mat, ent1);
            yield return ent1Mat.routine;

            BoxShape ent1Shape = TestUtils.SharedComponentCreate<BoxShape, BoxShape.Model>(scene, CLASS_ID.BOX_SHAPE, new BoxShape.Model());
            yield return ent1Shape.routine;

            TestUtils.SharedComponentAttach(ent1Shape, ent1);
            yield return new WaitForAllMessagesProcessed();

            // Set current scene as a different one
            CommonScriptableObjects.sceneNumber.Set(666666);

            // to force the video player to update its volume
            CommonScriptableObjects.playerCoords.Set(new Vector2Int(666, 666));

            yield return null;

            // Check the volume
            Assert.AreEqual(0f, videoTexture.texturePlayer.volume);
        }

        [UnityTest]
        public IEnumerator VolumeIsUnmutedWhenUserEntersScene()
        {
            // We disable SceneController monobehaviour to avoid its current scene id update
            sceneController.enabled = false;

            // Set current scene as a different one
            CommonScriptableObjects.sceneNumber.Set(666666);

            DCLVideoTexture videoTexture = CreateDCLVideoTexture(scene, "it-wont-load-during-test");
            yield return videoTexture.routine;

            var ent1 = TestUtils.CreateSceneEntity(scene);
            BasicMaterial ent1Mat = TestUtils.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>(scene, CLASS_ID.BASIC_MATERIAL, new BasicMaterial.Model() { texture = videoTexture.id });
            TestUtils.SharedComponentAttach(ent1Mat, ent1);
            yield return ent1Mat.routine;

            BoxShape ent1Shape = TestUtils.SharedComponentCreate<BoxShape, BoxShape.Model>(scene, CLASS_ID.BOX_SHAPE, new BoxShape.Model());
            yield return ent1Shape.routine;

            TestUtils.SharedComponentAttach(ent1Shape, ent1);
            yield return new WaitForAllMessagesProcessed();

            // Set current scene with this scene's id
            CommonScriptableObjects.sceneNumber.Set(scene.sceneData.sceneNumber);

            // to force the video player to update its volume
            CommonScriptableObjects.playerCoords.Set(new Vector2Int(666, 666));

            yield return null;

            // Check the volume
            Assert.AreEqual(videoTexture.GetVolume(), videoTexture.texturePlayer.volume);
        }

        [Category("Flaky")]
        [UnityTest]
        public IEnumerator VideoTextureIsDisposedAndReloadedCorrectly()
        {
            IDCLEntity entity = TestUtils.CreateSceneEntity(scene);
            DCLVideoTexture texture = CreateDCLVideoTexture(scene, "it-wont-load-during-test");

            BasicMaterial basicMaterial = TestUtils.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>(
                scene,
                DCL.Models.CLASS_ID.BASIC_MATERIAL,
                new BasicMaterial.Model()
                {
                    texture = texture.id
                });

            TestUtils.SharedComponentAttach(basicMaterial, entity);
            TestUtils.SharedComponentAttach(texture, entity);

            yield return basicMaterial.routine;

            Texture mainTex = basicMaterial.material.GetTexture(ShaderUtils.BaseMap);

            // texture should be created
            Assert.IsTrue(mainTex);

            TestUtils.SharedComponentUpdate(basicMaterial, new BasicMaterial.Model()
            {
                texture =  CreateDCLVideoTexture(scene, "it-wont-load-during-test2").id
            });

            yield return basicMaterial.routine;

            // texture should have being disposed
            Assert.IsFalse(mainTex);

            TestUtils.SharedComponentUpdate(basicMaterial, new BasicMaterial.Model()
            {
                texture = texture.id
            });

            yield return basicMaterial.routine;

            // texture should have being reloaded
            Assert.IsTrue(basicMaterial.material.GetTexture(ShaderUtils.BaseMap));
        }

        static DCLVideoClip CreateDCLVideoClip(ParcelScene scn, string url)
        {
            return TestUtils.SharedComponentCreate<DCLVideoClip, DCLVideoClip.Model>
            (
                scn,
                DCL.Models.CLASS_ID.VIDEO_CLIP,
                new DCLVideoClip.Model
                {
                    url = url
                }
            );
        }

        static DCLVideoTexture CreateDCLVideoTexture(ParcelScene scn, DCLVideoClip clip)
        {
            return TestUtils.SharedComponentCreate<DCLVideoTexture, DCLVideoTexture.Model>
            (
                scn,
                DCL.Models.CLASS_ID.VIDEO_TEXTURE,
                new DCLVideoTexture.Model
                {
                    videoClipId = clip.id
                }
            );
        }

        static DCLVideoTexture CreateDCLVideoTextureWithModel(ParcelScene scn, DCLVideoTexture.Model model)
        {
            return TestUtils.SharedComponentCreate<DCLVideoTexture, DCLVideoTexture.Model>
            (
                scn,
                CLASS_ID.VIDEO_TEXTURE,
                model
            );
        }

        static DCLVideoTexture CreateDCLVideoTexture(ParcelScene scn, string url) { return CreateDCLVideoTexture(scn, CreateDCLVideoClip(scn, "http://" + url)); }
        static DCLVideoTexture CreateDCLVideoTextureWithCustomTextureModel(ParcelScene scn, DCLVideoTexture.Model model) { return CreateDCLVideoTextureWithModel(scn, model); }
    }
}
