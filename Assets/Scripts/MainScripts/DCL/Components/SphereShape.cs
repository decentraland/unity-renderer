using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Components {
  
  public class SphereShape : BaseShape {

    [System.Serializable]
    public class Model
    {
      public string tag;
    }

    Model model = new Model();

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

    public override IEnumerator ApplyChanges(string newJson) {
      meshFilter.mesh = PrimitiveMeshBuilder.BuildSphere(1f);

      return null;
    }
  }
}
