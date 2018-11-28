using System;
using UnityEngine;

namespace DCL.Models {
  [Serializable]
  public class DecentralandEntity {
    // TODO: interfaces do not have logic or game objects
    public GameObject gameObject;
    public string entityId;
    public string parentId;
    public EntityComponents components = null;

    [Serializable]
    public class EntityComponents {
      public EntityShape shape = null;
      public EntityTransform transform = null;
    }

    [Serializable]
    public class EntityShape {
      public string tag;
      public string src;

      public bool Equals(EntityShape b) {
        if (b == this) return true;
        if (b == null) return false;
        return this.tag == b.tag && this.src == b.src;
      }
    }

    [Serializable]
    public class EntityTransform {
      public Vector3 position;
      public Quaternion rotation;
      public Vector3 scale;

      public void ApplyTo(GameObject gameObject) {
        if (gameObject != null) {
          var t = gameObject.transform;

          if (t.localPosition != position) {
            t.localPosition = position;
          }

          if (t.localRotation != rotation) {
            t.localRotation = rotation;
          }

          if (t.localScale != scale) {
            t.localScale = scale;
          }
        }
      }
    }
  }
}
