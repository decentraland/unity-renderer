using DCL.Controllers;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Components
{
    public class BoxShape : BaseParametrizedShape<BoxShape.Model>
    {
        [System.Serializable]
        new public class Model : BaseShape.Model
        {
        }

        public BoxShape(ParcelScene scene) : base(scene) { }

        public static Mesh cubeMesh = null;

        public override Mesh GenerateGeometry()
        {
            if (cubeMesh == null)
            {
                cubeMesh = PrimitiveMeshBuilder.BuildCube(1f);
            }

            return cubeMesh;
        }
    }
}