using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class AnimatorTests : IntegrationTestSuite_Legacy
    {
        [UnityTest]
        public IEnumerator CreateAnimationComponent()
        {
            var entity = TestHelpers.CreateSceneEntity(scene);

            Assert.IsTrue(entity.gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            TestHelpers.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
                JsonConvert.SerializeObject(new
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/CesiumMan/CesiumMan.glb"
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

            DCLAnimator animator =
                TestHelpers.EntityComponentCreate<DCLAnimator, DCLAnimator.Model>(scene, entity, animatorModel);

            LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(entity);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded == true);

            Assert.IsTrue(entity.gameObject.GetComponentInChildren<Animation>() != null,
                "'GLTFScene' child object with 'Animator' component should exist if the GLTF was loaded correctly.");
            Assert.IsTrue(entity.gameObject.GetComponentInChildren<DCLAnimator>() != null,
                "'GLTFScene' child object with 'DCLAnimator' component should exist if the GLTF was loaded correctly.");

            yield return animator.routine;

            animator = entity.gameObject.GetComponentInChildren<DCLAnimator>();

            Assert.IsTrue(animator.GetStateByString("clip01") != null, "dclAnimator.GetStateByString fail!");
            Assert.IsTrue(animator.GetModel().states[0].clip != null, "dclAnimator clipReference is null!");
        }

        [UnityTest]
        public IEnumerator DCLAnimatorResetAnimation()
        {
            GLTFShape gltfShape = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero,
                new LoadableShape.Model
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/Shark/shark_anim.gltf"
                });
            var entity = gltfShape.attachedEntities.First();

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

            DCLAnimator animator =
                TestHelpers.EntityComponentCreate<DCLAnimator, DCLAnimator.Model>(scene, entity, animatorModel);
            LoadWrapper gltfLoader = GLTFShape.GetLoaderForEntity(entity);
            yield return new WaitUntil(() => gltfLoader.alreadyLoaded);

            yield return animator.routine;

            animator.animComponent.cullingType = AnimationCullingType.AlwaysAnimate;

            yield return null;

            Animation animation = entity.gameObject.GetComponentInChildren<Animation>();
            foreach (AnimationState animState in animation)
            {
                Assert.AreNotEqual(0f, animState.time);
            }

            animatorModel.states[1].shouldReset = true;

            yield return TestHelpers.EntityComponentUpdate(animator, animatorModel);

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
            var gltfShape = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero,
                new LoadableShape.Model
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/Shark/shark_anim.gltf"
                });
            var entity = gltfShape.attachedEntities.First();

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

            DCLAnimator animator =
                TestHelpers.EntityComponentCreate<DCLAnimator, DCLAnimator.Model>(scene, entity, animatorModel);
            LoadWrapper gltfLoader = GLTFShape.GetLoaderForEntity(entity);
            yield return new WaitUntil(() => gltfLoader.alreadyLoaded);

            yield return animator.routine;

            animator.animComponent.cullingType = AnimationCullingType.AlwaysAnimate;

            yield return null;

            Animation animation = entity.gameObject.GetComponentInChildren<Animation>();

            foreach (AnimationState animState in animation)
            {
                Assert.AreNotEqual(0f, animState.time);
            }

            animatorModel.states[0].shouldReset = true;
            animatorModel.states[1].shouldReset = true;

            yield return TestHelpers.EntityComponentUpdate(animator, animatorModel);

            foreach (AnimationState animState in animation)
            {
                Assert.AreEqual(0f, animState.time);
            }
        }

        [UnityTest]
        public IEnumerator AnimationComponentMissingValuesGetDefaultedOnUpdate()
        {
            yield return TestHelpers.TestEntityComponentDefaultsOnUpdate<DCLAnimator.Model, DCLAnimator>(scene);
        }

        [UnityTest]
        public IEnumerator UpdateAnimationComponent()
        {
            var entity = TestHelpers.CreateSceneEntity(scene);

            Assert.IsTrue(entity.gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            TestHelpers.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
                JsonConvert.SerializeObject(new
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/CesiumMan/CesiumMan.glb"
                }));

            string clipName = "animation:0";
            DCLAnimator.Model animatorModel = new DCLAnimator.Model
            {
                states = new DCLAnimator.Model.DCLAnimationState[]
                {
                    new DCLAnimator.Model.DCLAnimationState
                    {
                        name = "clip01",
                        clip = clipName,
                        playing = true,
                        weight = 1,
                        speed = 1,
                        looping = false
                    }
                }
            };

            DCLAnimator animator = TestHelpers.EntityComponentCreate<DCLAnimator, DCLAnimator.Model>(scene, entity, animatorModel);

            LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(entity);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded == true);

            Assert.IsTrue(animator.animComponent.isPlaying);
            Assert.AreEqual(animator.animComponent.clip.name, clipName);
            Assert.IsFalse(animator.animComponent.clip.wrapMode == WrapMode.Loop);

            yield return null;

            // update component properties
            animatorModel.states[0].playing = false;
            animatorModel.states[0].looping = true;
            yield return TestHelpers.EntityComponentUpdate(animator, animatorModel);

            Assert.IsFalse(animator.animComponent.isPlaying);
            Assert.IsTrue(animator.animComponent.clip.wrapMode == WrapMode.Loop);
        }

        [UnityTest]
        [Explicit]
        [Category("Explicit")]
        public IEnumerator AnimationStartsAutomaticallyWithNoDCLAnimator()
        {
            // GLTFShape without DCLAnimator
            var entity = TestHelpers.CreateSceneEntity(scene);

            Assert.IsTrue(entity.gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            TestHelpers.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
                JsonConvert.SerializeObject(new
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/CesiumMan/CesiumMan.glb"
                }));

            LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(entity);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded == true);

            Animation animation = entity.meshRootGameObject.GetComponentInChildren<Animation>();

            Assert.IsTrue(animation != null);
            Assert.IsTrue(animation.isPlaying);

            // GLTFShape with DCLAnimator
            var entity2 = TestHelpers.CreateSceneEntity(scene);

            Assert.IsTrue(entity2.gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            TestHelpers.CreateAndSetShape(scene, entity2.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
                JsonConvert.SerializeObject(new
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/CesiumMan/CesiumMan.glb"
                }));

            string clipName = "animation:0";
            DCLAnimator.Model animatorModel = new DCLAnimator.Model
            {
                states = new DCLAnimator.Model.DCLAnimationState[]
                {
                    new DCLAnimator.Model.DCLAnimationState
                    {
                        name = "clip01",
                        clip = clipName,
                        playing = false,
                        weight = 1,
                        speed = 1,
                        looping = false
                    }
                }
            };

            DCLAnimator animator = TestHelpers.EntityComponentCreate<DCLAnimator, DCLAnimator.Model>(scene, entity, animatorModel);

            LoadWrapper gltfShape2 = GLTFShape.GetLoaderForEntity(entity);
            yield return new WaitUntil(() => gltfShape2.alreadyLoaded == true);

            Assert.IsTrue(animator.animComponent != null);
            Assert.AreEqual(animator.animComponent.clip.name, clipName);
            Assert.IsFalse(animator.animComponent.isPlaying);
        }

        [UnityTest]
        public IEnumerator NonSkeletalAnimationsSupport()
        {
            var entity = TestHelpers.CreateSceneEntity(scene);

            TestHelpers.SetEntityTransform(scene, entity, new Vector3(8, 2, 8), Quaternion.identity, Vector3.one);

            Assert.IsTrue(entity.gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null,
                "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

            TestHelpers.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
                JsonConvert.SerializeObject(new
                {
                    src = Utils.GetTestsAssetsPath() + "/GLB/non-skeletal-3-transformations.glb"
                }));

            string clipName = "All";
            DCLAnimator.Model animatorModel = new DCLAnimator.Model
            {
                states = new DCLAnimator.Model.DCLAnimationState[]
                {
                    new DCLAnimator.Model.DCLAnimationState
                    {
                        name = "clip01",
                        clip = clipName,
                        playing = false,
                        weight = 1,
                        speed = 1,
                        looping = false
                    }
                }
            };

            DCLAnimator animator = TestHelpers.EntityComponentCreate<DCLAnimator, DCLAnimator.Model>(scene, entity, animatorModel);

            LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(entity);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded == true);

            animator.animComponent.cullingType = AnimationCullingType.AlwaysAnimate;

            Assert.IsTrue(!animator.animComponent.isPlaying);
            Assert.AreEqual(animator.animComponent.clip.name, clipName);
            Assert.IsFalse(animator.animComponent.clip.wrapMode == WrapMode.Loop);

            Transform animatedGameObject = animator.animComponent.transform.GetChild(0);

            Vector3 originalScale = animatedGameObject.transform.localScale;
            Vector3 originalPos = animatedGameObject.transform.localPosition;
            Quaternion originalRot = animatedGameObject.transform.localRotation;

            // start animation
            animatorModel.states[0].playing = true;
            yield return TestHelpers.EntityComponentUpdate(animator, animatorModel);

            yield return new WaitForSeconds(0.1f);

            Assert.IsFalse(animatedGameObject.localScale == originalScale);
            Assert.IsFalse(animatedGameObject.localPosition == originalPos);
            Assert.IsFalse(animatedGameObject.localRotation == originalRot);
        }
    }
}