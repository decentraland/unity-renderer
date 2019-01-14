using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using DCL.Helpers;
using DCL.Models;
using DCL.Configuration;
using DCL.Components;

namespace DCL.Controllers
{
  public class ParcelScene : MonoBehaviour
  {
    public Dictionary<string, DecentralandEntity> entities = new Dictionary<string, DecentralandEntity>();
    public Dictionary<string, BaseDisposable> disposableComponents = new Dictionary<string, BaseDisposable>();

    public LoadParcelScenesMessage.UnityParcelScene sceneData { get; private set; }

    public void SetData(LoadParcelScenesMessage.UnityParcelScene data)
    {
      this.sceneData = data;

      this.name = gameObject.name = $"scene:{data.id}";

      gameObject.transform.position = LandHelpers.GridToWorldPosition(data.basePosition.x, data.basePosition.y);

      if (Environment.DEBUG)
      {
        for (int j = 0; j < data.parcels.Length; j++)
        {
          var plane = GameObject.CreatePrimitive(PrimitiveType.Plane);

          Object.Destroy(plane.GetComponent<MeshCollider>());

          plane.name = $"parcel:{data.parcels[j].x},{data.parcels[j].y}";
          plane.transform.SetParent(gameObject.transform);

          var position = LandHelpers.GridToWorldPosition(data.parcels[j].x, data.parcels[j].y);
          // SET TO A POSITION RELATIVE TO basePosition

          position.Set(position.x + ParcelSettings.PARCEL_SIZE / 2, ParcelSettings.DEBUG_FLOOR_HEIGHT, position.z + ParcelSettings.PARCEL_SIZE / 2);

          plane.transform.position = position;
        }
      }
    }

    void OnDestroy()
    {
      foreach (var entity in entities)
      {
        Destroy(entity.Value.gameObject);
      }
      entities.Clear();
    }

    public override string ToString()
    {
      return "gameObjectReference: " + this.ToString() + "\n" + sceneData.ToString();
    }

    public void CreateEntity(string entityID)
    {
      if (!entities.ContainsKey(entityID))
      {
        var newEntity = new DecentralandEntity();
        newEntity.entityId = entityID;
        newEntity.gameObject = new GameObject();
        newEntity.gameObject.transform.SetParent(gameObject.transform);
        newEntity.gameObject.name = "ENTITY_" + entityID;

        entities.Add(entityID, newEntity);
      }
      else
      {
        Debug.Log("Couldn't create entity with ID: " + entityID + " as it already exists.");
      }
    }

    public void RemoveEntity(string entityID)
    {
      if (entities.ContainsKey(entityID))
      {
        Object.Destroy(entities[entityID].gameObject);
        entities.Remove(entityID);
      }
      else
      {
        Debug.Log("Couldn't remove entity with ID: " + entityID + " as it doesn't exist.");
      }
    }

    public void RemoveAllEntities()
    {
      var list = entities.ToArray();
      for (int i = 0; i < list.Length; i++)
      {
        RemoveEntity(list[i].Key);
      }
    }

    private SetEntityParentMessage tmpParentMessage = new SetEntityParentMessage();

    public void SetEntityParent(string RawJSONParams)
    {
      JsonUtility.FromJsonOverwrite(RawJSONParams, tmpParentMessage);

      if (tmpParentMessage.entityId != tmpParentMessage.parentId)
      {
        GameObject rootGameObject = null;

        if (tmpParentMessage.parentId == "0")
        {
          rootGameObject = gameObject;
        }
        else
        {
          DecentralandEntity decentralandEntity = GetEntityForUpdate(tmpParentMessage.parentId);
          if (decentralandEntity != null)
          {
            rootGameObject = decentralandEntity.gameObject;
          }
        }

        if (rootGameObject != null)
        {
          DecentralandEntity decentralandEntity = GetEntityForUpdate(tmpParentMessage.entityId);

          if (decentralandEntity != null)
          {
            decentralandEntity.gameObject.transform.SetParent(rootGameObject.transform);
          }
        }
      }
    }

    /**
      * This method is called when we need to attach a disposable component to the entity
      */
    public void AttachEntityComponent(string json)
    {
      var parsedJson = JsonUtility.FromJson<AttachEntityComponentMessage>(json);

      DecentralandEntity decentralandEntity = GetEntityForUpdate(parsedJson.entityId);

      if (decentralandEntity == null) return;

      BaseDisposable disposableComponent;

      if (disposableComponents.TryGetValue(parsedJson.id, out disposableComponent) && disposableComponent != null)
      {
        disposableComponent.AttachTo(decentralandEntity);
      }
    }

