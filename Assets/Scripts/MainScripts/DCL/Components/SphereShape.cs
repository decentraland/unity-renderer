using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Components
{
    public class SphereShape : BaseParametrizedShape<SphereShape.Model>
    {
        [System.Serializable]
        new public class Model : BaseShape.Model
        {
        }

        public override string componentName => "Sphere Shape";

        public SphereShape(ParcelScene scene) : base(scene) { }

        public override Mesh GenerateGeometry()
        {
            return PrimitiveMeshBuilder.BuildSphere(1f);
        }
    }
}
