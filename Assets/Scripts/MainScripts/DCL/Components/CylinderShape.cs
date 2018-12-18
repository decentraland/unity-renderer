using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Components {
  [Serializable]
  public class CylinderShapeModel {
    public float radiusTop = 1f;        // Cone/Cylinder
    public float radiusBottom = 1f;     // Cone/Cylinder
    public float segmentsHeight = 1f;   // Cone/Cylinder
    public float segmentsRadial = 36f;  // Cone/Cylinder
    public bool openEnded = false;      // Cone/Cylinder
    public float? radius;               // Cone/Cylinder
    public float arc = 360f;            // Cone/Cylinder
  }

  public class CylinderShape : BaseShape<CylinderShapeModel> {
    public override IEnumerator ApplyChanges() {
      meshFilter.mesh = PrimitiveMeshBuilder.BuildCylinder(50, data.radiusTop, data.radiusBottom, 2f, 0f, true, false);
      return null;
    }
  }
}
