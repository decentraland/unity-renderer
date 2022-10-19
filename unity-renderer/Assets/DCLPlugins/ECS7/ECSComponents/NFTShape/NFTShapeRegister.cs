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
            var shapeFrameFactory = Resources.Load<NFTShapeFrameFactory>("NFTShapeFrameFactory");
            factory.AddOrReplaceComponent(componentId, ProtoSerialization.Deserialize<PBNftShape>,
                () => new ECSNFTShapeComponentHandler(shapeFrameFactory,
                    new NFTInfoRetriever(),
                    new NFTAssetRetriever(),
                    internalComponents.renderersComponent));
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