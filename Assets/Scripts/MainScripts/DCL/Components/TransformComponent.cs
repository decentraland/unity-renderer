using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Components {
  [Serializable]
  public class TransformModel {
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
  }

  public class TransformComponent : BaseComponent<TransformModel> {
    public override string componentName => "transform";

    public override IEnumerator ApplyChanges() {
      // this component is applied to the gameObjects transform
      if (gameObject != null) {
        var t = gameObject.transform;

        if (t.localPosition != data.position) {
          t.localPosition = data.position;
        }

        if (t.localRotation != data.rotation) {
          t.localRotation = data.rotation;
        }

        if (t.localScale != data.scale) {
          t.localScale = data.scale;
        }
      }
      return null;
    }

    void OnDisable() {
      if (gameObject != null) {
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localScale = Vector3.one;
        gameObject.transform.localRotation = Quaternion.identity;
      }
    }
  }
}
