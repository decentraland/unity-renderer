using DCL;
using DCL.ECSRuntime;

public class ECS7ComponentsPlugin : IPlugin
{
    private ComponentCrdtWriteSystem crdtWriteSystem;
    private ECSComponentWriter ecsComponentWriter;

    public ECS7ComponentsPlugin()
    {
        crdtWriteSystem = new ComponentCrdtWriteSystem(Environment.i.platform.updateEventHandler, Environment.i.world.state);
        ecsComponentWriter = new ECSComponentWriter(crdtWriteSystem.WriteMessage);

        DataStore.i.ecs7.componentsWriter = ecsComponentWriter;
    }

    public void Dispose()
    {
        crdtWriteSystem.Dispose();
        ecsComponentWriter.Dispose();
    }
}