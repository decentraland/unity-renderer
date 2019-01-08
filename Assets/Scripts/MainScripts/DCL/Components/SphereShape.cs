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
    protected override void Awake() {
      base.Awake();

      if (meshFilter == null) {
        meshFilter = meshGameObject.AddComponent<MeshFilter>();
      }

      if (meshRenderer == null) {
        meshRenderer = meshGameObject.AddComponent<MeshRenderer>();
      }

      meshRenderer.sharedMaterial = Resources.Load<Material>("Materials/Default");
    }

    public override IEnumerator ApplyChanges() {
      meshFilter.mesh = PrimitiveMeshBuilder.BuildSphere(1f);

      return null;
    }
  }
}
