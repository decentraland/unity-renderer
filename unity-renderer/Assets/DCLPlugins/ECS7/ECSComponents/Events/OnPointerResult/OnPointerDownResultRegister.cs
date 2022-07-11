using DCL;
using DCL.ECSComponents;
using DCL.ECSRuntime;

namespace DCLPlugins.ECSComponents
{
    public class OnPointerDownResultRegister
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private int componentId;
        
        public OnPointerDownResultRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            factory.AddOrReplaceComponent(componentId, OnPointerResultSerializer.DeserializeDown, null);
            componentWriter.AddOrReplaceComponentSerializer<PBOnPointerDownResult>(componentId, OnPointerResultSerializer.Serialize);

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