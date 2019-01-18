using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Components
{

    [System.Serializable]
    public class PlaneModel
    {
        public List<float>[] uvs;
        public float width = 1f;   // Plane
        public float height = 1f;  // Plane
        public bool withCollisions;
    }

    public class PlaneShape : BaseParametrizedShape<PlaneModel>
    {
        public PlaneShape(ParcelScene scene) : base(scene) { }

        public override Mesh GenerateGeometry()
        {
            // TODO: Set UVs
            return PrimitiveMeshBuilder.BuildPlane(1f);
        }

        public override bool HasCollisions() => this.model.withCollisions;
    }
}
