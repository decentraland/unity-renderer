using DCL;
using DCL.ECSComponents;
using DCL.ECSRuntime;

namespace DCL.ECS7
{
    public class ECS7Plugin : IPlugin
    {
        private readonly ComponentCrdtWriteSystem crdtWriteSystem;
        private readonly IECSComponentWriter componentWriter;
        private readonly ECS7ComponentsPlugin componentsPlugin;

        public ECS7Plugin()
        {
            crdtWriteSystem = new ComponentCrdtWriteSystem(Environment.i.platform.updateEventHandler, Environment.i.world.state);
            componentWriter = new ECSComponentWriter(crdtWriteSystem.WriteMessage);

            componentsPlugin = new ECS7ComponentsPlugin(DataStore.i.ecs7.componentsFactory, componentWriter);
        }

        public void Dispose()
        {
            componentsPlugin.Dispose();
            crdtWriteSystem.Dispose();
            componentWriter.Dispose();
        }
    }
}