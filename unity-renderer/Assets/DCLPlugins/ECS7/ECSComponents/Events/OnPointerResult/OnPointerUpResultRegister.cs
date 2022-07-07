using DCL;
using DCL.ECSComponents;
using DCL.ECSRuntime;

namespace DCLPlugins.ECSComponents
{
    public class OnPointerUpResultRegister
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private int componentId;
        
        public OnPointerUpResultRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            factory.AddOrReplaceComponent(componentId, OnPointerResultSerializer.DeserializeUp, null);
            componentWriter.AddOrReplaceComponentSerializer<PBOnPointerUpResult>(componentId, OnPointerResultSerializer.Serialize);

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