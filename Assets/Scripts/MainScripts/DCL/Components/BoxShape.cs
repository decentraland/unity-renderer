using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public class BoxShape : BaseParametrizedShape<BoxShape.Model>
    {
        [System.Serializable]
        new public class Model : BaseShape.Model
        {
        }

        public override string componentName => "Box Shape";

        public BoxShape(ParcelScene scene) : base(scene) { }

        public static Mesh cubeMesh = PrimitiveMeshBuilder.BuildCube(1f);

        public override Mesh GenerateGeometry()
        {
            return cubeMesh;
        }
    }
}
