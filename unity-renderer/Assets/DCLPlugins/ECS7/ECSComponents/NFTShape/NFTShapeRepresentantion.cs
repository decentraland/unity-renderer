using DCL.Components;

namespace DCL.ECSComponents
{
    public class NFTShapeRepresentantion : IShape
    {
        private readonly PBNFTShape shape;
        
        public NFTShapeRepresentantion(PBNFTShape shape)
        {
            this.shape = shape;
        }

        public bool IsVisible() { return shape.Visible; }
        
        public bool HasCollisions() { return shape.WithCollisions; }
    }
}