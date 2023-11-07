using System;
using DCL.ECSRuntime;
using DCL.SettingsCommon;

namespace DCL.ECSComponents
{
    public class VideoPlayerRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public VideoPlayerRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter, IInternalECSComponents internalComponents)
        {
            factory.AddOrReplaceComponent(componentId,
                ProtoSerialization.Deserialize<PBVideoPlayer>,
                () => new VideoPlayerHandler(
                    internalComponents.videoPlayerComponent,
                    DataStore.i.Get<DataStore_LoadingScreen>().decoupledLoadingHUD,
                    Settings.i.audioSettings,
                    DataStore.i.virtualAudioMixer));
            componentWriter.AddOrReplaceComponentSerializer<PBVideoPlayer>(componentId, ProtoSerialization.Serialize);

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
