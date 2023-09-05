using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class PointerLockRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public PointerLockRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            var poolWrapper = new ECSReferenceTypeIecsComponentPool<PBPointerLock>(
                new WrappedComponentPool<IWrappedComponent<PBPointerLock>>(10,
                    () => new ProtobufWrappedComponent<PBPointerLock>(new PBPointerLock()))
            );

            // since this component travels only one way (from renderer to scene) we don't need a handler
            factory.AddOrReplaceComponent(
                componentId,
                () => null,
                iecsComponentPool: poolWrapper // FD:: changed
            );

            componentWriter.AddOrReplaceComponentSerializer<PBPointerLock>(componentId, PointerLockSerializer.Serialize);

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
