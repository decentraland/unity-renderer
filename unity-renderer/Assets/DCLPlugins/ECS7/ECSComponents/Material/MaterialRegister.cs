using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class MaterialRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public MaterialRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter, IInternalECSComponents internalComponents)
        {
            factory.AddOrReplaceComponent(componentId,
                ProtoSerialization.Deserialize<PBMaterial>,
                () => new MaterialHandler(internalComponents.materialComponent));
            componentWriter.AddOrReplaceComponentSerializer<PBMaterial>(componentId, ProtoSerialization.Serialize);

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