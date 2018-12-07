using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Components {
  [Serializable]
  public class BoxShapeModel {
    public string tag;
  }

  public class BoxShape : BaseShape<BoxShapeModel> {
    public override IEnumerator ApplyChanges() {
      meshFilter.mesh = PrimitiveMeshBuilder.BuildCube(1f);
      return null;
    }
  }
}
