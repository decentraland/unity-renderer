using DCL.ECS7.ComponentWrapper;
using DCL.ECS7.ComponentWrapper.Generic;
using DCL.ECSRuntime;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class NFTShapeRegister
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private readonly int componentId;

        public NFTShapeRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter, IInternalECSComponents internalComponents)
        {
            var poolWrapper = new ECSReferenceTypeIECSComponentPool<PBNftShape>(
                new WrappedComponentPool<IWrappedComponent<PBNftShape>>(10,
                    () => new ProtobufWrappedComponent<PBNftShape>(new PBNftShape()))
            );

            var shapeFrameFactory = Resources.Load<NFTShapeFrameFactory>("NFTShapeFrameFactory");
            factory.AddOrReplaceComponent(componentId,
                () => new ECSNFTShapeComponentHandler(shapeFrameFactory,
                    new NFTInfoRetriever(),
                    new NFTAssetRetriever(),
                    internalComponents.renderersComponent),
                ProtoSerialization.Deserialize<PBNftShape>, // FD::
                iecsComponentPool: poolWrapper
                );
            componentWriter.AddOrReplaceComponentSerializer<PBNftShape>(componentId, ProtoSerialization.Serialize);

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
