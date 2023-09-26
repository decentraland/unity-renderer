using DCL;
using DCL.CRDT;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Helpers;
using ECSSystems.AnimationSystem;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using Environment = DCL.Environment;

namespace Tests.Systems.Animation
{
    public class AnimationSystemShould
    {
        private ECSComponent<PBGltfContainer> gltfContainerComponent;
        private ECSComponent<PBAnimator> animatorComponent;

        private Action systemUpdate;
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private ECS7TestEntity entity;

        [SetUp]
        protected void SetUp()
        {
            ServiceLocator serviceLocator = ServiceLocatorTestFactory.CreateMocked();
            serviceLocator.Register<IWebRequestController>(WebRequestController.Create);
            Environment.Setup(serviceLocator);

            var componentsFactory = new ECSComponentsFactory();
            var componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            var internalComponents = new InternalECSComponents(componentsManager, componentsFactory, executors);
            var componentGroups = new ComponentGroups(componentsManager);

            new GltfContainerRegister(0, componentsFactory, Substitute.For<IECSComponentWriter>(), internalComponents);
            new AnimatorRegister(1, componentsFactory, Substitute.For<IECSComponentWriter>(), internalComponents.AnimationPlayer);

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);
            scene = testUtils.CreateScene(666);
            entity = scene.CreateEntity(667);

            gltfContainerComponent = (ECSComponent<PBGltfContainer>)componentsManager.GetOrCreateComponent(0, scene, entity);
            animatorComponent = (ECSComponent<PBAnimator>)componentsManager.GetOrCreateComponent(1, scene, entity);

            scene.contentProvider.baseUrl = $"{TestAssetsUtils.GetPath()}/GLB/";
            scene.contentProvider.fileToHash.Add("palmtree", "PalmTree_01.glb");
            scene.contentProvider.fileToHash.Add("sharknado", "Shark/shark_anim.gltf");

            var system = new AnimationSystem(componentGroups.AnimationGroup, internalComponents.Animation);

            systemUpdate = () =>
            {
                internalComponents.MarkDirtyComponentsUpdate();
                system.Update();
                internalComponents.ResetDirtyComponentsUpdate();
            };
        }

        [TearDown]
        protected void TearDown()
        {
            testUtils.Dispose();
            AssetPromiseKeeper_GLTFast_Instance.i.Cleanup();
            PoolManager.i.Dispose();
        }

        [UnityTest]
        public IEnumerator SetAnimation()
        {
            const string CLIP1 = "shark_skeleton_swim";
            const string CLIP2 = "shark_skeleton_bite";

            var animState = new PBAnimationState()
            {
                Clip = CLIP1,
                Loop = false,
                Playing = true,
                Speed = 2,
                Weight = 1,
                ShouldReset = false
            };

            gltfContainerComponent.SetModel(scene, entity, new PBGltfContainer() { Src = "sharknado" });
            animatorComponent.SetModel(scene, entity, new PBAnimator() { States = { animState } });

            var handler = (GltfContainerHandler)gltfContainerComponent.Get(scene, entity.entityId).Value.handler;
            yield return handler.gltfLoader.Promise;

            systemUpdate();

            UnityEngine.Animation animation = handler.gameObject.GetComponentInChildren<UnityEngine.Animation>();
            Assert.AreEqual(WrapMode.Loop, animation[CLIP2].wrapMode);
            Assert.AreEqual(AnimationBlendMode.Blend, animation[CLIP2].blendMode);

            Assert.AreEqual(WrapMode.Default, animation[CLIP1].wrapMode);
            Assert.AreEqual(WrapMode.Default, animation[CLIP1].clip.wrapMode);
            Assert.AreEqual(animState.Weight, animation[CLIP1].weight);
            Assert.AreEqual(animState.Speed, animation[CLIP1].speed);
            Assert.AreEqual(animState.Playing, animation[CLIP1].enabled);
            Assert.IsTrue(animation.IsPlaying(CLIP1));
        }
    }
}
