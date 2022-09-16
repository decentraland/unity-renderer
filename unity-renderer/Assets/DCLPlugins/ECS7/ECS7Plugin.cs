using DCL.Controllers;
using DCL.CRDT;
using DCL.ECSComponents;
using DCL.ECSRuntime;

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
            DataStore.i.ecs7.isEcs7Enabled = true;

            sceneController = Environment.i.world.sceneController;

            componentsFactory = new ECSComponentsFactory();
            componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            internalEcsComponents = new InternalECSComponents(componentsManager, componentsFactory);

            crdtWriteSystem = new ComponentCrdtWriteSystem(Environment.i.world.state, sceneController, DataStore.i.rpcContext.context);
            componentWriter = new ECSComponentWriter(crdtWriteSystem.WriteMessage);

            componentsComposer = new ECS7ComponentsComposer(componentsFactory, componentWriter, internalEcsComponents);

            SystemsContext systemsContext = new SystemsContext(componentWriter,
                internalEcsComponents,
                new ComponentGroups(componentsManager),
                (ECSComponent<PBPointerEvents>)componentsManager.GetOrCreateComponent(ComponentID.POINTER_EVENTS));

            systemsController = new ECSSystemsController(crdtWriteSystem.LateUpdate, systemsContext);

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