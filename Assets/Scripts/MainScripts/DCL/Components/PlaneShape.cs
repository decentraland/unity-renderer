using DCL.Controllers;
using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Components
{
    public class PlaneShape : BaseParametrizedShape<PlaneShape.Model>
    {
        [System.Serializable]
        new public class Model : BaseShape.Model
        {
            public List<float>[] uvs;
            public float width = 1f;   // Plane
            public float height = 1f;  // Plane
        }

        public override string componentName => "Plane Shape";

        public PlaneShape(ParcelScene scene) : base(scene) { }

        public static Mesh planeMesh = null;

        public override Mesh GenerateGeometry()
        {
            if (planeMesh == null)
            {
                planeMesh = PrimitiveMeshBuilder.BuildPlane(1f);
            }
            // TODO: Set UVs
            return planeMesh;
        }
    }
}
