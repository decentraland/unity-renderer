using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class ECSTextShapeRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public ECSTextShapeRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter,
            IInternalECSComponents internalComponents)
        {
            factory.AddOrReplaceComponent(componentId, ProtoSerialization.Deserialize<PBTextShape>,
                () => new ECSTextShapeComponentHandler(AssetPromiseKeeper_Font.i, internalComponents.renderersComponent, internalComponents.sceneBoundsCheckComponent));
            componentWriter.AddOrReplaceComponentSerializer<PBTextShape>(componentId, ProtoSerialization.Serialize);

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
