using DCL.Components;

namespace DCL.ECSComponents
{
    public class ShapeRepresentation : IShape
    {
        private PBGLTFShape model;
        
        public void UpdateModel(PBGLTFShape model)
        {
            this.model = model;
        }
        
        public bool /*IShape*/ IsVisible() { return model.Visible; }
        public bool /*IShape*/ HasCollisions() { return model.WithCollisions; }
    }
}