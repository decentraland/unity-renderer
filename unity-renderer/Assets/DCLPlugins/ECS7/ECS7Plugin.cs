using System.Collections.Generic;
using DCL.Controllers;
using DCL.CRDT;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCLPlugins.ECSComponents.Events;

namespace DCL.ECS7
{
    public class ECS7Plugin : IPlugin
    {
        private readonly ComponentCrdtWriteSystem crdtWriteSystem;
        private readonly IECSComponentWriter componentWriter;
        private readonly ECS7ComponentsComposer componentsComposer;
        private readonly ECSSystemsController systemsController;
        private readonly ECSComponentsFactory componentsFactory;
        private readonly ECSComponentsManager componentsManager;
        private readonly InternalECSComponents internalEcsComponents;
        private readonly CanvasPainter canvasPainter;

        private readonly ISceneController sceneController;

        public ECS7Plugin()
        {
            sceneController = Environment.i.world.sceneController;

            componentsFactory = new ECSComponentsFactory();
            componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            internalEcsComponents = new InternalECSComponents(componentsManager, componentsFactory);

            crdtWriteSystem = new ComponentCrdtWriteSystem(Environment.i.world.state, sceneController, DataStore.i.rpcContext.context);
            componentWriter = new ECSComponentWriter(crdtWriteSystem.WriteMessage);

            SystemsContext systemsContext = new SystemsContext(componentWriter,
                internalEcsComponents,
                new ComponentGroups(componentsManager),
                new Queue<PointerEvent>(26));

            ECSContext context = new ECSContext(systemsContext);

            componentsComposer = new ECS7ComponentsComposer(componentsFactory, componentWriter, internalEcsComponents, context);

            systemsController = new ECSSystemsController(Environment.i.platform.updateEventHandler, crdtWriteSystem.LateUpdate, context.systemsContext);

            canvasPainter = new CanvasPainter(DataStore.i.ecs7, CommonScriptableObjects.rendererState, Environment.i.platform.updateEventHandler, componentsManager, Environment.i.world.state);

            sceneController.OnNewSceneAdded += OnSceneAdded;
        }

        public void Dispose()
        {
            componentsComposer.Dispose();
            crdtWriteSystem.Dispose();
            componentWriter.Dispose();
            systemsController.Dispose();
            internalEcsComponents.Dispose();

            canvasPainter.Dispose();

            sceneController.OnNewSceneAdded -= OnSceneAdded;
        }

        private void OnSceneAdded(IParcelScene scene)
        {
            scene.crdtExecutor = new CRDTExecutor(scene, componentsManager);
        }
    }
}