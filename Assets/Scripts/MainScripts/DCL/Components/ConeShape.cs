using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Components
{
    [System.Serializable]
    public class ConeModel
    {
        public float radiusTop = 0f;        // Cone/Cylinder
        public float radiusBottom = 1f;     // Cone/Cylinder
        public float segmentsHeight = 1f;   // Cone/Cylinder
        public float segmentsRadial = 36f;  // Cone/Cylinder
        public bool openEnded = false;      // Cone/Cylinder
        public float? radius;               // Cone/Cylinder
        public float arc = 360f;            // Cone/Cylinder
        public bool withCollisions;
    }

    public class ConeShape : BaseParametrizedShape<ConeModel>
    {
        public ConeShape(ParcelScene scene) : base(scene) { }

        public override Mesh GenerateGeometry()
        {
            return PrimitiveMeshBuilder.BuildCone(50, model.radiusTop, model.radiusBottom, 2f, 0f, true, false);
        }

        public override bool HasCollisions() => this.model.withCollisions;
    }
}
