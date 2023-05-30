using DCL.Controllers;
using DCL.CRDT;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using RPC.Context;
using System.Collections.Generic;

namespace DCL.ECS7
{
    public class ECS7Plugin : IPlugin
    {
        private readonly ComponentCrdtWriteSystem crdtWriteSystem;
        private readonly IECSComponentWriter componentWriter;
        private readonly ECS7ComponentsComposer componentsComposer;
        private readonly ECSSystemsController systemsController;
        private readonly ECSComponentsFactory componentsFactory;
        private readonly InternalECSComponents internalEcsComponents;
        private readonly CrdtExecutorsManager crdtExecutorsManager;
        private readonly Dictionary<int, IParcelScene> sceneNumberMapping;
        internal readonly ECSComponentsManager componentsManager;
        private readonly BaseList<IParcelScene> loadedScenes;
        private readonly ISceneController sceneController;

        public ECS7Plugin()
        {
            DataStore.i.ecs7.isEcs7Enabled = true;
            loadedScenes = DataStore.i.ecs7.scenes;
            CRDTServiceContext crdtContext = DataStore.i.rpc.context.crdt;

            sceneController = Environment.i.world.sceneController;
            Dictionary<int, ICRDTExecutor> crdtExecutors = new Dictionary<int, ICRDTExecutor>(10);
            crdtContext.CrdtExecutors = crdtExecutors;

            componentsFactory = new ECSComponentsFactory();
            componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            crdtExecutorsManager = new CrdtExecutorsManager(crdtExecutors, componentsManager, sceneController, crdtContext);
            crdtWriteSystem = new ComponentCrdtWriteSystem(crdtExecutors, sceneController, DataStore.i.rpc.context);
            componentWriter = new ECSComponentWriter(crdtWriteSystem.WriteMessage);

            internalEcsComponents = new InternalECSComponents(componentsManager, componentsFactory, crdtExecutors);
            componentsComposer = new ECS7ComponentsComposer(componentsFactory, componentWriter, internalEcsComponents);

            sceneNumberMapping = new Dictionary<int, IParcelScene>(81); // Scene Load Radius 4 -> max scenes 81
            crdtContext.sceneStateHandler = new SceneStateHandler(
                sceneNumberMapping,
                internalEcsComponents.EngineInfo,
                internalEcsComponents.GltfContainerLoadingStateComponent);

            SystemsContext systemsContext = new SystemsContext(componentWriter, crdtContext.sceneStateHandler,
                internalEcsComponents,
                new ComponentGroups(componentsManager),
                (ECSComponent<PBBillboard>)componentsManager.GetOrCreateComponent(ComponentID.BILLBOARD));

            systemsController = new ECSSystemsController(crdtWriteSystem.LateUpdate, systemsContext);

            sceneController.OnNewSceneAdded += SceneControllerOnNewSceneAdded;
            sceneController.OnSceneRemoved += SceneControllerOnSceneRemoved;
        }

        public void Dispose()
        {
            componentsComposer.Dispose();
            crdtWriteSystem.Dispose();
            componentWriter.Dispose();
            systemsController.Dispose();
            internalEcsComponents.Dispose();
            crdtExecutorsManager.Dispose();

            sceneController.OnNewSceneAdded -= SceneControllerOnNewSceneAdded;
            sceneController.OnSceneRemoved -= SceneControllerOnSceneRemoved;
        }

        private void SceneControllerOnNewSceneAdded(IParcelScene scene)
        {
            if (scene.sceneData.sdk7)
            {
                loadedScenes.Add(scene);
                sceneNumberMapping.Add(scene.sceneData.sceneNumber, scene);
            }
        }

        private void SceneControllerOnSceneRemoved(IParcelScene scene)
        {
            if (scene.sceneData.sdk7)
            {
                loadedScenes.Remove(scene);
                sceneNumberMapping.Remove(scene.sceneData.sceneNumber);
            }
        }
    }
}
