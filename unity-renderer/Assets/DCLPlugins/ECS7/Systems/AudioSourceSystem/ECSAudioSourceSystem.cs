using DCL.ECS7;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECS7.InternalComponents;
using DCL.ECSComponents;
using System.Collections.Generic;

namespace ECSSystems.AudioSourceSystem
{
    public class ECSAudioSourceSystem
    {
        private readonly IInternalECSComponent<InternalAudioSource> audioSourceComponent;
        private readonly IReadOnlyDictionary<int, ComponentWriter> componentsWriter;
        private readonly WrappedComponentPool<IWrappedComponent<PBAudioSource>> audioSourcePool;

        public ECSAudioSourceSystem(
            IInternalECSComponent<InternalAudioSource> audioSourceComponent,
            IReadOnlyDictionary<int, ComponentWriter> componentsWriter,
            WrappedComponentPool<IWrappedComponent<PBAudioSource>> audioSourcePool)
        {
            this.audioSourceComponent = audioSourceComponent;
            this.componentsWriter = componentsWriter;
            this.audioSourcePool = audioSourcePool;
        }

        public void Update()
        {
            var audioSourceComponentList = audioSourceComponent.GetForAll();

            // Loop through every audio source component
            for(var i = 0; i < audioSourceComponentList.Count; i++)
            {
                var component = audioSourceComponentList[i];
                var scene = component.key1;
                long entity = component.key2;
                var model = component.value.model;

                if (!componentsWriter.TryGetValue(scene.sceneData.sceneNumber, out var writer)) continue;
                var wrappedComponent = audioSourcePool.Get();
                wrappedComponent.WrappedComponent.Model.Playing = model.audioSource.isPlaying;
                wrappedComponent.WrappedComponent.Model.Loop = model.loop;
                wrappedComponent.WrappedComponent.Model.Volume = model.volume;
                wrappedComponent.WrappedComponent.Model.Pitch = model.pitch;
                wrappedComponent.WrappedComponent.Model.AudioClipUrl ??= model.audioClipUrl;
                wrappedComponent.WrappedComponent.Model.CurrentTime = model.audioSource.time;
                writer.Put(entity, ComponentID.AUDIO_SOURCE, wrappedComponent);
            }
        }
    }
}
