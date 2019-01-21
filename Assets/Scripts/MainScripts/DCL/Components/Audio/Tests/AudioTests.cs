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
    public class AudioTests
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

            scene.EntityComponentCreate(JsonUtility.ToJson(new DCL.Models.EntityComponentCreateMessage
            {
                entityId = entityId,
                name = "audioSourceTest",
                classId = (int)DCL.Models.CLASS_ID.AUDIO_SOURCE,
                json = JsonUtility.ToJson(audioSourceModel)
            }));
            
            yield return null;
        }

        public IEnumerator LoadAudioClip(ParcelScene scene, string audioClipId, string url, bool loop, bool loading, float volume, bool waitForLoading=true)
        {
            DCLAudioClip audioClip = null;

            audioClip = scene.SharedComponentCreate(JsonUtility.ToJson(new DCL.Models.SharedComponentCreateMessage
            {
                id = audioClipId,//"audioClipTest",
                name = "audioClip",
                classId = (int)DCL.Models.CLASS_ID.AUDIO_CLIP,
            })) as DCLAudioClip;

            yield return new WaitForSeconds(0.01f);

            Assert.NotNull(scene.disposableComponents.ContainsKey("audioClipTest"), "Shared component was not created correctly!");

            DCLAudioClip.Model model = new DCLAudioClip.Model
            {
                url = url,//"http://127.0.0.1:9991/Audio/Train.wav",
                loop = loop,
                shouldTryToLoad = loading,
                volume = volume
            };
            
            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = audioClipId,
                json = JsonUtility.ToJson(model)
            }));

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
            var sceneController = TestHelpers.InitializeSceneController(true);

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            yield return LoadAudioClip(scene,
                audioClipId: "audioClipTest",
                url: "http://127.0.0.1:9991/Audio/Train.wav",
                loop: true,
                loading: true,
                volume: 1.0f,
                waitForLoading: waitForLoading);

            string entityId = "e1";
            TestHelpers.CreateSceneEntity(scene, entityId);
            yield return new WaitForSeconds(0.01f);

            //NOTE(Brian): Play test
            yield return CreateAudioSource(scene,
                entityId: entityId,
                audioClipId: "audioClipTest",
                playing: true);

            DCLAudioSource dclAudioSource = scene.entities[entityId].gameObject.GetComponentInChildren<DCLAudioSource>();
            AudioSource unityAudioSource = scene.entities[entityId].gameObject.GetComponentInChildren<AudioSource>();

            Assert.NotNull(scene.entities.ContainsKey(entityId), "Entity was not created correctly!");
            Assert.NotNull(dclAudioSource, "DCLAudioSource Creation Failure!");
            Assert.NotNull(unityAudioSource, "Unity AudioSource Creation Failure!");

            yield return new WaitForSeconds(5f);

            Assert.IsTrue(unityAudioSource.isPlaying, "Audio Source is not playing when it should!");

            //NOTE(Brian): Stop test
            yield return CreateAudioSource(scene,
                entityId: entityId,
                audioClipId: "audioClipTest",
                playing: false);

            yield return new WaitForSeconds(0.1f);

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
    }
}
