using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using DCL.Helpers;
using DCL.Models;
using DCL.Configuration;

namespace DCL.Controllers {
  public class ParcelScene : MonoBehaviour {
    public Dictionary<string, DecentralandEntity> entities = new Dictionary<string, DecentralandEntity>();

    public LoadParcelScenesMessage.UnityParcelScene sceneData { get; private set; }

    public void SetData(LoadParcelScenesMessage.UnityParcelScene data) {
      this.sceneData = data;

      this.name = gameObject.name = $"scene:{data.id}";

      gameObject.transform.position = LandHelpers.GridToWorldPosition(data.basePosition.x, data.basePosition.y);

      if (Environment.DEBUG) {
        for (int j = 0; j < data.parcels.Length; j++) {
          var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);

          Object.Destroy(plane.GetComponent<MeshCollider>());

          var renderer = plane.GetComponent<MeshRenderer>();
          // renderer.material = material;
          if (renderer.material != null) {
            renderer.material.renderQueue = 3001;
          }

          plane.name = $"parcel:{data.parcels[j].x},{data.parcels[j].y}";
          plane.transform.SetParent(gameObject.transform);

          var position = LandHelpers.GridToWorldPosition(data.parcels[j].x, data.parcels[j].y);
          // SET TO A POSITION RELATIVE TO basePosition

          position.Set(position.x + ParcelSettings.PARCEL_SIZE / 2, ParcelSettings.DEBUG_FLOOR_HEIGHT, position.z + ParcelSettings.PARCEL_SIZE / 2);

          plane.transform.position = position;
        }
      }
    }

    public override string ToString() {
      return "gameObjectReference: " + this.ToString() + "\n" + sceneData.ToString();
    }

    internal void Dispose() {
      RemoveAllEntities();
      Object.Destroy(this);
    }

    public void CreateEntity(string entityID) {
      if (!entities.ContainsKey(entityID)) {
        var newEntity = new DecentralandEntity();
        newEntity.components = new DecentralandEntity.EntityComponents();
        newEntity.entityId = entityID;
        newEntity.gameObject = new GameObject();
        newEntity.gameObject.transform.SetParent(gameObject.transform);

        entities.Add(entityID, newEntity);
      } else {
        Debug.Log("Couldn't create entity with ID: " + entityID + " as it already exists.");
      }
    }

    public void RemoveEntity(string entityID) {
      if (entities.ContainsKey(entityID)) {
        Object.Destroy(entities[entityID].gameObject);
        entities.Remove(entityID);
      } else {
        Debug.Log("Couldn't remove entity with ID: " + entityID + " as it doesn't exist.");
      }
    }

    public void RemoveAllEntities() {
      var list = entities.ToArray();
      for (int i = 0; i < list.Length; i++) {
        RemoveEntity(list[i].Key);
      }
    }

    private SetEntityParentMessage tmpParentMessage = new SetEntityParentMessage();

    public void SetEntityParent(string RawJSONParams) {
      JsonUtility.FromJsonOverwrite(RawJSONParams, tmpParentMessage);

      if (tmpParentMessage.entityId != tmpParentMessage.parentId) {
        GameObject rootGameObject = null;

        if (tmpParentMessage.parentId == "0") {
          rootGameObject = gameObject;
        } else {
          DecentralandEntity decentralandEntity = GetEntityForUpdate(tmpParentMessage.parentId);
          if (decentralandEntity != null) {
            rootGameObject = decentralandEntity.gameObject;
          }
        }

        if (rootGameObject != null) {
          DecentralandEntity decentralandEntity = GetEntityForUpdate(tmpParentMessage.entityId);

          if (decentralandEntity != null) {
            decentralandEntity.gameObject.transform.SetParent(rootGameObject.transform);
          }
        }
      }
    }

    [System.Obsolete]
    public void UpdateEntity(string RawJSONParams) {
      DecentralandEntity parsedEntity = JsonUtility.FromJson<DecentralandEntity>(RawJSONParams);

      DecentralandEntity decentralandEntity = GetEntityForUpdate(parsedEntity.entityId);

      if (decentralandEntity != null) {
        if (parsedEntity.components != null) {
          // Update entity transform data
          if (parsedEntity.components.transform != null) {
            decentralandEntity.components.transform = parsedEntity.components?.transform;
          }

          // Update entity shape data. We check a shape property instead of shape itself for null as the JSONUtility removes the null of every component once one has been parsed.
          // TODO: Find a way to avoid the shape being initialized when a different component is parsed from the JSON.
          if (!string.IsNullOrEmpty(parsedEntity.components.shape.tag)) {
            // TODO: Detect changes in shape.
            if (decentralandEntity.components.shape == null) {
              // First time shape instantiation
              ShapeComponentHelpers.IntializeDecentralandEntityRenderer(decentralandEntity, parsedEntity.components.shape);
            }

            decentralandEntity.components.shape = parsedEntity.components.shape;
          }
        }

        decentralandEntity.components?.transform?.ApplyTo(decentralandEntity.gameObject);
      } else {
        Debug.Log("Couldn't update entity " + parsedEntity.entityId + " because that entity doesn't exist.");
      }
    }

