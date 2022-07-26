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

        private readonly ISceneController sceneController;

        public ECS7Plugin()
        {
            sceneController = Environment.i.world.sceneController;
            
            componentsFactory = new ECSComponentsFactory();
            componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);

            crdtWriteSystem = new ComponentCrdtWriteSystem(Environment.i.world.state, sceneController, DataStore.i.rpcContext.context);
            componentWriter = new ECSComponentWriter(crdtWriteSystem.WriteMessage);

            componentsComposer = new ECS7ComponentsComposer(componentsFactory, componentWriter);
            systemsController = new ECSSystemsController(Environment.i.platform.updateEventHandler, crdtWriteSystem.LateUpdate);

            sceneController.OnNewSceneAdded += OnSceneAdded;
        }

        public void Dispose()
        {
            componentsComposer.Dispose();
            crdtWriteSystem.Dispose();
            componentWriter.Dispose();
            systemsController.Dispose();
            
            sceneController.OnNewSceneAdded -= OnSceneAdded;
        }

        private void OnSceneAdded(IParcelScene scene)
        {
            scene.crdtExecutor = new CRDTExecutor(scene, componentsManager);
        }
    }
}