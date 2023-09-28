using DCL;
using DCL.CRDT;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Helpers;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.Components.GltfContainer
{
    public class GltfContainerAnimationShould
    {
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private ECS7TestEntity entity;
        private GltfContainerHandler handler;

        private IInternalECSComponent<InternalAnimation> animationComponent;

        [SetUp]
        public void SetUp()
        {
            ServiceLocator serviceLocator = ServiceLocatorTestFactory.CreateMocked();
            serviceLocator.Register<IWebRequestController>(WebRequestController.Create);
            Environment.Setup(serviceLocator);

            ECSComponentsFactory componentFactory = new ECSComponentsFactory();
            ECSComponentsManager componentsManager = new ECSComponentsManager(componentFactory.componentBuilders);
            var executors = new Dictionary<int, ICRDTExecutor>();
            InternalECSComponents internalEcsComponents = new InternalECSComponents(componentsManager, componentFactory, executors);

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);
            scene = testUtils.CreateScene(666);
            entity = scene.CreateEntity(23423);

            scene.contentProvider.baseUrl = $"{TestAssetsUtils.GetPath()}/GLB/";
            scene.contentProvider.fileToHash.Add("palmtree", "PalmTree_01.glb");
            scene.contentProvider.fileToHash.Add("sharknado", "Shark/shark_anim.gltf");

            animationComponent = internalEcsComponents.Animation;

            handler = new GltfContainerHandler(
                internalEcsComponents.onPointerColliderComponent,
                internalEcsComponents.physicColliderComponent,
                internalEcsComponents.customLayerColliderComponent,
                internalEcsComponents.renderersComponent,
                internalEcsComponents.GltfContainerLoadingStateComponent,
                animationComponent,
                new DataStore_ECS7(),
                new DataStore_FeatureFlag());

            handler.OnComponentCreated(scene, entity);
        }

        [TearDown]
        public void TearDown()
        {
            handler.OnComponentRemoved(scene, entity);
            testUtils.Dispose();
            AssetPromiseKeeper_GLTFast_Instance.i.Cleanup();
            PoolManager.i.Dispose();
        }

        [UnityTest]
        public IEnumerator CreateInternalComponent()
        {
            // add gltf with animation
            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer() { Src = "sharknado" });
            yield return handler.gltfLoader.Promise;

            Animation animation = handler.gameObject.GetComponentInChildren<Animation>();
            ECSComponentData<InternalAnimation> internalAnimation = animationComponent.GetFor(scene, entity).Value;

            Assert.IsTrue(animation);
            Assert.AreEqual(animation, internalAnimation.model.Animation);
            Assert.IsFalse(internalAnimation.model.IsInitialized);

            // add gltf without animation
            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer() { Src = "palmtree" });
            yield return handler.gltfLoader.Promise;

            Assert.IsFalse(animationComponent.GetFor(scene, entity).HasValue);
        }

        [UnityTest]
        public IEnumerator RemoveInternalComponent()
        {
            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer() { Src = "sharknado" });
            yield return handler.gltfLoader.Promise;

            handler.OnComponentRemoved(scene, entity);
            Assert.IsFalse(animationComponent.GetFor(scene, entity).HasValue);
        }
    }
}
