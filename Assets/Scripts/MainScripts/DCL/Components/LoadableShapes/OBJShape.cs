using DCL.Controllers;

namespace DCL.Components
{
    public class OBJShape : LoadableShape<LoadWrapper_OBJ, LoadableShape.Model>
    {
        public OBJShape(ParcelScene scene) : base(scene)
        {
        }
    }
}
