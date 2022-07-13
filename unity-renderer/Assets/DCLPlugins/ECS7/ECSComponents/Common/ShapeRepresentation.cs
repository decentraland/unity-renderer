using DCL.Components;

namespace DCL.ECSComponents
{
    public class ShapeRepresentation : IShape
    {
        private bool isVisible;
        private bool withCollisions;
        
        public void UpdateModel( bool isVisible, bool withCollisions)
        {
            this.isVisible = isVisible;
            this.withCollisions = withCollisions;
        }
        
        public bool /*IShape*/ IsVisible() { return isVisible; }
        public bool /*IShape*/ HasCollisions() { return withCollisions; }
    }
}