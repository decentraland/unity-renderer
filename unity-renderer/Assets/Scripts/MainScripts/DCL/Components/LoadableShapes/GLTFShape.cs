using DCL.Controllers;

namespace DCL.Components
{
    public class GLTFShape : LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>
    {
        public override string componentName => "GLTF Shape";

        public GLTFShape(ParcelScene scene) : base(scene)
        {
        }
    }
}
