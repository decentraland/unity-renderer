using DCL;
using DCL.ECS7;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCLPlugins.ECSComponents.OnPointerDown;

namespace DCLPlugins.ECSComponents
{
    public class PointerEventsRegister
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private int componentId;
        
        public PointerEventsRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter, IECSContext context)
        {
            factory.AddOrReplaceComponent(componentId, ProtoSerialization.Deserialize<PBPointerEvents>, () => new PointerEventsComponentHandler(componentWriter, DataStore.i.ecs7, context));
            componentWriter.AddOrReplaceComponentSerializer<PBPointerEvents>(componentId, ProtoSerialization.Serialize);

            this.factory = factory;
            this.componentWriter = componentWriter;
            this.componentId = componentId;
        }
        
        public void Dispose()
        {
            factory.RemoveComponent(componentId);
            componentWriter.RemoveComponentSerializer(componentId);
        }
    }
}