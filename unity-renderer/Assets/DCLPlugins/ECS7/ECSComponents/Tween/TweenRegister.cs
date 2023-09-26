using DCL.ECS7;
using DCL.ECS7.ComponentWrapper.Generic;
using System;
using DCL.ECSRuntime;
using System.Collections.Generic;

namespace DCL.ECSComponents
{
    public class TweenRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public TweenRegister(
            int componentId,
            ECSComponentsFactory factory,
            IECSComponentWriter componentWriter,
            IInternalECSComponents internalComponents,
            WrappedComponentPool<IWrappedComponent<PBTweenState>> tweenStateComponentPool,
            IReadOnlyDictionary<int, ComponentWriter> componentsWriter)
        {
            factory.AddOrReplaceComponent(componentId, ProtoSerialization.Deserialize<PBTween>, () => new ECSTweenHandler(
                internalComponents.TweenComponent,
                tweenStateComponentPool,
                componentsWriter));
            componentWriter.AddOrReplaceComponentSerializer<PBTween>(componentId, ProtoSerialization.Serialize);

            factory.AddOrReplaceComponent(ComponentID.TWEEN_STATE, ProtoSerialization.Deserialize<PBTweenState>, null);
            componentWriter.AddOrReplaceComponentSerializer<PBTweenState>(ComponentID.TWEEN_STATE, ProtoSerialization.Serialize);

            this.factory = factory;
            this.componentWriter = componentWriter;
            this.componentId = componentId;
        }

        public void Dispose()
        {
            factory.RemoveComponent(componentId);
            componentWriter.RemoveComponentSerializer(componentId);
            componentWriter.RemoveComponentSerializer(ComponentID.TWEEN_STATE);
        }
    }
}
