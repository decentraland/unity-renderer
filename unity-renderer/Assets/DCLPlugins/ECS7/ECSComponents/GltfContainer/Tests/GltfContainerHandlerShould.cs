using DCL;
using DCL.Configuration;
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

            var keepEntityAliveComponent = new InternalECSComponent<InternalComponent>(
                0, componentsManager, componentFactory, null,
                new KeyValueSet<ComponentIdentifier, ComponentWriteData>(),
                executors);

            keepEntityAliveComponent.PutFor(scene, entity, new InternalComponent());

            scene.contentProvider.baseUrl = $"{TestAssetsUtils.GetPath()}/GLB/";
            scene.contentProvider.fileToHash.Add("palmtree", "PalmTree_01.glb");
            scene.contentProvider.fileToHash.Add("sharknado", "Shark/shark_anim.gltf");

            renderersComponent = internalEcsComponents.renderersComponent;
            pointerColliderComponent = internalEcsComponents.onPointerColliderComponent;
            physicColliderComponent = internalEcsComponents.physicColliderComponent;
            gltfContainerLoadingStateComponent = internalEcsComponents.GltfContainerLoadingStateComponent;
            dataStoreEcs7 = new DataStore_ECS7();

            handler = new GltfContainerHandler(pointerColliderComponent,
                physicColliderComponent,
                renderersComponent,
                gltfContainerLoadingStateComponent,
                dataStoreEcs7,
                new DataStore_FeatureFlag());

            handler.OnComponentCreated(scene, entity);
        }

        [TearDown]
        public void TearDown()
        {
            handler.OnComponentRemoved(scene, entity);
            testUtils.Dispose();
            AssetPromiseKeeper_GLTF.i.Cleanup();
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
            yield return new WaitUntil(() => handler.gltfLoader.isFinished);

            // make sure gltf is loaded properly by checking an specific gameobject name in its hierarchy
            Assert.AreEqual("PalmTree_01", handler.gameObject.transform.GetChild(0).GetChild(0).name);

            model = new PBGltfContainer()
            {
                Src = "sharknado"
            };

            handler.OnComponentModelUpdated(scene, entity, model);
            yield return new WaitUntil(() => handler.gltfLoader.isFinished);

            // make sure gltf is loaded properly by checking an specific gameobject name in its hierarchy
            Assert.AreEqual("shark_skeleton", handler.gameObject.transform.GetChild(0).GetChild(0).GetChild(0).name);
        }

        [UnityTest]
        public IEnumerator SetCollidersCorrectly()
        {
            // test with several updates to make sure that changing gltf does not affect the expected result
            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer() { Src = "palmtree" });
            yield return new WaitUntil(() => handler.gltfLoader.isFinished);

            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer() { Src = "sharknado" });
            yield return new WaitUntil(() => handler.gltfLoader.isFinished);

            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer() { Src = "palmtree" });
            yield return new WaitUntil(() => handler.gltfLoader.isFinished);

            Collider physicCollider = handler.gameObject.transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Collider>();
            Collider pointerCollider = handler.gameObject.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<Collider>();

            Assert.IsTrue(physicCollider);
            Assert.IsTrue(pointerCollider);

            Assert.AreEqual(physicCollider.gameObject.layer, PhysicsLayers.characterOnlyLayer);
            Assert.AreEqual(pointerCollider.gameObject.layer, PhysicsLayers.onPointerEventLayer);

            var physicColliderComponentData = physicColliderComponent.GetFor(scene, entity);
            var pointerColliderComponentData = pointerColliderComponent.GetFor(scene, entity);

            Assert.AreEqual(physicCollider, physicColliderComponentData.model.colliders.Pairs[0].key);
            Assert.AreEqual(pointerCollider, pointerColliderComponentData.model.colliders.Pairs[0].key);
        }

        [UnityTest]
        public IEnumerator SetRenderersCorrectly()
        {
            // test with several updates to make sure that changing gltf does not affect the expected result
            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer() { Src = "sharknado" });
            yield return new WaitUntil(() => handler.gltfLoader.isFinished);

            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer() { Src = "palmtree" });
            yield return new WaitUntil(() => handler.gltfLoader.isFinished);

            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer() { Src = "sharknado" });
            yield return new WaitUntil(() => handler.gltfLoader.isFinished);

            IList<Renderer> renderers = handler.gameObject.GetComponentsInChildren<Renderer>();
            IECSReadOnlyComponentData<InternalRenderers> internalRenderers = renderersComponent.GetFor(scene, entity);

            for (int i = 0; i < renderers.Count; i++)
            {
                Assert.IsTrue(internalRenderers.model.renderers.Contains(renderers[i]));
            }
        }

        [UnityTest]
        public IEnumerator SetAssetAsLoaded()
        {
            Assert.IsFalse(dataStoreEcs7.pendingSceneResources.ContainsKey(scene.sceneData.sceneNumber));
            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer() { Src = "palmtree" });
            Assert.AreEqual(1, dataStoreEcs7.pendingSceneResources[scene.sceneData.sceneNumber].GetRefCount("palmtree"));
            yield return new WaitUntil(() => handler.gltfLoader.isFinished);
            Assert.AreEqual(0, dataStoreEcs7.pendingSceneResources[scene.sceneData.sceneNumber].Count());

            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer() { Src = "sharknado" });
            Assert.AreEqual(1, dataStoreEcs7.pendingSceneResources[scene.sceneData.sceneNumber].GetRefCount("sharknado"));
            yield return new WaitUntil(() => handler.gltfLoader.isFinished);
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
            yield return new WaitUntil(() => handler.gltfLoader.isFinished);
            handler.OnComponentRemoved(scene, entity);
            yield return null;

            Assert.IsFalse(handler.gameObject);
            Assert.IsFalse(handler.gltfLoader.isFinished);
            Assert.IsNull(renderersComponent.GetFor(scene, entity));
            Assert.IsNull(physicColliderComponent.GetFor(scene, entity));
            Assert.IsNull(pointerColliderComponent.GetFor(scene, entity));
        }

        [Test]
        public void PutGltfContainerLoadingStateAsLoading()
        {
            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer() { Src = "palmtree" });
            var model = gltfContainerLoadingStateComponent.GetFor(scene, entity).model;
            Assert.AreEqual(LoadingState.Loading, model.LoadingState);
            Assert.IsFalse(model.GltfContainerRemoved);
        }

        [UnityTest]
        public IEnumerator PutGltfContainerLoadingStateAsFinished()
        {
            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer() { Src = "palmtree" });
            yield return new WaitUntil(() => handler.gltfLoader.isFinished);
            var model = gltfContainerLoadingStateComponent.GetFor(scene, entity).model;
            Assert.AreEqual(LoadingState.Finished, model.LoadingState);
            Assert.IsFalse(model.GltfContainerRemoved);
        }

        [UnityTest]
        public IEnumerator PutGltfContainerLoadingStateAsFinishedWithError()
        {
            var ignoreFailingMessages = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;
            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer() { Src = "non-existing-gltf" });
            yield return new WaitUntil(() => handler.gltfLoader.isFinished);

            LogAssert.ignoreFailingMessages = ignoreFailingMessages;
            var model = gltfContainerLoadingStateComponent.GetFor(scene, entity).model;
            Assert.AreEqual(LoadingState.FinishedWithError, model.LoadingState);
            Assert.IsFalse(model.GltfContainerRemoved);
        }

        [Test]
        public void RemoveGltfContainerLoadingStateWhenGltfContainerRemoved()
        {
            handler.OnComponentRemoved(scene, entity);
            var model = gltfContainerLoadingStateComponent.GetFor(scene, entity).model;
            Assert.AreEqual(LoadingState.Unknown, model.LoadingState);
            Assert.IsTrue(model.GltfContainerRemoved);
        }
    }
}
