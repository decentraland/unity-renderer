using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class AudioTests : IntegrationTestSuite_Legacy
    {
        protected override IEnumerator TearDown()
        {
            sceneController.enabled = true;
            return base.TearDown();
        }

        public DCLAudioClip CreateAudioClip(string url, bool loop, bool shouldTryToLoad, double volume)
        {
            DCLAudioClip.Model model = new DCLAudioClip.Model
            {
                url = url,
                loop = loop,
                shouldTryToLoad = shouldTryToLoad,
                volume = volume
            };

            return CreateAudioClip(model);
        }

        public DCLAudioClip CreateAudioClip(DCLAudioClip.Model model)
        {
            return TestHelpers.SharedComponentCreate<DCLAudioClip, DCLAudioClip.Model>(scene, CLASS_ID.AUDIO_CLIP, model);
        }

        public IEnumerator CreateAndLoadAudioClip(bool waitForLoading = true)
        {
            DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);
            yield return null;

            yield return TestHelpers.CreateAudioSourceWithClipForEntity(entity);

            DCLAudioSource dclAudioSource = entity.gameObject.GetComponentInChildren<DCLAudioSource>();
            AudioSource unityAudioSource = dclAudioSource.GetComponentInChildren<AudioSource>();

            Assert.IsTrue(scene.entities.ContainsKey(entity.entityId), "Entity was not created correctly!");
            Assert.IsTrue(dclAudioSource != null, "DCLAudioSource Creation Failure!");
            Assert.IsTrue(unityAudioSource != null, "Unity AudioSource Creation Failure!");

            yield return dclAudioSource.routine;

            Assert.IsTrue(unityAudioSource.isPlaying, "Audio Source is not playing when it should!");

            //NOTE(Brian): Stop test
            yield return TestHelpers.CreateAudioSource(scene,
                entityId: entity.entityId,
                audioClipId: "audioClipTest",
                playing: false);

            yield return null;

            Assert.IsTrue(!unityAudioSource.isPlaying, "Audio Source is playing when it should NOT play!");
        }

        /// <summary>
        /// This should test creating a audioclip/audiosource couple, wait for audioClip load and send playing:true afterwards.
        /// </summary>
        [UnityTest]
        public IEnumerator CreateAndLoadAudioClipTest()
        {
            yield return CreateAndLoadAudioClip(waitForLoading: true);
        }

        /// <summary>
        /// This should test creating a audioclip/audiosource couple but send playing:true before the audioClip finished loading.
        /// </summary>
        [UnityTest]
        public IEnumerator PlayAudioTestWithoutFinishLoading()
        {
            yield return CreateAndLoadAudioClip(waitForLoading: false);
        }

        [UnityTest]
        public IEnumerator AudioComponentMissingValuesGetDefaultedOnUpdate()
        {
            yield return TestHelpers.TestEntityComponentDefaultsOnUpdate<DCLAudioSource.Model, DCLAudioSource>(scene);
        }

        [UnityTest]
        public IEnumerator AudioClipMissingValuesGetDefaultedOnUpdate()
        {
            // 1. Create component with non-default configs
            DCLAudioClip.Model componentModel = new DCLAudioClip.Model
            {
                loop = true,
                shouldTryToLoad = false,
                volume = 0.8f
            };

            DCLAudioClip audioClip = CreateAudioClip(componentModel);

            yield return audioClip.routine;

            // 2. Check configured values
            Assert.IsTrue(audioClip.isLoop);
            Assert.IsFalse(audioClip.shouldTryLoad);
            Assert.AreEqual(0.8f, audioClip.volume);

            // 3. Update component with missing values
            componentModel = new DCLAudioClip.Model { };

            scene.SharedComponentUpdate(audioClip.id, JsonUtility.ToJson(componentModel));

            yield return audioClip.routine;

            // 4. Check defaulted values
            Assert.IsFalse(audioClip.isLoop);
            Assert.IsTrue(audioClip.shouldTryLoad);
            Assert.AreEqual(1f, audioClip.volume);
        }

        [UnityTest]
        public IEnumerator AudioIsLooped()
        {
            DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);
            yield return null;

            yield return TestHelpers.LoadAudioClip(scene, "1", DCL.Helpers.Utils.GetTestsAssetsPath() + "/Audio/short_effect.ogg", false, true, 1);

            yield return TestHelpers.CreateAudioSource(scene, entity.entityId, "1", true, loop: true);

            DCLAudioSource dclAudioSource = entity.components.Values.FirstOrDefault(x => x is DCLAudioSource) as DCLAudioSource;
            float initTime = dclAudioSource.audioSource.clip.length - 0.05f;
            dclAudioSource.audioSource.time = initTime;
            yield return new WaitForSeconds(0.1f);

            Assert.IsTrue(dclAudioSource.playTime > 0 && dclAudioSource.playTime < initTime);
        }

        [UnityTest]
        public IEnumerator AudioIsNotLooped()
        {
            DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);
            yield return null;

            yield return TestHelpers.LoadAudioClip(scene, "1", DCL.Helpers.Utils.GetTestsAssetsPath() + "/Audio/short_effect.ogg", false, true, 1);

            yield return TestHelpers.CreateAudioSource(scene, entity.entityId, "1", true, loop: false);

            DCLAudioSource dclAudioSource = entity.components.Values.FirstOrDefault(x => x is DCLAudioSource) as DCLAudioSource;
            dclAudioSource.audioSource.time = dclAudioSource.audioSource.clip.length - 0.05f;
            yield return new WaitForSeconds(0.1f);

            Assert.AreEqual(0, dclAudioSource.playTime);
        }

        [UnityTest]
        public IEnumerator AudioClipAttachedGetsReplacedOnNewAttachment()
        {
            yield return TestHelpers.TestAttachedSharedComponentOfSameTypeIsReplaced<DCLAudioClip.Model, DCLAudioClip>(
                scene, CLASS_ID.AUDIO_CLIP);
        }

        [UnityTest]
        public IEnumerator VolumeWhenAudioCreatedWithNoUserInScene()
        {
            // We disable SceneController monobehaviour to avoid its current scene id update
            sceneController.enabled = false;

            // Set current scene as a different one
            CommonScriptableObjects.sceneID.Set("unexistent-scene");

            DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);
            yield return null;

            yield return TestHelpers.CreateAudioSourceWithClipForEntity(entity);

            DCLAudioSource dclAudioSource = entity.gameObject.GetComponentInChildren<DCLAudioSource>();
            yield return dclAudioSource.routine;

            AudioSource unityAudioSource = dclAudioSource.GetComponentInChildren<AudioSource>();

            // Check the volume
            Assert.AreEqual(0f, unityAudioSource.volume);
        }

        [UnityTest]
        public IEnumerator VolumeWhenAudioCreatedWithUserInScene()
        {
            // We disable SceneController monobehaviour to avoid its current scene id update
            sceneController.enabled = false;

            // Set current scene with this scene's id
            CommonScriptableObjects.sceneID.Set(scene.sceneData.id);

            DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);
            yield return null;

            yield return TestHelpers.CreateAudioSourceWithClipForEntity(entity);

            DCLAudioSource dclAudioSource = entity.gameObject.GetComponentInChildren<DCLAudioSource>();
            yield return dclAudioSource.routine;

            AudioSource unityAudioSource = dclAudioSource.GetComponentInChildren<AudioSource>();

            // Check the volume
            Assert.AreEqual(unityAudioSource.volume, dclAudioSource.volume);
        }

        [UnityTest]
        public IEnumerator VolumeIsMutedWhenUserLeavesScene()
        {
            // We disable SceneController monobehaviour to avoid its current scene id update
            sceneController.enabled = false;

            // Set current scene with this scene's id
            CommonScriptableObjects.sceneID.Set(scene.sceneData.id);

            DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);
            yield return null;

            yield return TestHelpers.CreateAudioSourceWithClipForEntity(entity);

            DCLAudioSource dclAudioSource = entity.gameObject.GetComponentInChildren<DCLAudioSource>();
            yield return dclAudioSource.routine;

            AudioSource unityAudioSource = dclAudioSource.GetComponentInChildren<AudioSource>();

            // Set current scene as a different one
            CommonScriptableObjects.sceneID.Set("unexistent-scene");

            // Check the volume
            Assert.AreEqual(unityAudioSource.volume, 0f);
        }

        [UnityTest]
        public IEnumerator VolumeIsUnmutedWhenUserEntersScene()
        {
            // We disable SceneController monobehaviour to avoid its current scene id update
            sceneController.enabled = false;

            // Set current scene as a different one
            CommonScriptableObjects.sceneID.Set("unexistent-scene");

            DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);
            yield return null;

            yield return TestHelpers.CreateAudioSourceWithClipForEntity(entity);

            DCLAudioSource dclAudioSource = entity.gameObject.GetComponentInChildren<DCLAudioSource>();
            yield return dclAudioSource.routine;

            AudioSource unityAudioSource = dclAudioSource.GetComponentInChildren<AudioSource>();

            // Set current scene with this scene's id
            CommonScriptableObjects.sceneID.Set(scene.sceneData.id);

            // Check the volume
            Assert.AreEqual(unityAudioSource.volume, dclAudioSource.volume);
        }

        [UnityTest]
        public IEnumerator AudioStreamComponentCreation()
        {
            DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);
            DCLAudioStream.Model model = new DCLAudioStream.Model()
            {
                url = "https://audio.dcl.guru/radio/8110/radio.mp3",
                playing = false,
                volume = 1f
            };
            DCLAudioStream component = TestHelpers.EntityComponentCreate<DCLAudioStream, DCLAudioStream.Model>(scene, entity,model );
            
            yield return component.routine;
            Assert.IsFalse(component.GetModel().playing);

            model.playing = true;
            component.UpdateFromModel(model);
            yield return component.routine;
            Assert.IsTrue(component.GetModel().playing);
            
            model.playing = false;
            component.UpdateFromModel(model);
            yield return component.routine; 
            Assert.IsFalse(component.GetModel().playing);
        }

        [Test]
        public void AudioClip_OnReadyBeforeLoading()
        {
            DCLAudioClip dclAudioClip = CreateAudioClip(DCL.Helpers.Utils.GetTestsAssetsPath() + "/Audio/short_effect.ogg", true, true, 1);
            bool isOnReady = false;
            dclAudioClip.CallWhenReady((x) => { isOnReady = true; });

            Assert.IsTrue(isOnReady); //DCLAudioClip is ready on creation
        }

        [UnityTest]
        public IEnumerator AudioClip_OnReadyWaitLoading()
        {
            DCLAudioClip dclAudioClip = CreateAudioClip(DCL.Helpers.Utils.GetTestsAssetsPath() + "/Audio/short_effect.ogg", true, true, 1);
            bool isOnReady = false;
            dclAudioClip.CallWhenReady((x) => { isOnReady = true; });
            yield return dclAudioClip.routine;

            Assert.IsTrue(isOnReady);
        }

        [UnityTest]
        public IEnumerator AudioClip_OnReadyAfterLoadingInstantlyCalled()
        {
            DCLAudioClip dclAudioClip = CreateAudioClip(DCL.Helpers.Utils.GetTestsAssetsPath() + "/Audio/short_effect.ogg", true, true, 1);
            yield return dclAudioClip.routine;
            bool isOnReady = false;
            dclAudioClip.CallWhenReady((x) => { isOnReady = true; });

            Assert.IsTrue(isOnReady);
        }
    }
}