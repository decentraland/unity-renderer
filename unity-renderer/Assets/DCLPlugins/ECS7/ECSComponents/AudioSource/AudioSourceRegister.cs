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
            factory.AddOrReplaceComponent(componentId, AudioSourceSerializer.Deserialize, () => new ECSAudioSourceComponentHandler(
                DataStore.i,
                Settings.i,
                AssetPromiseKeeper_AudioClip.i,
                CommonScriptableObjects.sceneNumber,
                internalComponents.audioSourceComponent,
                internalComponents.sceneBoundsCheckComponent));
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
