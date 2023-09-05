using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class AnimatorRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public AnimatorRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            var poolWrapper = new ECSReferenceTypeIecsComponentPool<PBAnimator>(
                new WrappedComponentPool<IWrappedComponent<PBAnimator>>(10,
                    () => new ProtobufWrappedComponent<PBAnimator>(new PBAnimator()))
            );

            factory.AddOrReplaceComponent(componentId,
                () => new AnimatorComponentHandler(DataStore.i.ecs7),
                // AnimatorSerializer.Deserialize // FD::
                iecsComponentPool: poolWrapper
                );

            componentWriter.AddOrReplaceComponentSerializer<PBAnimator>(componentId, AnimatorSerializer.Serialize);

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
