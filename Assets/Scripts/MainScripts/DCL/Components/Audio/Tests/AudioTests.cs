using System.Collections;
using System.Collections.Generic;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class AudioTests : TestsBase
    {
        public IEnumerator CreateAudioSource(ParcelScene scene, string entityId, string audioClipId, bool playing)
        {
            Debug.Log("Creating/updating audio source...");
            var audioSourceModel = new DCLAudioSource.Model()
            {
                audioClipId = audioClipId,
                playing = playing,
                volume = 1.0f,
                loop = true,
                pitch = 1.0f
            };

            DCLAudioSource audioSource = TestHelpers.EntityComponentCreate<DCLAudioSource, DCLAudioSource.Model>(scene, scene.entities[entityId], audioSourceModel);

            yield return audioSource.routine;
        }

        public IEnumerator LoadAudioClip(ParcelScene scene, string audioClipId, string url, bool loop, bool loading, float volume, bool waitForLoading = true)
        {
            DCLAudioClip.Model model = new DCLAudioClip.Model
            {
                url = url,//TestHelpers.GetTestsAssetsPath() + "/Audio/Train.wav",
                loop = loop,
                shouldTryToLoad = loading,
                volume = volume
            };

            DCLAudioClip audioClip = scene.SharedComponentCreate(JsonUtility.ToJson(new DCL.Models.SharedComponentCreateMessage
            {
                id = audioClipId,
                name = "audioClip",
                classId = (int)DCL.Models.CLASS_ID.AUDIO_CLIP,
            })) as DCLAudioClip;

            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = audioClipId,
                json = JsonUtility.ToJson(model)
            }));

            yield return audioClip.routine;

            Assert.IsTrue(scene.disposableComponents.ContainsKey("audioClipTest"), "Shared component was not created correctly!");

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
            yield return InitScene();

            yield return LoadAudioClip(scene,
                audioClipId: "audioClipTest",
                url: TestHelpers.GetTestsAssetsPath() + "/Audio/Train.wav",
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
            yield return InitScene();

            DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);

            // 1. Create component with non-default configs
            DCLAudioSource.Model componentModel = new DCLAudioSource.Model()
            {
                audioClipId = "audioClipTest",
                playing = false,
                volume = 0.3f,
                loop = true,
                pitch = 0.8f
            };

            DCLAudioSource audioSourceComponent = TestHelpers.EntityComponentCreate<DCLAudioSource, DCLAudioSource.Model>(scene, entity, componentModel);

            // 2. Check configured values
            Assert.AreEqual(0.3f, audioSourceComponent.model.volume);
            Assert.AreEqual(0.8f, audioSourceComponent.model.pitch);

            // 3. Update component with missing values
            componentModel = new DCLAudioSource.Model()
            {
                audioClipId = "audioClipTest",
                playing = false,
                loop = false
            };

            scene.EntityComponentUpdate(entity, CLASS_ID_COMPONENT.AUDIO_SOURCE, JsonUtility.ToJson(componentModel));

            // 4. Check changed values
            Assert.IsFalse(audioSourceComponent.model.loop);

            // 5. Check defaulted values
            Assert.AreEqual(1f, audioSourceComponent.model.volume);
            Assert.AreEqual(1f, audioSourceComponent.model.pitch);
        }

        [UnityTest]
        public IEnumerator AudioClipMissingValuesGetDefaultedOnUpdate()
        {
            yield return InitScene();

            // 1. Create component with non-default configs
            DCLAudioClip.Model componentModel = new DCLAudioClip.Model
            {
                loop = true,
                shouldTryToLoad = false,
                volume = 0.8f
            };

            DCLAudioClip audioClip = TestHelpers.SharedComponentCreate<DCLAudioClip, DCLAudioClip.Model>(scene, CLASS_ID.AUDIO_CLIP, componentModel);

            yield return audioClip.routine;

            // 2. Check configured values
            Assert.IsTrue(audioClip.model.loop);
            Assert.IsFalse(audioClip.model.shouldTryToLoad);
            Assert.AreEqual(0.8f, audioClip.model.volume);

            // 3. Update component with missing values
            componentModel = new DCLAudioClip.Model { };

            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = audioClip.id,
                json = JsonUtility.ToJson(componentModel)
            }));

            yield return audioClip.routine;

            // 4. Check defaulted values
            Assert.IsFalse(audioClip.model.loop);
            Assert.IsTrue(audioClip.model.shouldTryToLoad);
            Assert.AreEqual(1f, audioClip.model.volume);
        }
    }
}
