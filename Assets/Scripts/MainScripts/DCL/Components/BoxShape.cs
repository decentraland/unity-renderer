using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    [System.Serializable]
    public class BoxModel : BaseParamShapeModel
    {
    }

    public class BoxShape : BaseParametrizedShape<BoxModel>
    {
        public BoxShape(ParcelScene scene) : base(scene) { }

        public static Mesh cubeMesh = PrimitiveMeshBuilder.BuildCube(1f);

        public override Mesh GenerateGeometry()
        {
            return cubeMesh;
        }
    }
}