    public void UpdateEntityComponent(string json)
    {
      var parsedJson = JsonUtility.FromJson<UpdateEntityComponentMessage>(json);

      DecentralandEntity decentralandEntity = GetEntityForUpdate(parsedJson.entityId);
      if (decentralandEntity == null) return;

      switch ((CLASS_ID)parsedJson.classId)
      {
        case CLASS_ID.TRANSFORM:
          {
            var component = LandHelpers.GetOrCreateComponent<DCL.Components.DCLTransform>(decentralandEntity.gameObject);
            component.scene = this;
            component.entity = decentralandEntity;
            component.UpdateFromJSON(parsedJson.json);
            break;
          }
        case CLASS_ID.BOX_SHAPE:
          {
            var component = LandHelpers.GetOrCreateComponent<DCL.Components.BoxShape>(decentralandEntity.gameObject);
            component.scene = this;
            component.entity = decentralandEntity;
            component.UpdateFromJSON(parsedJson.json);
            break;
          }
        case CLASS_ID.SPHERE_SHAPE:
          {
            var component = LandHelpers.GetOrCreateComponent<DCL.Components.SphereShape>(decentralandEntity.gameObject);
            component.scene = this;
            component.entity = decentralandEntity;
            component.UpdateFromJSON(parsedJson.json);
            break;
          }
        case CLASS_ID.CONE_SHAPE:
          {
            var component = LandHelpers.GetOrCreateComponent<DCL.Components.ConeShape>(decentralandEntity.gameObject);
            component.scene = this;
            component.entity = decentralandEntity;
            component.UpdateFromJSON(parsedJson.json);
            break;
          }
        case CLASS_ID.CYLINDER_SHAPE:
          {
            var component = LandHelpers.GetOrCreateComponent<DCL.Components.CylinderShape>(decentralandEntity.gameObject);
            component.scene = this;
            component.entity = decentralandEntity;
            component.UpdateFromJSON(parsedJson.json);
            break;
          }
        case CLASS_ID.PLANE_SHAPE:
          {
            var component = LandHelpers.GetOrCreateComponent<DCL.Components.PlaneShape>(decentralandEntity.gameObject);
            component.scene = this;
            component.entity = decentralandEntity;
            component.UpdateFromJSON(parsedJson.json);
            break;
          }
        case CLASS_ID.GLTF_SHAPE:
          {
            var component = LandHelpers.GetOrCreateComponent<DCL.Components.GLTFShape>(decentralandEntity.gameObject);
            component.scene = this;
            component.entity = decentralandEntity;
            component.UpdateFromJSON(parsedJson.json);
            break;
          }
        case CLASS_ID.OBJ_SHAPE:
          {
            var component = LandHelpers.GetOrCreateComponent<DCL.Components.OBJShape>(decentralandEntity.gameObject);
            component.scene = this;
            component.entity = decentralandEntity;
            component.UpdateFromJSON(parsedJson.json);
            break;
          }
        case CLASS_ID.ANIMATOR:
          {
            var component = LandHelpers.GetOrCreateComponent<DCL.Components.DCLAnimator>(decentralandEntity.gameObject);
            component.scene = this;
            component.entity = decentralandEntity;
            component.UpdateFromJSON(parsedJson.json);
            break;
          }
        case CLASS_ID.ONCLICK:
          {
            var component = LandHelpers.GetOrCreateComponent<DCL.Components.OnClick>(decentralandEntity.gameObject);
            component.scene = this;
            component.entity = decentralandEntity;
            break;
          }
        default:
#if UNITY_EDITOR
          throw new UnityException($"Unknown classId {json}");
#else
          break;
#endif
      }
    }

    public void ComponentCreated(string json)
    {
      var parsedJson = JsonUtility.FromJson<ComponentCreatedMessage>(json);

      BaseDisposable disposableComponent;
      if (disposableComponents.TryGetValue(parsedJson.id, out disposableComponent))
      {
        disposableComponents.Remove(parsedJson.id);
      }

      switch ((CLASS_ID)parsedJson.classId)
      {
        case CLASS_ID.BASIC_MATERIAL:
          {
            disposableComponents.Add(parsedJson.id, new DCL.Components.BasicMaterial(this));
            break;
          }
        case CLASS_ID.PBR_MATERIAL:
          {
            disposableComponents.Add(parsedJson.id, new DCL.Components.PBRMaterial(this));
            break;
          }
        default:
#if UNITY_EDITOR
          throw new UnityException($"Unknown classId {json}");
#else
          // Ignore errors outside of the editor
          break;
#endif
      }
    }

    public void ComponentDisposed(string json)
    {
      var parsedJson = JsonUtility.FromJson<ComponentDisposedMessage>(json);

      BaseDisposable disposableComponent;

      if (disposableComponents.TryGetValue(parsedJson.id, out disposableComponent))
      {
        if (disposableComponent != null)
        {
          disposableComponent.Dispose();
        }

        disposableComponents.Remove(parsedJson.id);
      }
    }

    public void ComponentRemoved(string json)
    {
      var parsedJson = JsonUtility.FromJson<ComponentRemovedMessage>(json);

      DecentralandEntity decentralandEntity = GetEntityForUpdate(parsedJson.entityId);
      if (decentralandEntity == null) return;

      var components = decentralandEntity.gameObject.GetComponents<DCL.Components.UpdateableComponent>();

      for (int i = 0; i < components.Length; i++)
      {
        if (components[i] && components[i].componentName == parsedJson.name)
        {
#if UNITY_EDITOR
          DestroyImmediate(components[i]);
#else
          Destroy(components[i]);
#endif
        }
      }
    }

    public void ComponentUpdated(string json)
    {
      var parsedJson = JsonUtility.FromJson<ComponentUpdatedMessage>(json);

      BaseDisposable disposableComponent;

      if (disposableComponents.TryGetValue(parsedJson.id, out disposableComponent) && disposableComponent != null)
      {
        disposableComponent.UpdateFromJSON(parsedJson.json);
      }
    }

    private DecentralandEntity GetEntityForUpdate(string entityId)
    {
      if (string.IsNullOrEmpty(entityId))
      {
        Debug.LogError("Null or empty entityId");
        return null;
      }

      DecentralandEntity decentralandEntity;

      if (!entities.TryGetValue(entityId, out decentralandEntity))
      {
        return null;
      }

      if (decentralandEntity == null || decentralandEntity.gameObject == null)
      {
        entities.Remove(entityId);
        return null;
      }

      return decentralandEntity;
    }
  }
}
