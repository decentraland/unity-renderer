using DCL;
using DCL.CRDT;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.TestTools;
using Environment = DCL.Environment;

namespace Tests
{
    public class GltfContainerComponentLifeCycleShould
    {
        private const int GLTF_COMPONENT_ID = 666;
        private const int SCENE_ID = 666;
        private const int ENTITY_ID = 666;

        private ECS7TestUtilsScenesAndEntities testUtils;
        private ECS7TestScene scene;
        private IECSComponentWriter writer;
        private Action updateSystems;

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

            updateSystems = () =>
            {
                internalEcsComponents.MarkDirtyComponentsUpdate();
                internalEcsComponents.ResetDirtyComponentsUpdate();
            };

            testUtils = new ECS7TestUtilsScenesAndEntities(componentsManager, executors);
            scene = testUtils.CreateScene(SCENE_ID);

            writer = MockWriter(executors);

            var gltfContainerRegister = new GltfContainerRegister(GLTF_COMPONENT_ID, componentFactory, writer, internalEcsComponents);

            scene.contentProvider.baseUrl = $"{TestAssetsUtils.GetPath()}/GLB/";
            scene.contentProvider.fileToHash.Add("palmtree", "PalmTree_01.glb");
            scene.contentProvider.fileToHash.Add("sharknado", "Shark/shark_anim.gltf");
        }

        [TearDown]
        public void TearDown()
        {
            testUtils.Dispose();
            AssetPromiseKeeper_GLTFast_Instance.i.Cleanup();
            PoolManager.i.Dispose();
            DataStore.Clear();
        }

        [UnityTest]
        public IEnumerator CreateCorrectly()
        {
            writer.PutComponent(SCENE_ID, ENTITY_ID, GLTF_COMPONENT_ID, new PBGltfContainer()
            {
                Src = "palmtree"
            });

            yield return null;
            yield return null;
            updateSystems();

            Assert.AreEqual(1, scene.entities.Count);
        }

        [UnityTest]
        public IEnumerator RemoveCorrectly()
        {
            writer.PutComponent(SCENE_ID, ENTITY_ID, GLTF_COMPONENT_ID, new PBGltfContainer()
            {
                Src = "palmtree"
            });

            yield return null;
            yield return null;
            updateSystems();

            writer.RemoveComponent(SCENE_ID, ENTITY_ID, GLTF_COMPONENT_ID);
            yield return null;
            updateSystems();

            Assert.AreEqual(0, scene.entities.Count);
        }

        [UnityTest]
        public IEnumerator UpdateCorrectly()
        {
            writer.PutComponent(SCENE_ID, ENTITY_ID, GLTF_COMPONENT_ID, new PBGltfContainer()
            {
                Src = "palmtree"
            });

            yield return null;
            yield return null;
            updateSystems();

            writer.PutComponent(SCENE_ID, ENTITY_ID, GLTF_COMPONENT_ID, new PBGltfContainer()
            {
                Src = "sharknado"
            });

            yield return null;
            yield return null;
            updateSystems();

            Assert.AreEqual(1, scene.entities.Count);
        }

        private static IECSComponentWriter MockWriter(IReadOnlyDictionary<int, ICRDTExecutor> executors)
        {
            Func<PBGltfContainer, byte[]> gltfSerializer = null;
            IECSComponentWriter writer = Substitute.For<IECSComponentWriter>();

            writer.WhenForAnyArgs(x => x.AddOrReplaceComponentSerializer(Arg.Any<int>(), Arg.Any<Func<PBGltfContainer, byte[]>>()))
                  .Do(info =>
                   {
                       gltfSerializer = info.ArgAt<Func<PBGltfContainer, byte[]>>(1);
                   });

            writer.WhenForAnyArgs(x => x.PutComponent(Arg.Any<int>(), Arg.Any<long>(), Arg.Any<int>(), Arg.Any<PBGltfContainer>()))
                  .Do(info =>
                   {
                       executors[info.ArgAt<int>(0)]
                          .ExecuteWithoutStoringState(
                               info.ArgAt<long>(1),
                               info.ArgAt<int>(2),
                               gltfSerializer(info.ArgAt<PBGltfContainer>(3)));
                   });

            writer.WhenForAnyArgs(x => x.RemoveComponent(Arg.Any<int>(), Arg.Any<long>(), Arg.Any<int>()))
                  .Do(info =>
                   {
                       executors[info.ArgAt<int>(0)]
                          .ExecuteWithoutStoringState(
                               info.ArgAt<long>(1),
                               info.ArgAt<int>(2),
                               null);
                   });

            return writer;
        }
    }
}
