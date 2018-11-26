using System;
using UnityEngine;

namespace DCL.Models {
  [Serializable]
  public class DecentralandEntity {
    // TODO: interfaces do not have logic or game objects
    public GameObject gameObjectReference;
    public string id;
    public string parentId;
    public EntityComponents components = null;

    Vector3 auxVector;

    public void UpdateGameObjectComponents() {
      if (gameObjectReference != null) {

        if (components.transform != null) {
          // Apply position update
          auxVector.Set(components.transform.position.x,
                                    components.transform.position.y,
                                    components.transform.position.z);

          gameObjectReference.transform.position = auxVector;

          // Apply rotation update
          auxVector.Set(components.transform.rotation.x,
                                    components.transform.rotation.y,
                                    components.transform.rotation.z);

          gameObjectReference.transform.rotation = Quaternion.Euler(auxVector);

          // Apply scale update
          auxVector.Set(components.transform.scale.x,
                                    components.transform.scale.y,
                                    components.transform.scale.z);

          gameObjectReference.transform.localScale = auxVector;
        }

        // TODO: Update the rest of the components.
      }
    }

    [Serializable]
    public class EntityComponents {
      public EntityShape shape = null;
      public EntityTransform transform = null;
    }

    [Serializable]
    public class EntityShape {
      public string tag;
      public string src;
    }

    [Serializable]
    public class EntityTransform {
      public Vector3 position;
      public Vector3 rotation;
      public Vector3 scale;
    }
  }
}
