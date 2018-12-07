using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Components {
  public abstract class BaseShape<T> : BaseComponent<T> where T : new() {
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public override string componentName => "shape";

    void Awake() {
      meshFilter = gameObject.GetComponent<MeshFilter>();
      if (!meshFilter) {
        meshFilter = gameObject.AddComponent<MeshFilter>();
      }

      meshRenderer = gameObject.GetComponent<MeshRenderer>();
      if (!meshRenderer) {
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
      }
    }

    void OnDestroy() {
      if (meshFilter) {
        Destroy(meshFilter);
      }
      if (meshRenderer) {
        Destroy(meshRenderer);
      }
    }
  }
}
