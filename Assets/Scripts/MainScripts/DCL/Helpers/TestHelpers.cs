using UnityEngine;
using DCL.Configuration;
using DCL.Controllers;
using Newtonsoft.Json;
using DCL.Components;

namespace DCL.Helpers {
  public static class TestHelpers {
    public static string CreateSceneMessage(string sceneId, string method, string payload) {
      return $"{sceneId}\t{method}\t{payload}\n";
    }

    public static void InstantiateEntityWithShape(ParcelScene scene, string entityId, DCL.Models.CLASS_ID classId, Vector3 position, string remoteSrc = "") {
      scene.CreateEntity(entityId);

      scene.UpdateEntityComponent(JsonUtility.ToJson(new DCL.Models.UpdateEntityComponentMessage {
        entityId = entityId,
        name = "shape",
        classId = (int)classId,
        json = JsonConvert.SerializeObject(new {
          src = remoteSrc
        })
      }));

      scene.UpdateEntityComponent(JsonUtility.ToJson(new DCL.Models.UpdateEntityComponentMessage {
        entityId = entityId,
        name = "transform",
        classId = (int)DCL.Models.CLASS_ID.TRANSFORM,
        json = JsonConvert.SerializeObject(new {
          position = position,
          scale = new Vector3(1, 1, 1),
          rotation = new {
            x = 0,
            y = 0,
            z = 0,
            w = 1
          }
        })
      }));
    }

    public static void InstantiateEntityWithMaterial(ParcelScene scene, string entityId, Vector3 position, BasicMaterial.Model basicMaterial, string materialComponentID = "a-material") {
      InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, position);

      scene.ComponentCreated(JsonUtility.ToJson(new DCL.Models.ComponentCreatedMessage {
        classId = (int)DCL.Models.CLASS_ID.BASIC_MATERIAL,
        id = materialComponentID,
        name = "material"
      }));

      scene.ComponentUpdated(JsonUtility.ToJson(new DCL.Models.ComponentUpdatedMessage {
        id = materialComponentID,
        json = JsonUtility.ToJson(basicMaterial)
      }));

      scene.AttachEntityComponent(JsonUtility.ToJson(new DCL.Models.AttachEntityComponentMessage {
        entityId = entityId,
        id = materialComponentID,
        name = "material"
      }));
    }

    public static void InstantiateEntityWithMaterial(ParcelScene scene, string entityId, Vector3 position, PBRMaterial.Model pbrMaterial, string materialComponentID = "a-material") {
      InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, position);

      scene.ComponentCreated(JsonUtility.ToJson(new DCL.Models.ComponentCreatedMessage {
        classId = (int)DCL.Models.CLASS_ID.PBR_MATERIAL,
        id = materialComponentID,
        name = "material"
      }));

      scene.ComponentUpdated(JsonUtility.ToJson(new DCL.Models.ComponentUpdatedMessage {
        id = materialComponentID,
        json = JsonUtility.ToJson(pbrMaterial)
      }));

      scene.AttachEntityComponent(JsonUtility.ToJson(new DCL.Models.AttachEntityComponentMessage {
        entityId = entityId,
        id = materialComponentID,
        name = "material"
      }));
    }
  }
}
