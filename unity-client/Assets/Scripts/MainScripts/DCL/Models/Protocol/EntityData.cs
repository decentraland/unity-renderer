using DCL.Models;
using System.Collections.Generic;
using UnityEngine;
using static ProtocolV2;

[System.Serializable]
public class EntityData 
{
    public string entityId;
    public TransformComponent transformComponent;

    public List<GenericComponent> components = new List<GenericComponent>();
    public List<GenericComponent> sharedComponents = new List<GenericComponent>();


    [System.Serializable]
    public class TransformComponent
    {
        public int componentId => (int)CLASS_ID_COMPONENT.TRANSFORM;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
    }

}
