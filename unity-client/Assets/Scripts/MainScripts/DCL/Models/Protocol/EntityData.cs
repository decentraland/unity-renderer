using DCL.Models;
using System.Collections.Generic;
using UnityEngine;
using static ProtocolV2;

[System.Serializable]
public class EntityData 
{
    public string entityId;
    public TransformComponent transformComponent;
    public NFTComponent nftComponent;

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

    [System.Serializable]
    public class NFTComponent
    {
        public int componentId => (int)CLASS_ID.NFT_SHAPE;
        public string id;
        public string src;
        public string assetId;
        public ColorRepresentation color;
        public int style = 0;
    }

}
