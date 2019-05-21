using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests
{
    public class AnimatorTests : TestsBase
    {
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
            Assert.IsTrue(animator.model.states[0].clip != null, "dclAnimator clipReference is null!");
        }

        [UnityTest]
        public IEnumerator DCLAnimatorResetAnimation()
        {
            yield return InitScene();
            DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);

            TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, new BaseLoadableShape<GLTFLoader>.Model
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/Shark/shark_anim.gltf"
            });

            DCLAnimator.Model animatorModel = new DCLAnimator.Model
            {
                states = new[]
                {
                    new DCLAnimator.Model.DCLAnimationState
                    {
                        name = "Bite",
                        clip = "shark_skeleton_bite",
                        playing = true,
                        weight = 1,
                        speed = 1
                    },
                    new DCLAnimator.Model.DCLAnimationState
                    {
                        name = "Swim",
                        clip = "shark_skeleton_swim",
                        playing = true,
                        weight = 1,
                        speed = 1
                    }
                }
            };

            DCLAnimator animator = TestHelpers.EntityComponentCreate<DCLAnimator, DCLAnimator.Model>(scene, entity, animatorModel);
            GLTFLoader gltfShape = entity.gameObject.GetComponentInChildren<GLTFLoader>();
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            yield return animator.routine;

            yield return new WaitForSeconds(1.5f);

            Animation animation = entity.gameObject.GetComponentInChildren<Animation>();
            foreach (AnimationState animState in animation)
            {
                Assert.AreNotEqual(0f, animState.time);
            }

            animatorModel.states[1].shouldReset = true;
            yield return animator.UpdateComponent(JsonUtility.ToJson(animatorModel));

            animator.ResetAnimation(animator.GetStateByString("Swim"));
            foreach (AnimationState animState in animation)
            {
                if (animator.GetStateByString("Swim").clipReference.name == animState.clip.name)
                {
                    Assert.AreEqual(0f, animState.time);
                }
                else
                {
                    Assert.AreNotEqual(0f, animState.time);
                }
            }
        }

        [UnityTest]
        public IEnumerator DCLAnimatorResetAllAnimations()
        {
            yield return InitScene();
            DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);

            TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, new BaseLoadableShape<GLTFLoader>.Model
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/Shark/shark_anim.gltf"
            });

            DCLAnimator.Model animatorModel = new DCLAnimator.Model
            {
                states = new[]
                {
                    new DCLAnimator.Model.DCLAnimationState
                    {
                        name = "Bite",
                        clip = "shark_skeleton_bite",
                        playing = true,
                        weight = 1,
                        speed = 1
                    },
                    new DCLAnimator.Model.DCLAnimationState
                    {
                        name = "Swim",
                        clip = "shark_skeleton_swim",
                        playing = true,
                        weight = 1,
                        speed = 1
                    }
                }
            };

            DCLAnimator animator = TestHelpers.EntityComponentCreate<DCLAnimator, DCLAnimator.Model>(scene, entity, animatorModel);
            GLTFLoader gltfShape = entity.gameObject.GetComponentInChildren<GLTFLoader>();
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            yield return animator.routine;

            yield return new WaitForSeconds(1.5f);

            Animation animation = entity.gameObject.GetComponentInChildren<Animation>();
            foreach (AnimationState animState in animation)
            {
                Assert.AreNotEqual(0f, animState.time);
            }

            animatorModel.states[0].shouldReset = true;
            animatorModel.states[1].shouldReset = true;
            yield return animator.UpdateComponent(JsonUtility.ToJson(animatorModel));
            foreach (AnimationState animState in animation)
            {
                Assert.AreEqual(0f, animState.time);
            }
        }

        [UnityTest]
        public IEnumerator AnimationComponentMissingValuesGetDefaultedOnUpdate()
        {
            yield return InitScene();
            yield return TestHelpers.TestEntityComponentDefaultsOnUpdate<DCLAnimator.Model, DCLAnimator>(scene);
        }
    }
}
