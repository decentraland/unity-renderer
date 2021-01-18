using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtocolV2
{
    #region Class Declarations

    [System.Serializable]
    public class ColorRepresentation
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public ColorRepresentation(Color color)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        public Color ToColor()
        {
            return new Color(r,g,b,a);
        }
    }

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

    [System.Serializable]
    public class NFTComponent 
    {
        public string src;
        public string assetId;
        public ColorRepresentation color; 
        public int style = 0;
    }

    #endregion

    [System.Serializable]
    public class PublishSceneResultPayload
    {
        public bool ok;
        public string error;
    }

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
