using System.Collections;
using System.Collections.Generic;

namespace DCL.Models
{
  public enum CLASS_ID
  {
    TRANSFORM = 1,
    ANIMATOR = 2,
    UUID_CALLBACK = 8,
    BOX_SHAPE = 16,
    SPHERE_SHAPE = 17,
    PLANE_SHAPE = 18,
    CONE_SHAPE = 19,
    CYLINDER_SHAPE = 20,
    TEXT_SHAPE = 21,

    NFT_SHAPE = 22,
    UI_WORLD_SPACE_SHAPE = 23,
    UI_SCREEN_SPACE_SHAPE = 24,
    UI_CONTAINER_RECT = 25,
    UI_CONTAINER_STACK = 26,
    UI_TEXT_SHAPE = 27,
    UI_INPUT_TEXT_SHAPE = 28,
    UI_IMAGE_SHAPE = 29,
    UI_SLIDER_SHAPE = 30,

    GLTF_SHAPE = 54,
    OBJ_SHAPE = 55,
    BASIC_MATERIAL = 64,
    PBR_MATERIAL = 65,

    ONCLICK = 80
  }

  public class CallMethodComponentMessage
  {
    public string methodName;
    public object[] args;
  }

  [System.Serializable]
  public class AttachEntityComponentMessage
  {
    /// id of the affected entity
    public string entityId;
    /// name of the compoenent
    public string name;
    /// ID of the disposable component
    public string id;
  }

  [System.Serializable]
  public class UpdateEntityComponentMessage
  {
    /// id of the affected entity
    public string entityId;
    /// name of the compoenent
    public string name;
    /// class of the component that should be instantiated
    public int classId;

    public string json;
  }

  [System.Serializable]
  public class SetEntityParentMessage
  {
    /// id of the affected entity
    public string entityId;
    /// id of the parent entity
    public string parentId;
  }

  [System.Serializable]
  public class ComponentRemovedMessage
  {
    /// id of the affected entity
    public string entityId;
    /// name of the compoenent
    public string name;
  }

  [System.Serializable]
  public class ComponentUpdatedMessage
  {
    /// ID of the disposable component
    public string id;
    public string json;
  }

  [System.Serializable]
  public class ComponentDisposedMessage
  {
    /// ID of the disposable component to dispose
    public string id;
  }

  [System.Serializable]
  public class ComponentCreatedMessage
  {
    /// ID of the disposable component
    public string id;
    /// name of the compoenent
    public string name;
    /// class of the component that should be instantiated
    public int classId;
  }
}
