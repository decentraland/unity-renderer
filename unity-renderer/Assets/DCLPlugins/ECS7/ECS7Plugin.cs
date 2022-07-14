using DCL.ECSComponents;
using DCL.ECSRuntime;
using ECSSystems.Helpers;

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
            systemsController = new ECSSystemsController(Environment.i.platform.updateEventHandler, crdtWriteSystem.LateUpdate);

            SetUpReferences();
        }

        public void Dispose()
        {
            componentsComposer.Dispose();
            crdtWriteSystem.Dispose();
            componentWriter.Dispose();
            systemsController.Dispose();
        }

        private void SetUpReferences()
        {
            ReferencesContainer.componentsWriter = componentWriter;
            ReferencesContainer.loadedScenes = Environment.i.world.state.loadedScenesList;
        }
    }
}