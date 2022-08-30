using DCL;
using DCL.ECSComponents;
using DCL.ECSRuntime;

namespace DCLPlugins.ECSComponents
{
    public class OnPointerResultRegister
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private int componentId;
        
        public OnPointerResultRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            factory.AddOrReplaceComponent(componentId, ProtoSerialization.Deserialize<PBPointerEventsResult>, null);
            componentWriter.AddOrReplaceComponentSerializer<PBPointerEventsResult>(componentId, ProtoSerialization.Serialize);

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