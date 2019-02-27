using System.Collections;
using System.Collections.Generic;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Newtonsoft.Json;
using UnityGLTF;

namespace Tests
{
    public class AnimatorTests
    {
        // TODO: Find a way to run this test on Unity Cloud Build, even though it passes locally, it fails on timeout in Unity Cloud Build
        [UnityTest]
        [Explicit("This test fails in cloud build")]
        public IEnumerator CreateAnimationComponent()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForEndOfFrame();

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForEndOfFrame();

            string entityId = "1";
            scene.CreateEntity(JsonUtility.ToJson(new DCL.Models.CreateEntityMessage
            {
                id = entityId
            }));

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null, "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/CesiumMan/CesiumMan.glb"
            }));

            string animJson = JsonConvert.SerializeObject(new DCLAnimator.Model
            {
                states = new DCLAnimator.Model.DCLAnimationState[]
                {
                    new DCLAnimator.Model.DCLAnimationState
                    {
                        name = "clip01",
                        clip = "animation:0",
                        playing = true,
                        weight = 1,
                        speed = 1
                    }
                }
            });

            scene.EntityComponentCreate(JsonUtility.ToJson(new DCL.Models.EntityComponentCreateMessage
            {
                entityId = entityId,
                name = "animation",
                classId = (int)DCL.Models.CLASS_ID_COMPONENT.ANIMATOR,
                json = animJson
            }));


            GLTFLoader gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<GLTFLoader>();

            yield return new WaitUntil(() => gltfShape.alreadyLoaded == true);

            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponentInChildren<Animator>() != null, "'GLTFScene' child object with 'Animator' component should exist if the GLTF was loaded correctly.");
            Assert.IsTrue(scene.entities[entityId].gameObject.GetComponentInChildren<DCLAnimator>() != null, "'GLTFScene' child object with 'DCLAnimator' component should exist if the GLTF was loaded correctly.");

            yield return new WaitForSeconds(0.5f);
            DCLAnimator dclAnimator = scene.entities[entityId].gameObject.GetComponentInChildren<DCLAnimator>();

            Assert.IsTrue(dclAnimator.GetStateByString("clip01") != null, "dclAnimator.GetStateByString fail!");
            Assert.IsTrue(dclAnimator.model.states[0].clipReference != null, "dclAnimator clipReference is null!");
        }

        [UnityTest]
        public IEnumerator AnimationComponentMissingValuesGetDefaultedOnUpdate()
        {
            var sceneController = TestHelpers.InitializeSceneController();

            yield return new WaitForEndOfFrame();

            var sceneData = new LoadParcelScenesMessage.UnityParcelScene();
            var scene = sceneController.CreateTestScene(sceneData);

            yield return new WaitForEndOfFrame();

            string entityId = "1";
            TestHelpers.CreateSceneEntity(scene, entityId);

            TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/CesiumMan/CesiumMan.glb"
            }));

            // 1. Create component with non-default configs
            string componentJSON = JsonUtility.ToJson(new DCLAnimator.Model
            {
                states = new DCLAnimator.Model.DCLAnimationState[]
                {
                    new DCLAnimator.Model.DCLAnimationState
                    {
                        name = "clip01",
                        clip = "animation:0",
                        playing = true,
                        weight = 0.7f,
                        speed = 0.1f
                    }
                }
            });

            DCLAnimator animatorComponent = (DCLAnimator)scene.EntityComponentCreate(JsonUtility.ToJson(new DCL.Models.EntityComponentCreateMessage
            {
                entityId = entityId,
                name = "animation",
                classId = (int)DCL.Models.CLASS_ID_COMPONENT.ANIMATOR,
                json = componentJSON
            }));

            GLTFLoader gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<GLTFLoader>();

            yield return new WaitUntil(() => gltfShape.alreadyLoaded == true);

            // 2. Check configured values
            Assert.AreEqual(0.7f, animatorComponent.model.states[0].weight);
            Assert.AreEqual(0.1f, animatorComponent.model.states[0].speed);

            // 3. Update component with missing values
            componentJSON = JsonUtility.ToJson(new DCLAnimator.Model
            {
                states = new DCLAnimator.Model.DCLAnimationState[]
                {
                    new DCLAnimator.Model.DCLAnimationState
                    {
                        name = "clip02",
                        clip = "animation:0",
                        playing = true
                    }
                }
            });

            scene.EntityComponentUpdate(scene.entities[entityId], CLASS_ID_COMPONENT.ANIMATOR, componentJSON);

            // 4. Check changed values
            Assert.AreEqual("clip02", animatorComponent.model.states[0].name);

            // 5. Check defaulted values
            Assert.AreEqual(1f, animatorComponent.model.states[0].weight);
            Assert.AreEqual(1f, animatorComponent.model.states[0].speed);
        }
    }
}
