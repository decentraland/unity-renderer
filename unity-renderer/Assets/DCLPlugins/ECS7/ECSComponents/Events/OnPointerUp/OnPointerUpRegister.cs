using System.Collections.Generic;
using DCL;
using DCL.ECS7;
using DCL.ECSComponents;
using DCL.ECSComponents.OnPointerUp;
using DCL.ECSRuntime;

namespace DCLPlugins.ECSComponents
{
    public class OnPointerUpRegister
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private int componentId;
        
        public OnPointerUpRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter, IECSContext context)
        {
            factory.AddOrReplaceComponent(componentId, OnPointerUpSerializer.Deserialize, () => new OnPointerUpComponentHandler(componentWriter, DataStore.i.ecs7, context));
            componentWriter.AddOrReplaceComponentSerializer<PBOnPointerUp>(componentId, OnPointerUpSerializer.Serialize);

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