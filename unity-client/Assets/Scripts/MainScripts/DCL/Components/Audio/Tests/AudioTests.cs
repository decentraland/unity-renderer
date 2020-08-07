using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class AudioTests : TestsBase
    {
        public IEnumerator CreateAudioSource(ParcelScene scene, string entityId, string audioClipId, bool playing, bool loop = true)
        {
            var audioSourceModel = new DCLAudioSource.Model()
            {
                audioClipId = audioClipId,
                playing = playing,
                volume = 1.0f,
                loop = loop,
                pitch = 1.0f
            };

            DCLAudioSource audioSource =
                TestHelpers.EntityComponentCreate<DCLAudioSource, DCLAudioSource.Model>(scene, scene.entities[entityId],
                    audioSourceModel);

            yield return audioSource.routine;
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

        public IEnumerator LoadAudioClip(ParcelScene scene, string audioClipId, string url, bool loop, bool loading,
            float volume, bool waitForLoading = true)
        {
            DCLAudioClip.Model model = new DCLAudioClip.Model
            {
                url = url,
                loop = loop,
                shouldTryToLoad = loading,
                volume = volume
            };

            DCLAudioClip audioClip = scene.SharedComponentCreate(
                audioClipId,
                (int) CLASS_ID.AUDIO_CLIP
            ) as DCLAudioClip;

            scene.SharedComponentUpdate(audioClipId, JsonUtility.ToJson(model));

            yield return audioClip.routine;

            Assert.IsTrue(scene.disposableComponents.ContainsKey(audioClipId),
                "Shared component was not created correctly!");

            if (waitForLoading)
            {
                yield return new WaitUntil(
                    () =>
                    {
                        return audioClip.loadingState != DCLAudioClip.LoadState.LOADING_IN_PROGRESS &&
                               audioClip.loadingState != DCLAudioClip.LoadState.IDLE;
                    });
            }
        }

        public IEnumerator CreateAndLoadAudioClip(bool waitForLoading = true)
        {
            yield return LoadAudioClip(scene,
                audioClipId: "audioClipTest",
                url: DCL.Helpers.Utils.GetTestsAssetsPath() + "/Audio/Train.wav",
                loop: true,
                loading: true,
                volume: 1.0f,
                waitForLoading: waitForLoading);

            DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);
            yield return null;

            //NOTE(Brian): Play test
            yield return CreateAudioSource(scene,
                entityId: entity.entityId,
                audioClipId: "audioClipTest",
                playing: true);

            DCLAudioSource dclAudioSource = entity.gameObject.GetComponentInChildren<DCLAudioSource>();
            AudioSource unityAudioSource = dclAudioSource.GetComponentInChildren<AudioSource>();

            Assert.IsTrue(scene.entities.ContainsKey(entity.entityId), "Entity was not created correctly!");
            Assert.IsTrue(dclAudioSource != null, "DCLAudioSource Creation Failure!");
            Assert.IsTrue(unityAudioSource != null, "Unity AudioSource Creation Failure!");

            yield return dclAudioSource.routine;

            Assert.IsTrue(unityAudioSource.isPlaying, "Audio Source is not playing when it should!");

            //NOTE(Brian): Stop test
            yield return CreateAudioSource(scene,
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
            Assert.IsTrue(audioClip.model.loop);
            Assert.IsFalse(audioClip.model.shouldTryToLoad);
            Assert.AreEqual(0.8f, audioClip.model.volume);

            // 3. Update component with missing values
            componentModel = new DCLAudioClip.Model { };

            scene.SharedComponentUpdate(audioClip.id, JsonUtility.ToJson(componentModel));

            yield return audioClip.routine;

            // 4. Check defaulted values
            Assert.IsFalse(audioClip.model.loop);
            Assert.IsTrue(audioClip.model.shouldTryToLoad);
            Assert.AreEqual(1f, audioClip.model.volume);
        }

        [UnityTest]
        public IEnumerator AudioIsLooped()
        {
            DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);
            yield return null;

            yield return LoadAudioClip(scene, "1", DCL.Helpers.Utils.GetTestsAssetsPath() + "/Audio/short_effect.ogg", false, true, 1);

            yield return CreateAudioSource(scene, entity.entityId, "1", true, loop: true);

            yield return new WaitForSeconds((scene.GetSharedComponent("1") as DCLAudioClip).audioClip.length + 0.1f);

            DCLAudioSource dclAudioSource = entity.components.Values.FirstOrDefault(x => x is DCLAudioSource) as DCLAudioSource;
            Assert.AreNotEqual(0, dclAudioSource.playTime);
        }

        [UnityTest]
        public IEnumerator AudioIsNotLooped()
        {
            DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);
            yield return null;

            yield return LoadAudioClip(scene, "1", DCL.Helpers.Utils.GetTestsAssetsPath() + "/Audio/short_effect.ogg", false, true, 1);

            yield return CreateAudioSource(scene, entity.entityId, "1", true, loop: false);

            yield return new WaitForSeconds((scene.GetSharedComponent("1") as DCLAudioClip).audioClip.length + 0.1f);

            DCLAudioSource dclAudioSource = entity.components.Values.FirstOrDefault(x => x is DCLAudioSource) as DCLAudioSource;
            Assert.AreEqual(0, dclAudioSource.playTime);
        }

        [UnityTest]
        public IEnumerator AudioClipAttachedGetsReplacedOnNewAttachment()
        {
            yield return TestHelpers.TestAttachedSharedComponentOfSameTypeIsReplaced<DCLAudioClip.Model, DCLAudioClip>(
                scene, CLASS_ID.AUDIO_CLIP);
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