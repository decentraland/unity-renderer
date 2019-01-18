using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Components
{
    [System.Serializable]
    public class SphereModel
    {
        public bool withCollisions;
    }

    public class SphereShape : BaseParametrizedShape<SphereModel>
    {
        public SphereShape(ParcelScene scene) : base(scene) { }

        public override Mesh GenerateGeometry()
        {
            return PrimitiveMeshBuilder.BuildSphere(1f);
        }

        public override bool HasCollisions() => this.model.withCollisions;
    }
}
