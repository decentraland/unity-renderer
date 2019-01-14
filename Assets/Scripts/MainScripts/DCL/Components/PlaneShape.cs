using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Components {

  public class PlaneShape : BaseShape {

    [System.Serializable]
    public class Model
    {
      public List<float>[] uvs;
      public float width = 1f;   // Plane
      public float height = 1f;  // Plane
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
      meshFilter.mesh = PrimitiveMeshBuilder.BuildPlane(1f);
      return null;
    }
  }
}
