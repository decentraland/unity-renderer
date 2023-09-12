using DCL.ECS7.InternalComponents;
using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class AudioStreamRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public AudioStreamRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter,
            IInternalECSComponent<InternalMediaEnabledTag> mediaEnabledTagComponent)
        {
            factory.AddOrReplaceComponent(componentId, ProtoSerialization.Deserialize<PBAudioStream>,
                () => new ECSAudioStreamComponentHandler(mediaEnabledTagComponent));
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
