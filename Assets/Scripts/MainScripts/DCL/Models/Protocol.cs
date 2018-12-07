using System.Collections;
using System.Collections.Generic;

namespace DCL.Models {
  public enum CLASS_ID {
    TRANSFORM = 1,
    UUID_CALLBACK = 8,
    BOX_SHAPE = 16,
    SPHERE_SHAPE = 17,
    PLANE_SHAPE = 18,
    CONE_SHAPE = 19,
    CYLINDER_SHAPE = 20,
    TEXT_SHAPE = 21,
    GLTF_SHAPE = 54,
    OBJ_SHAPE = 55,
    BASIC_MATERIAL = 64,
    PBR_MATERIAL = 65
  }

  [System.Serializable]
  public class AttachEntityComponentMessage {
    /// id of the affected entity
    public string entityId;
    /// name of the compoenent
    public string name;
    /// ID of the disposable component
    public string id;
  }

  [System.Serializable]
  public class UpdateEntityComponentMessage {
    /// id of the affected entity
    public string entityId;
    /// name of the compoenent
    public string name;
    /// class of the component that should be instantiated
    public int classId;

    public string json;
  }

  [System.Serializable]
  public class SetEntityParentMessage {
    /// id of the affected entity
    public string entityId;
    /// id of the parent entity
    public string parentId;
  }

  [System.Serializable]
  public class ComponentRemovedMessage {
    /// id of the affected entity
    public string entityId;
    /// name of the compoenent
    public string name;
  }

  [System.Serializable]
  public class ComponentUpdatedMessage {
    /// ID of the disposable component
    public string id;
    public string json;
  }

  [System.Serializable]
  public class ComponentDisposedMessage {
    /// ID of the disposable component to dispose
    public string id;
  }

  [System.Serializable]
  public class ComponentCreatedMessage {
    /// ID of the disposable component
    public string id;
    /// name of the compoenent
    public string name;
    /// class of the component that should be instantiated
    public int classId;
  }
}
