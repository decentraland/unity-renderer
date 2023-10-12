using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class TweenRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public TweenRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter, IInternalECSComponents internalComponents)
        {
            factory.AddOrReplaceComponent(componentId, ProtoSerialization.Deserialize<PBTween>, () => new ECSTweenHandler(internalComponents.TweenComponent, internalComponents.sceneBoundsCheckComponent));
            componentWriter.AddOrReplaceComponentSerializer<PBTween>(componentId, ProtoSerialization.Serialize);

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
