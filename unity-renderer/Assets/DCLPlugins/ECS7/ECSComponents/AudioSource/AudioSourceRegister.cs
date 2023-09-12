using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using System;
using DCL.ECSRuntime;
using DCL.SettingsCommon;

namespace DCL.ECSComponents
{
    public class AudioSourceRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public AudioSourceRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter, IInternalECSComponents internalComponents)
        {
            var poolWrapper = new ECSReferenceTypeIECSComponentPool<PBAudioSource>(
                new WrappedComponentPool<IWrappedComponent<PBAudioSource>>(10,
                    () => new ProtobufWrappedComponent<PBAudioSource>(new PBAudioSource()))
            );

            factory.AddOrReplaceComponent(componentId,
                () => new ECSAudioSourceComponentHandler(
                    DataStore.i,
                    Settings.i,
                    AssetPromiseKeeper_AudioClip.i,
                    CommonScriptableObjects.sceneNumber,
                    internalComponents.audioSourceComponent,
                    internalComponents.sceneBoundsCheckComponent),
                AudioSourceSerializer.Deserialize, // FD::
                iecsComponentPool: poolWrapper
                );
            componentWriter.AddOrReplaceComponentSerializer<PBAudioSource>(componentId, AudioSourceSerializer.Serialize);

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
