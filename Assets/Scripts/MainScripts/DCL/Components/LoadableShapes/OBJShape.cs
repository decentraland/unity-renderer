using DCL.Controllers;

namespace DCL.Components
{
    public class OBJShape : BaseLoadableShape<OBJLoader>
    {
        public OBJShape(ParcelScene scene) : base(scene)
        {
        }
    }
}