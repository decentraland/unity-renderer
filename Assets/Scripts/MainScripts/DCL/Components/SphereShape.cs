using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Components {
  [Serializable]
  public class SphereShapeModel {
    public string tag;
  }

  public class SphereShape : BaseShape<SphereShapeModel> {
    public override IEnumerator ApplyChanges() {
      meshFilter.mesh = PrimitiveMeshBuilder.BuildSphere(1f);
      return null;
    }
  }
}
