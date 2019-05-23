using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public class SphereShape : BaseParametrizedShape<SphereShape.Model>
    {
        [System.Serializable]
        new public class Model : BaseShape.Model
        {
        }

        public SphereShape(ParcelScene scene) : base(scene)
        {
        }

        public static Mesh mesh = null;

        public override Mesh GenerateGeometry()
        {
            if (mesh == null)
            {
                mesh = PrimitiveMeshBuilder.BuildSphere(1f);
            }

            return mesh;
        }
    }
}
