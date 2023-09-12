using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECS7.InternalComponents;
using System;
using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class VideoPlayerRegister : IDisposable
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public VideoPlayerRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter, IInternalECSComponents internalComponents)
        {
            var poolWrapper = new ECSReferenceTypeIECSComponentPool<PBVideoPlayer>(
                new WrappedComponentPool<IWrappedComponent<PBVideoPlayer>>(10,
                    () => new ProtobufWrappedComponent<PBVideoPlayer>(new PBVideoPlayer()))
            );

            factory.AddOrReplaceComponent(componentId,
                () => new VideoPlayerHandler(
                    internalComponents.videoPlayerComponent,
                    DataStore.i.Get<DataStore_LoadingScreen>().decoupledLoadingHUD),
                    ProtoSerialization.Deserialize<PBVideoPlayer>, // FD::
                    iecsComponentPool: poolWrapper // FD:: changed
                );

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
