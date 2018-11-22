using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Models {

  [System.Serializable]
  public class LoadParcelScenesMessage {
    public List<UnityParcelScene> parcelsToLoad;


    /**
     * This class is received as an element of the list of parcels that need to be visible
     */
    [System.Serializable]
    public class UnityParcelScene {
      public string id;

      public Vector2 basePosition;
      public Vector2[] parcels;

      public string owner;
      public List<ContentMapping> contents;
      public string baseUrl;

      [System.Serializable]
      public class ContentMapping {
        public string file;
        public string hash;
      }
    }
  }
}
