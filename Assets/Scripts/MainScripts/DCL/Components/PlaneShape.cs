using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Components {
  [Serializable]
  public class PlaneShapeModel {
    public List<float>[] uvs;
    public float width = 1f;   // Plane
    public float height = 1f;  // Plane
  }

  public class PlaneShape : BaseShape<PlaneShapeModel> {
    public override IEnumerator ApplyChanges() {
      meshFilter.mesh = PrimitiveMeshBuilder.BuildPlane(1f);
      return null;
    }
  }
}
