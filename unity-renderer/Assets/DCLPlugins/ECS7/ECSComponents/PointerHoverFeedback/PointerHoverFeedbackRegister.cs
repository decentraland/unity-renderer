using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class PointerHoverFeedbackRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public PointerHoverFeedbackRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            factory.AddOrReplaceComponent(componentId,
                ProtoSerialization.Deserialize<PBPointerHoverFeedback>,
                null);
            componentWriter.AddOrReplaceComponentSerializer<PBPointerHoverFeedback>(componentId, ProtoSerialization.Serialize);

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