using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtocolV2 
{
    #region Class Declarations

    [System.Serializable]
    public class QuaternionRepresentation
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public QuaternionRepresentation(Quaternion quaternion)
        {

            x = quaternion.x;
            y = quaternion.y;
            z = quaternion.z;
            w = quaternion.w;
        }
    }

    #region Components

    [System.Serializable]
    public class GenericComponent
    {
        public int componentId;
        public string classId;
        public object data;
    }

    [System.Serializable]
    public class TransformComponent
    {
        public Vector3 position;

        public QuaternionRepresentation rotation;

        public Vector3 scale;

    }

    #endregion

    [System.Serializable]
    public class EntityPayload
    {
        public string entityId;
        public ComponentPayload[] components;
    }

    [System.Serializable]
    public class ComponentPayload
    {
        public int componentId;
        public object data;
    }

    [System.Serializable]
    public class EntitySingleComponentPayload
    {
        public string entityId;
        public int componentId;
        public object data;
    }

    [System.Serializable]
    public class RemoveEntityPayload
    {
        public string entityId;
    }

    [System.Serializable]
    public class RemoveEntityComponentsPayload
    {
        public string entityId;
        public string componentId;
    }

    [System.Serializable]
    public class AddEntityEvent
    {
        public string type = "AddEntity";
        public EntityPayload payload;
    }

    [System.Serializable]
    public class ModifyEntityComponentEvent
    {
        public string type = "SetComponent";
        public EntitySingleComponentPayload payload;
    }

    [System.Serializable]
    public class RemoveEntityEvent
    {
        public string type = "RemoveEntity";
        public RemoveEntityPayload payload;
    }

    [System.Serializable]
    public class RemoveEntityComponentsEvent
    {
        public string type = "RemoveComponent";
        public RemoveEntityComponentsPayload payload;
    }

    [System.Serializable]
    public class StoreSceneStateEvent
    {
        public string type = "StoreSceneState";
        public string payload = "";
    }

    #endregion
}
