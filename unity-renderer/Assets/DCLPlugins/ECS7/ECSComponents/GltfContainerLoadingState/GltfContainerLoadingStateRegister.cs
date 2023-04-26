using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class GltfContainerLoadingStateRegister
    {
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public GltfContainerLoadingStateRegister(int componentId, IECSComponentWriter componentWriter)
        {
            componentWriter.AddOrReplaceComponentSerializer<PBGltfContainerLoadingState>(componentId, ProtoSerialization.Serialize);

            this.componentWriter = componentWriter;
            this.componentId = componentId;
        }

        public void Dispose()
        {
            componentWriter.RemoveComponentSerializer(componentId);
        }
    }
}
