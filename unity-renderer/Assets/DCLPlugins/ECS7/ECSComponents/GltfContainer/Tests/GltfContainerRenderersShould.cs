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
    public class GltfContainerRenderersShould
    {
        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private ECS7TestEntity entity;
        private GltfContainerHandler handler;

        private IInternalECSComponent<InternalRenderers> renderersComponent;

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

            handler = new GltfContainerHandler(
                internalEcsComponents.onPointerColliderComponent,
                internalEcsComponents.physicColliderComponent,
                internalEcsComponents.customLayerColliderComponent,
                renderersComponent,
                internalEcsComponents.GltfContainerLoadingStateComponent,
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
        public IEnumerator SetRenderersCorrectly()
        {
            // test with several updates to make sure that changing gltf does not affect the expected result
            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer() { Src = "sharknado" });
            yield return handler.gltfLoader.Promise;

            IList<Renderer> renderers = handler.gameObject.GetComponentsInChildren<Renderer>();
            IECSReadOnlyComponentData<InternalRenderers> internalRenderers = renderersComponent.GetFor(scene, entity);

            for (int i = 0; i < renderers.Count; i++)
            {
                Assert.IsTrue(internalRenderers.model.renderers.Contains(renderers[i]));
            }

            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer() { Src = "palmtree" });
            yield return handler.gltfLoader.Promise;

            renderers = handler.gameObject.GetComponentsInChildren<Renderer>();
            internalRenderers = renderersComponent.GetFor(scene, entity);

            for (int i = 0; i < renderers.Count; i++)
            {
                Assert.IsTrue(internalRenderers.model.renderers.Contains(renderers[i]));
            }

            handler.OnComponentModelUpdated(scene, entity, new PBGltfContainer() { Src = "sharknado" });
            yield return handler.gltfLoader.Promise;

            renderers = handler.gameObject.GetComponentsInChildren<Renderer>();
            internalRenderers = renderersComponent.GetFor(scene, entity);

            for (int i = 0; i < renderers.Count; i++)
            {
                Assert.IsTrue(internalRenderers.model.renderers.Contains(renderers[i]));
            }
        }
    }
}
