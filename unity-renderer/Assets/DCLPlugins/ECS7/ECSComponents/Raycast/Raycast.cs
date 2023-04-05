using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCLPlugins.ECSComponents.Raycast;

namespace DCLPlugins.ECSComponents
{
    public class RaycastRegister
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private int componentId;
        
        public RaycastRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter, IInternalECSComponents internalComponents)
        {
            factory.AddOrReplaceComponent(
                componentId, 
                ProtoSerialization.Deserialize<PBRaycast>, 
                () => new RaycastComponentHandler(componentWriter, internalComponents.physicColliderComponent)
            );
            componentWriter.AddOrReplaceComponentSerializer<PBRaycast>(componentId, ProtoSerialization.Serialize);

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