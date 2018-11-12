using UnityEngine;

[System.Serializable]
public class DecentralandScene {
  public GameObject gameObjectReference;
  public float x;
  public float y;
  public string id;
  public Vector2 basePosition; // originally called 'base' but we need to override its name as we can't name a variable 'base' in C#
  public Vector2[] parcels;
  public Data data;
  public bool _isValid;
  public bool didLoad;
  public bool justUpdated = false;

  float parcelSize = 10f;
  Vector3 auxiliaryVector3;

  public void Initialize() {
    if (gameObjectReference != null) {
      gameObjectReference.transform.position = GridToWorldPosition(x, y);

      for (int i = 0; i < gameObjectReference.transform.childCount; i++) { // Every child is a parcel land.
        gameObjectReference.transform.GetChild(i).position = GridToWorldPosition(parcels[i].x, parcels[i].y);
      }
    }
  }

  Vector3 GridToWorldPosition(float xGridPosition, float yGridPosition) {
    auxiliaryVector3.Set(xGridPosition * parcelSize, 0f, yGridPosition * parcelSize);

    return auxiliaryVector3;
  }

  public override string ToString() {
    return "gameObjectReference: " + gameObjectReference +
            "\n x: " + x +
            "\n y: " + y +
            "\n id: " + id +
            "\n base: " + basePosition +
            "\n parcels: " + parcels +
            "\n data: " + data +
            "\n isValid: " + _isValid +
            "\n didLoad: " + didLoad;
  }

  // Extra structures
  [System.Serializable]
  public class Data {
    public string owner;
    public string parcel_id;
    public Contents contents;
    public string baseUrl;
    public Scene scene;

    [System.Serializable]
    public class Contents {
      public string gamejs; // originally called 'game.js'
      public string gamets; // originally called 'game.ts'
      public string scenejson; // originally called 'scene.json'
      public string tsconfigjson; // originally called 'tsconfig.json'
    }

    [System.Serializable]
    public class Scene {
      public Display display;
      public string owner;
      public Contact contact;
      public string main;
      public string[] tags;
      public SceneParcels scene;
      public Communications communications;
      public Policy policy;

      [System.Serializable]
      public class Display {
        public string title;
        public string favicon;
      }

      [System.Serializable]
      public class Contact {
        public string name;
        public string email;
      }

      [System.Serializable]
      public class SceneParcels {
        public string[] parcels;
        public string basePosition;
      }

      [System.Serializable]
      public class Communications {
        public string type;
        public string signalling;
      }

      [System.Serializable]
      public class Policy {
        public string contentRating;
        public bool fly;
        public bool voiceEnabled;
        public string[] blacklist;
        public string teleportPosition;
      }
    }
  }
}

public class DecentralandScenesPackage {
  public DecentralandScene[] scenes;
}
