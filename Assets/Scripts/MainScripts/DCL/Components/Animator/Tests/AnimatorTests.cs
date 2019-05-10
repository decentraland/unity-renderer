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
    public class AnimatorTests : TestsBase
    {
        // TODO: Find a way to run this test on Unity Cloud Build, even though it passes locally, it fails on timeout in Unity Cloud Build
        [UnityTest]
        public IEnumerator CreateAnimationComponent()
        {
            yield return InitScene();

            DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);

            Assert.IsTrue(entity.gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null, "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            TestHelpers.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/CesiumMan/CesiumMan.glb"
            }));

            DCLAnimator.Model animatorModel = new DCLAnimator.Model
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
            };

            DCLAnimator animator = TestHelpers.EntityComponentCreate<DCLAnimator, DCLAnimator.Model>(scene, entity, animatorModel);

            GLTFLoader gltfShape = entity.gameObject.GetComponentInChildren<GLTFLoader>();
            yield return new WaitUntil(() => gltfShape.alreadyLoaded == true);

            Assert.IsTrue(entity.gameObject.GetComponentInChildren<Animation>() != null, "'GLTFScene' child object with 'Animator' component should exist if the GLTF was loaded correctly.");
            Assert.IsTrue(entity.gameObject.GetComponentInChildren<DCLAnimator>() != null, "'GLTFScene' child object with 'DCLAnimator' component should exist if the GLTF was loaded correctly.");

            yield return animator.routine;

            animator = entity.gameObject.GetComponentInChildren<DCLAnimator>();

            Assert.IsTrue(animator.GetStateByString("clip01") != null, "dclAnimator.GetStateByString fail!");
            Assert.IsTrue(animator.model.states[0].clipReference != null, "dclAnimator clipReference is null!");
        }

        [UnityTest]
        public IEnumerator AnimationComponentMissingValuesGetDefaultedOnUpdate()
        {
            yield return InitScene();
            yield return TestHelpers.TestEntityComponentDefaultsOnUpdate<DCLAnimator.Model, DCLAnimator>(scene);
        }
    }
}
