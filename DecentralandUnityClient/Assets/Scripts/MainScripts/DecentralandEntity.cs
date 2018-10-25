using UnityEngine;

[System.Serializable]
public class DecentralandEntity
{
    public string entityIdParam;
    public string parentIdParam;
    public EntityComponents entityComponents;
    public GameObject sceneObjectReference;

    [System.Serializable]
    public struct EntityComponents
    {
        public EntityPosition position;
        public EntityScale scale;
        public EntityShape shape;
        public EntityPhysics physics;

        [System.Serializable]
        public struct EntityPosition
        {
            public float x;
            public float y;
            public float z;
        }

        [System.Serializable]
        public struct EntityScale
        {
            public float x;
            public float y;
            public float z;
        }

        [System.Serializable]
        public struct EntityShape
        {
            public string tag;
        }

        [System.Serializable]
        public struct EntityPhysics
        {
            public float accelerationX;
            public float accelerationY;
            public float accelerationZ;
            public float mass;
            public bool rigid;
            public float velocityX;
            public float velocityY;
            public float velocityZ;
        }
    }
}