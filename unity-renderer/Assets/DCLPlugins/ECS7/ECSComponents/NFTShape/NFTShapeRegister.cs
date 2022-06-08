using DCL.ECSRuntime;

namespace DCL.ECSComponents
{
    public class NFTShapeRegister
    {
        private readonly ECSComponentsFactory factory;
        private readonly IECSComponentWriter componentWriter;
        private int componentId;
        
        public NFTShapeRegister(int componentId, ECSComponentsFactory factory, IECSComponentWriter componentWriter)
        {
            factory.AddOrReplaceComponent(componentId, NFTShapeSerializer.Deserialize, () => new ECSNFTShapeComponentHandler());
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