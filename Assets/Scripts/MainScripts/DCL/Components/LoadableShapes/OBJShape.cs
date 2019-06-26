using DCL.Controllers;

namespace DCL.Components
{
    public class OBJShape : LoadableShape<LoadWrapper_OBJ>
    {
        public OBJShape(ParcelScene scene) : base(scene)
        {
        }
    }
}
