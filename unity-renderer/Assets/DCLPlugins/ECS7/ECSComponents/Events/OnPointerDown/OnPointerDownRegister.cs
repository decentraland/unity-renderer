using System.Collections.Generic;
using DCL;
using DCL.ECS7;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCLPlugins.ECS7.ECSComponents.Events.OnPointerDown.Serializer;
using DCLPlugins.ECSComponents.OnPointerDown;

namespace DCLPlugins.ECSComponents
{
    public class OnPointerDownRegister
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private int componentId;
        
        public OnPointerDownRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter, IECSContext context)
        {
            factory.AddOrReplaceComponent(componentId, OnPointerDownSerializer.Deserialize, () => new OnPointerDownComponentHandler(componentWriter, DataStore.i.ecs7, context));
            componentWriter.AddOrReplaceComponentSerializer<PBOnPointerDown>(componentId, OnPointerDownSerializer.Serialize);

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