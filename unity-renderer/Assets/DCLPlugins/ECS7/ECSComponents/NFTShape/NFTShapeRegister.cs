using DCL.ECSRuntime;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class NFTShapeRegister
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private int componentId;
        
        public NFTShapeRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            var shapeFrameFactory = Resources.Load<NFTShapeFrameFactory>("NFTShapeFrameFactory");
            factory.AddOrReplaceComponent(componentId, NFTShapeSerializer.Deserialize, () => new ECSNFTShapeComponentHandler(DataStore.i.ecs7, shapeFrameFactory, new NFTInfoRetriever(), new NFTAssetRetriever()));
            componentWriter.AddOrReplaceComponentSerializer<PBNFTShape>(componentId, NFTShapeSerializer.Serialize);

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