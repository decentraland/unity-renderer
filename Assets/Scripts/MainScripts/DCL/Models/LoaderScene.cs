using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Models {
  /**
   * This class is received as an element of the list of parcels that need to be visible
   */
  public struct LoaderScene {
    public string id;
    public int x;
    public int y;

    [JsonProperty("base")]
    public Vector2 basePosition;

    public Vector2[] parcels;

    public WorkerSceneData data;

    public override string ToString() {
      return
              " x: " + x +
              "\n y: " + y +
              "\n id: " + id +
              "\n base: " + basePosition +
              "\n parcels: " + parcels +
              "\n data: " + data;
    }

    /**
     * This class is sended by the EstateManager
     */
    public struct WorkerSceneData {
      public string owner;
      public string parcel_id;
      public Dictionary<string, string> contents;
      public string baseUrl;
    }
  }

}
