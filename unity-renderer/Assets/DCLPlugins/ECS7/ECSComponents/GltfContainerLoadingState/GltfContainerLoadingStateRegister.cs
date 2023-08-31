using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class GltfContainerLoadingStateRegister
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public GltfContainerLoadingStateRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            factory.AddOrReplaceComponent(componentId, null, ProtoSerialization.Deserialize<PBGltfContainerLoadingState>);
            componentWriter.AddOrReplaceComponentSerializer<PBGltfContainerLoadingState>(componentId, ProtoSerialization.Serialize);

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
