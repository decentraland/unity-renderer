using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using System;

namespace DCL.ECSComponents
{
    public class AnimatorRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public AnimatorRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter,
            IInternalECSComponent<InternalAnimationPlayer> internalAnimationPlayer)
        {
            var handler = new AnimatorHandler(internalAnimationPlayer);
            factory.AddOrReplaceComponent(componentId, ProtoSerialization.Deserialize<PBAnimator>, () => handler);
            componentWriter.AddOrReplaceComponentSerializer<PBAnimator>(componentId, ProtoSerialization.Serialize);

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
