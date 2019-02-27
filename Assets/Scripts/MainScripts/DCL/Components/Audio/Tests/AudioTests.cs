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
                classId = (int)DCL.Models.CLASS_ID_COMPONENT.AUDIO_SOURCE,
                json = JsonUtility.ToJson(audioSourceModel)
            }));

            yield return null;
        }

        public IEnumerator LoadAudioClip(ParcelScene scene, string audioClipId, string url, bool loop, bool loading, float volume, bool waitForLoading = true)
        {
            DCLAudioClip audioClip = null;

            audioClip = scene.SharedComponentCreate(JsonUtility.ToJson(new DCL.Models.SharedComponentCreateMessage
            {
                id = audioClipId,//"audioClipTest",
                name = "audioClip",
                classId = (int)DCL.Models.CLASS_ID.AUDIO_CLIP,
            })) as DCLAudioClip;

            yield return new WaitForSeconds(0.01f);

            Assert.IsTrue(scene.disposableComponents.ContainsKey("audioClipTest"), "Shared component was not created correctly!");

            DCLAudioClip.Model model = new DCLAudioClip.Model
            {
                url = url,//TestHelpers.GetTestsAssetsPath() + "/Audio/Train.wav",
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
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            yield return LoadAudioClip(scene,
                audioClipId: "audioClipTest",
                url: TestHelpers.GetTestsAssetsPath() + "/Audio/Train.wav",
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
            AudioSource unityAudioSource = dclAudioSource.GetComponentInChildren<AudioSource>();

            Assert.IsTrue(scene.entities.ContainsKey(entityId), "Entity was not created correctly!");
            Assert.IsTrue(dclAudioSource != null, "DCLAudioSource Creation Failure!");
            Assert.IsTrue(unityAudioSource != null, "Unity AudioSource Creation Failure!");

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

        [UnityTest]
        public IEnumerator AudioComponentMissingValuesGetDefaultedOnUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForEndOfFrame();

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForEndOfFrame();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            // 1. Create component with non-default configs
            string componentJSON = JsonUtility.ToJson(new DCLAudioSource.Model()
            {
                audioClipId = "audioClipTest",
                playing = false,
                volume = 0.3f,
                loop = true,
                pitch = 0.8f
            });

            DCLAudioSource audioSourceComponent = (DCLAudioSource)scene.EntityComponentCreate(JsonUtility.ToJson(new DCL.Models.EntityComponentCreateMessage
            {
                entityId = entityId,
                name = "audioSourceTest",
                classId = (int)DCL.Models.CLASS_ID_COMPONENT.AUDIO_SOURCE,
                json = componentJSON
            }));

            // 2. Check configured values
            Assert.AreEqual(0.3f, audioSourceComponent.model.volume);
            Assert.AreEqual(0.8f, audioSourceComponent.model.pitch);

            // 3. Update component with missing values
            componentJSON = JsonUtility.ToJson(new DCLAudioSource.Model()
            {
                audioClipId = "audioClipTest",
                playing = false,
                loop = false
            });

            scene.EntityComponentUpdate(scene.entities[entityId], CLASS_ID_COMPONENT.AUDIO_SOURCE, componentJSON);

            // 4. Check changed values
            Assert.IsFalse(audioSourceComponent.model.loop);

            // 5. Check defaulted values
            Assert.AreEqual(1f, audioSourceComponent.model.volume);
            Assert.AreEqual(1f, audioSourceComponent.model.pitch);
        }

        [UnityTest]
        public IEnumerator AudioClipMissingValuesGetDefaultedOnUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForSeconds(0.01f);

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForSeconds(0.01f);

            // 1. Create component with non-default configs
            string componentJSON = JsonUtility.ToJson(new DCLAudioClip.Model
            {
                loop = true,
                shouldTryToLoad = false,
                volume = 0.8f
            });

            string componentId = TestHelpers.GetUniqueId("shape", (int)DCL.Models.CLASS_ID.AUDIO_CLIP, "1");

            DCLAudioClip PBRMaterialComponent = (DCLAudioClip)scene.SharedComponentCreate(JsonUtility.ToJson(new DCL.Models.SharedComponentCreateMessage
            {
                id = componentId,
                classId = (int)DCL.Models.CLASS_ID.AUDIO_CLIP
            }));

            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = componentId,
                json = componentJSON
            }));

            yield return new WaitForSeconds(0.01f);

            // 2. Check configured values
            Assert.IsTrue(PBRMaterialComponent.model.loop);
            Assert.IsFalse(PBRMaterialComponent.model.shouldTryToLoad);
            Assert.AreEqual(0.8f, PBRMaterialComponent.model.volume);

            // 3. Update component with missing values
            componentJSON = JsonUtility.ToJson(new DCLAudioClip.Model { });

            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = componentId,
                json = componentJSON
            }));

            // 4. Check defaulted values
            Assert.IsFalse(PBRMaterialComponent.model.loop);
            Assert.IsTrue(PBRMaterialComponent.model.shouldTryToLoad);
            Assert.AreEqual(1f, PBRMaterialComponent.model.volume);
        }
    }
}
