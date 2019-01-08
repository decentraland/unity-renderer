using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGLTF;
using DCL.Helpers;

namespace DCL.Components {
  [Serializable]
  public class OnClickModel {
    public string uuid;
  }

  public class OnClick : BaseComponent<OnClickModel> {
    public override string componentName => "onClick";

    Rigidbody rigidBody;
    GameObject[] OnClickColliderGameObjects;

    public override IEnumerator ApplyChanges() {
      return null;
    }

    void Start() {
      if (GetComponentInChildren<MeshFilter>() != null) {
        Initialize();
      }

      entity.OnComponentUpdated -= OnComponentUpdated;
      entity.OnComponentUpdated += OnComponentUpdated;
    }

    public void Initialize() {
      // we add a rigidbody to be able to detect the children colliders for the OnClick functionality
      if (gameObject.GetComponent<Rigidbody>() == null) {
        rigidBody = gameObject.AddComponent<Rigidbody>();
        rigidBody.useGravity = false;
        rigidBody.isKinematic = true;
      }

      // Create OnClickCollider child
      var meshFilters = GetComponentsInChildren<MeshFilter>();
      var onClickColliderObjectName = "OnClickCollider";
      var onClickColliderObjectLayer = LayerMask.NameToLayer("OnClick");

      DestroyOnClickColliders();

      OnClickColliderGameObjects = new GameObject[meshFilters.Length];
      for (int i = 0; i < OnClickColliderGameObjects.Length; i++) {
        OnClickColliderGameObjects[i] = new GameObject();
        OnClickColliderGameObjects[i].name = onClickColliderObjectName;
        OnClickColliderGameObjects[i].layer = onClickColliderObjectLayer; // to avoid movement collisions with its collider

        var meshCollider = OnClickColliderGameObjects[i].AddComponent<MeshCollider>();
        meshCollider.sharedMesh = meshFilters[i].mesh;

        // Reset objects position, rotation and scale once it's been parented
        OnClickColliderGameObjects[i].transform.SetParent(meshFilters[i].transform);
        OnClickColliderGameObjects[i].transform.localScale = new Vector3(1, 1, 1);
        OnClickColliderGameObjects[i].transform.localRotation = Quaternion.identity;
        OnClickColliderGameObjects[i].transform.localPosition = Vector3.zero;
      }
    }

    void OnComponentUpdated(DCL.Components.UpdateableComponent updatedComponent) {
      if (!LandHelpers.IsShapeComponent(updatedComponent)) return;

      Initialize();
    }

    // Unity hook
    void OnMouseDown() {
      if (!enabled) return;

      int mouseButtonPressed = 0;
      if (Input.GetMouseButton(1)) {
        mouseButtonPressed = 1;
      }

      DCL.Interface.WebInterface.ReportOnClickEvent(scene.sceneData.id, data.uuid, mouseButtonPressed);
    }

    void OnDestroy() {
      entity.OnComponentUpdated -= OnComponentUpdated;

      Destroy(rigidBody);

      DestroyOnClickColliders();
    }

    void DestroyOnClickColliders() {
      if (OnClickColliderGameObjects == null) return;

      for (int i = 0; i < OnClickColliderGameObjects.Length; i++) {
        Destroy(OnClickColliderGameObjects[i]);
      }
    }
  }
}
