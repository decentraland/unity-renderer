using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class AudioStreamRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public AudioStreamRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            var poolWrapper = new ECSReferenceTypeIECSComponentPool<PBAudioStream>(
                new WrappedComponentPool<IWrappedComponent<PBAudioStream>>(10,
                    () => new ProtobufWrappedComponent<PBAudioStream>(new PBAudioStream()))
            );

            factory.AddOrReplaceComponent(componentId,
                () => new ECSAudioStreamComponentHandler(),
                ProtoSerialization.Deserialize<PBAudioStream>,// FD::
                iecsComponentPool: poolWrapper
                );
            componentWriter.AddOrReplaceComponentSerializer<PBAudioStream>(componentId, ProtoSerialization.Serialize);

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