    /**
      * This method is called when we need to attach a disposable component to the entity
      */
    public void AttachEntityComponent(string json) {
      AttachEntityComponentMessage parsedJson = JsonUtility.FromJson<AttachEntityComponentMessage>(json);

      DecentralandEntity decentralandEntity = GetEntityForUpdate(parsedJson.entityId);
      if (decentralandEntity == null) return;
    }

    public void UpdateEntityComponent(string json) {
      UpdateEntityComponentMessage parsedJson = JsonUtility.FromJson<UpdateEntityComponentMessage>(json);

      DecentralandEntity decentralandEntity = GetEntityForUpdate(parsedJson.entityId);
      if (decentralandEntity == null) return;

      if (parsedJson.name == "transform") {
        DecentralandEntity.EntityTransform parsedTransform = JsonUtility.FromJson<DecentralandEntity.EntityTransform>(parsedJson.json);
        parsedTransform.ApplyTo(decentralandEntity.gameObject);
      } else if (parsedJson.name == "shape") {
        DecentralandEntity.EntityShape newShape = JsonUtility.FromJson<DecentralandEntity.EntityShape>(parsedJson.json);
        ShapeComponentHelpers.IntializeDecentralandEntityRenderer(decentralandEntity, newShape);
      }
    }

    public void ComponentAdded(string json) {
      ComponentAddedMessage parsedJson = JsonUtility.FromJson<ComponentAddedMessage>(json);

      DecentralandEntity decentralandEntity = GetEntityForUpdate(parsedJson.entityId);
      if (decentralandEntity == null) return;

    }

    public void ComponentCreated(string json) {
      ComponentCreatedMessage parsedJson = JsonUtility.FromJson<ComponentCreatedMessage>(json);
    }

    public void ComponentDisposed(string componentId) {
      // componentId
    }

    public void ComponentRemoved(string json) {
      ComponentRemovedMessage parsedJson = JsonUtility.FromJson<ComponentRemovedMessage>(json);

      DecentralandEntity decentralandEntity = GetEntityForUpdate(parsedJson.entityId);
      if (decentralandEntity == null) return;

      if (parsedJson.name == "transform") {
        decentralandEntity.gameObject.transform.localPosition = Vector3.zero;
        decentralandEntity.gameObject.transform.localScale = Vector3.one;
        decentralandEntity.gameObject.transform.localRotation = Quaternion.identity;
      }
    }

    public void ComponentUpdated(string json) {
      ComponentUpdatedMessage parsedJson = JsonUtility.FromJson<ComponentUpdatedMessage>(json);
    }

    private DecentralandEntity GetEntityForUpdate(string entityId) {
      if (string.IsNullOrEmpty(entityId)) {
        Debug.LogError("Null or empty entityId");
        return null;
      }

      DecentralandEntity decentralandEntity;

      if (!entities.TryGetValue(entityId, out decentralandEntity) || decentralandEntity == null || decentralandEntity.gameObject == null) {
        return null;
      }

      return decentralandEntity;
    }
  }

  [System.Serializable]
  public class AttachEntityComponentMessage {
    public string entityId;
    /** name of the compoenent */
    public string name;
    public string componentId;
  }

  [System.Serializable]
  public class UpdateEntityComponentMessage {
    public string entityId;
    /** name of the compoenent */
    public string name;
    public string json;
  }

  [System.Serializable]
  public class SetEntityParentMessage {
    public string entityId;
    public string parentId;
  }

  [System.Serializable]
  public class ComponentRemovedMessage {
    public string entityId;
    /** name of the compoenent */
    public string name;
  }

  [System.Serializable]
  public class ComponentAddedMessage {
    public string entityId;
    /** name of the compoenent */
    public string name;
    public string json;
  }

  [System.Serializable]
  public class ComponentUpdatedMessage {
    public string componentId;
    public string json;
  }

  [System.Serializable]
  public class ComponentCreatedMessage {
    public string componentId;
    /** name of the compoenent */
    public string name;
  }
}
