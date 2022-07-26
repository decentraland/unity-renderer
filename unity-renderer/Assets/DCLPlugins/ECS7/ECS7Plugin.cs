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

        public ECS7Plugin()
        {
            crdtWriteSystem = new ComponentCrdtWriteSystem(Environment.i.world.state,
                Environment.i.world.sceneController, DataStore.i.rpcContext.context);
            componentWriter = new ECSComponentWriter(crdtWriteSystem.WriteMessage);

            componentsComposer = new ECS7ComponentsComposer(DataStore.i.ecs7.componentsFactory, componentWriter);
            systemsController = new ECSSystemsController(Environment.i.platform.updateEventHandler, componentWriter, crdtWriteSystem.LateUpdate);
        }

        public void Dispose()
        {
            componentsComposer.Dispose();
            crdtWriteSystem.Dispose();
            componentWriter.Dispose();
            systemsController.Dispose();
        }
    }
}