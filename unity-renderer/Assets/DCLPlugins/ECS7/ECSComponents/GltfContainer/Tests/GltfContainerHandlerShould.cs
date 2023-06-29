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

namespace Tests
{
    public class GltfContainerHandlerShould
    {
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private ECS7TestEntity entity;
        private GltfContainerHandler handler;

        private IInternalECSComponent<InternalColliders> pointerColliderComponent;
        private IInternalECSComponent<InternalColliders> physicColliderComponent;
        private IInternalECSComponent<InternalColliders> customLayerColliderComponent;
        private IInternalECSComponent<InternalRenderers> renderersComponent;
        private IInternalECSComponent<InternalGltfContainerLoadingState> gltfContainerLoadingStateComponent;
        private DataStore_ECS7 dataStoreEcs7;

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

            renderersComponent = internalEcsComponents.renderersComponent;
            pointerColliderComponent = internalEcsComponents.onPointerColliderComponent;
            physicColliderComponent = internalEcsComponents.physicColliderComponent;
            customLayerColliderComponent = internalEcsComponents.customLayerColliderComponent;
            gltfContainerLoadingStateComponent = internalEcsComponents.GltfContainerLoadingStateComponent;
            dataStoreEcs7 = new DataStore_ECS7();

            handler = new GltfContainerHandler(pointerColliderComponent,
                physicColliderComponent,
                customLayerColliderComponent,
                renderersComponent,
                gltfContainerLoadingStateComponent,
                dataStoreEcs7,
                new DataStore_FeatureFlag());

            handler.OnComponentCreated(scene, entity);
        }

        [TearDown]
        public void TearDown()
        {
            AssetPromiseKeeper_GLTFast_Instance.i.Cleanup();
            testUtils.Dispose();
            PoolManager.i.Dispose();
        }

        [UnityTest]
        public IEnumerator UpdateCorrectly()
        {
            PBGltfContainer model = new PBGltfContainer()
            {
                Src = "palmtree"
            };

            handler.OnComponentModelUpdated(scene, entity, model);
            yield return handler.gltfLoader.Promise;

            var palmTree = FindDeepChild(handler.gameObject.transform, "PalmTree_01");
            Assert.IsNotNull(palmTree);

            model = new PBGltfContainer()
            {
                Src = "sharknado"
            };

            handler.OnComponentModelUpdated(scene, entity, model);
            yield return handler.gltfLoader.Promise;

            var sharkSkeleton = FindDeepChild(handler.gameObject.transform, "shark_skeleton");
            Assert.IsNotNull(sharkSkeleton);
        }

        private Transform FindDeepChild(Transform parent, string name)
        {
            if (parent.gameObject.name == name)
                return parent;

            foreach (Transform child in parent)
                return FindDeepChild(child, name);

            return null;
        }

        [UnityTest]
        public IEnumerator SetAssetAsLoaded()
        {
            Assert.IsFalse(dataStoreEcs7.pendingSceneResources.ContainsKey(scene.sceneData.sceneNumber));
            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer() { Src = "palmtree" });
            Assert.AreEqual(1, dataStoreEcs7.pendingSceneResources[scene.sceneData.sceneNumber].GetRefCount("palmtree"));
            yield return handler.gltfLoader.Promise;
            Assert.AreEqual(0, dataStoreEcs7.pendingSceneResources[scene.sceneData.sceneNumber].Count());

            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer() { Src = "sharknado" });
            Assert.AreEqual(1, dataStoreEcs7.pendingSceneResources[scene.sceneData.sceneNumber].GetRefCount("sharknado"));
            yield return handler.gltfLoader.Promise;
            Assert.AreEqual(0, dataStoreEcs7.pendingSceneResources[scene.sceneData.sceneNumber].Count());
        }

        [Test]
        public void RemoveAssetAsPendingIfComponentRemovedWhileLoading()
        {
            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer() { Src = "palmtree" });
            Assert.AreEqual(1, dataStoreEcs7.pendingSceneResources[scene.sceneData.sceneNumber].GetRefCount("palmtree"));
            handler.OnComponentRemoved(scene, entity);
            Assert.AreEqual(0, dataStoreEcs7.pendingSceneResources[scene.sceneData.sceneNumber].Count());
        }

        [UnityTest]
        public IEnumerator RemoveComponentCorrectly()
        {
            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer() { Src = "palmtree" });
            yield return handler.gltfLoader.Promise;
            handler.OnComponentRemoved(scene, entity);
            yield return null;

            Assert.IsFalse(handler.gameObject);
            Assert.IsFalse(handler.gltfLoader.IsFinished);
            Assert.IsNull(renderersComponent.GetFor(scene, entity));
            Assert.IsNull(physicColliderComponent.GetFor(scene, entity));
            Assert.IsNull(pointerColliderComponent.GetFor(scene, entity));
            Assert.IsNull(customLayerColliderComponent.GetFor(scene, entity));
        }

        [Test]
        public void PutGltfContainerLoadingStateAsLoading()
        {
            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer() { Src = "palmtree" });
            var model = gltfContainerLoadingStateComponent.GetFor(scene, entity).Value.model;
            Assert.AreEqual(LoadingState.Loading, model.LoadingState);
            Assert.IsFalse(model.GltfContainerRemoved);
        }

        [UnityTest]
        public IEnumerator PutGltfContainerLoadingStateAsFinished()
        {
            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer() { Src = "palmtree" });
            yield return handler.gltfLoader.Promise;

            var model = gltfContainerLoadingStateComponent.GetFor(scene, entity).Value.model;
            Assert.AreEqual(LoadingState.Finished, model.LoadingState);
            Assert.IsFalse(model.GltfContainerRemoved);
        }

        [UnityTest]
        public IEnumerator PutGltfContainerLoadingStateAsFinishedWithError()
        {
            var ignoreFailingMessages = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;
            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer() { Src = "non-existing-gltf" });
            yield return handler.gltfLoader.Promise;

            LogAssert.ignoreFailingMessages = ignoreFailingMessages;
            var model = gltfContainerLoadingStateComponent.GetFor(scene, entity).Value.model;
            Assert.AreEqual(LoadingState.FinishedWithError, model.LoadingState);
            Assert.IsFalse(model.GltfContainerRemoved);
        }

        [Test]
        public void RemoveGltfContainerLoadingStateWhenGltfContainerRemoved()
        {
            handler.OnComponentRemoved(scene, entity);
            var model = gltfContainerLoadingStateComponent.GetFor(scene, entity).Value.model;
            Assert.AreEqual(LoadingState.Unknown, model.LoadingState);
            Assert.IsTrue(model.GltfContainerRemoved);
        }
    }
}
