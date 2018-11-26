using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using DCL.Helpers;
using DCL.Models;
using DCL.Configuration;

namespace DCL.Controllers {
  public class ParcelScene {
    public GameObject rootGameObject;

    public Dictionary<string, DecentralandEntity> entities = new Dictionary<string, DecentralandEntity>();

    public LoadParcelScenesMessage.UnityParcelScene sceneData { get; }

    public ParcelScene(LoadParcelScenesMessage.UnityParcelScene data) {
      this.sceneData = data;

      rootGameObject = new GameObject();
      rootGameObject.name = $"scene:{data.id}";
      rootGameObject.transform.position = LandHelpers.GridToWorldPosition(data.basePosition.x, data.basePosition.y);

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
          plane.transform.SetParent(rootGameObject.transform);

          var position = LandHelpers.GridToWorldPosition(
            // SET TO A POSITION RELATIVE TO basePosition
            data.parcels[j].x - data.basePosition.x,
            data.parcels[j].y - data.basePosition.y
          );

          position.y += 0.01f;

          plane.transform.position = position;
        }
      }
    }

    public override string ToString() {
      return "gameObjectReference: " + rootGameObject + "\n" + sceneData.ToString();
    }

    internal void Dispose() {
      RemoveAllEntities();
      Object.Destroy(rootGameObject);
    }

    public void CreateEntity(string entityID) {
      if (!entities.ContainsKey(entityID)) {

        var newEntity = new DecentralandEntity();
        newEntity.components = new DecentralandEntity.EntityComponents();
        newEntity.id = entityID;
        newEntity.gameObjectReference = new GameObject();
        newEntity.gameObjectReference.name = entityID;

        newEntity.gameObjectReference.transform.SetParent(rootGameObject.transform);

        entities.Add(entityID, newEntity);
      } else {
        Debug.Log("Couldn't create entity with ID: " + entityID + " as it already exists.");
      }
    }

    public void RemoveEntity(string entityID) {
      if (entities.ContainsKey(entityID)) {
        Object.Destroy(entities[entityID].gameObjectReference);
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

    public void SetEntityParent(string RawJSONParams) {
      // TODO: Root entity
      DecentralandEntity auxiliaryDecentralandEntity = JsonUtility.FromJson<DecentralandEntity>(RawJSONParams);

      if (auxiliaryDecentralandEntity.id != auxiliaryDecentralandEntity.parentId) {
        DecentralandEntity parentDecentralandEntity;

        entities.TryGetValue(auxiliaryDecentralandEntity.parentId, out parentDecentralandEntity);

        if (parentDecentralandEntity != null) {
          DecentralandEntity decentralandEntity;

          entities.TryGetValue(auxiliaryDecentralandEntity.id, out decentralandEntity);

          if (decentralandEntity != null) {
            decentralandEntity.gameObjectReference.transform.SetParent(parentDecentralandEntity.gameObjectReference.transform);
          } else {
            Debug.Log("Couldn't enparent entity " + auxiliaryDecentralandEntity.id + " because that entity doesn't exist.");
          }
        } else {
          Debug.Log("Couldn't enparent entity " + auxiliaryDecentralandEntity.id + " because the parent (id " + auxiliaryDecentralandEntity.parentId + ") doesn't exist");
        }
      } else {
        Debug.Log("Couldn't enparent entity " + auxiliaryDecentralandEntity.id + " because the configured parent id is its own id.");
      }
    }

    public void UpdateEntity(string RawJSONParams) {
      DecentralandEntity decentralandEntity;

      DecentralandEntity parsedEntity = JsonUtility.FromJson<DecentralandEntity>(RawJSONParams);

      entities.TryGetValue(parsedEntity.id, out decentralandEntity);

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
            if (decentralandEntity.components.shape == null) { // First time shape instantiation
              var shapeComponent = ShapeComponentHelpers.IntializeDecentralandEntityRenderer(decentralandEntity, parsedEntity);
            }

            decentralandEntity.components.shape = parsedEntity.components.shape;
          }
        }

        decentralandEntity.UpdateGameObjectComponents();
      } else {
        Debug.Log("Couldn't update entity " + parsedEntity.id + " because that entity doesn't exist.");
      }
    }
  }
}
