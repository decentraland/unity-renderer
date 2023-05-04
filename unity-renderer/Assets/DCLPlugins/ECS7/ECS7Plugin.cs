using DCL.Controllers;
using DCL.CRDT;
using DCL.ECSComponents;
using DCL.ECSRuntime;
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

        internal readonly ECSComponentsManager componentsManager;
        private readonly BaseList<IParcelScene> loadedScenes;
        private readonly ISceneController sceneController;

        public ECS7Plugin()
        {
            DataStore.i.ecs7.isEcs7Enabled = true;
            loadedScenes = DataStore.i.ecs7.scenes;

            sceneController = Environment.i.world.sceneController;
            Dictionary<int, ICRDTExecutor> crdtExecutors = new Dictionary<int, ICRDTExecutor>(10);
            DataStore.i.rpc.context.crdt.CrdtExecutors = crdtExecutors;

            componentsFactory = new ECSComponentsFactory();
            componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            internalEcsComponents = new InternalECSComponents(componentsManager, componentsFactory, crdtExecutors);

            crdtExecutorsManager = new CrdtExecutorsManager(crdtExecutors, componentsManager, sceneController, DataStore.i.rpc.context.crdt);

            crdtWriteSystem = new ComponentCrdtWriteSystem(crdtExecutors, sceneController, DataStore.i.rpc.context);
            componentWriter = new ECSComponentWriter(crdtWriteSystem.WriteMessage);

            componentsComposer = new ECS7ComponentsComposer(componentsFactory, componentWriter, internalEcsComponents);

            SystemsContext systemsContext = new SystemsContext(componentWriter,
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
            }
        }

        private void SceneControllerOnSceneRemoved(IParcelScene scene)
        {
            if (scene.sceneData.sdk7)
            {
                loadedScenes.Remove(scene);
            }
        }
    }
}
