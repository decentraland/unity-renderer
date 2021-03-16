using DCL.Controllers;
using DCL.Models;

namespace DCL.Components
{
    public class GLTFShape : LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>
    {
        public override string componentName => "GLTF Shape";

        public GLTFShape()
        {
        }

        public override int GetClassId()
        {
            return (int) CLASS_ID.GLTF_SHAPE;
        }

        public override string ToString()
        {
            if (scene == null || scene.contentProvider == null || model == null)
                return base.ToString();

            string fullUrl;

            bool found = scene.contentProvider.TryGetContentsUrl(model.src, out fullUrl);

            if (!found)
                fullUrl = "Not found!";

            return $"{componentName} (src = {model.src}, full url = {fullUrl}";
        }
    }
}