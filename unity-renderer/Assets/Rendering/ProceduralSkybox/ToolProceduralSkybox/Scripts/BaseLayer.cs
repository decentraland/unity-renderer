using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    [System.Serializable]
    public class TextureLayer
    {
        public bool disabled;
        public string nameInEditor;
        public bool expandedInEditor;
        public float timeSpan_start;
        public float timeSpan_End;
        public float fadingIn, fadingOut;
        public float tintercentage;
        public List<TransitioningFloat> renderDistance;
        public LayerType layerType;
        public Texture2D texture;
        public Texture2D textureNormal;
        public Cubemap cubemap;
        public Gradient color;
        public Vector2 tiling;

        // Offset
        public List<TransitioningVector2> offset;

        // satellite
        public List<TransitioningVector2> satelliteWidthHeight;

        // Rotations
        public List<TransitioningFloat> rotation_float;
        public List<TransitioningVector3> cubemapRotations;

        public Vector2 speed_Vec2;
        public float speed;
        public float normalIntensity;

        public TextureLayer(string name = "noname")
        {
            disabled = false;
            tiling = new Vector2(1, 1);
            speed_Vec2 = new Vector2(0, 0);
            nameInEditor = name;
            offset = new List<TransitioningVector2>();
            renderDistance = new List<TransitioningFloat>();
            rotation_float = new List<TransitioningFloat>();
            satelliteWidthHeight = new List<TransitioningVector2>();
            cubemapRotations = new List<TransitioningVector3>();
            color = new Gradient();
        }
    }

    public enum LayerType
    {
        Planar = 0,
        Radial = 1,
        Satellite = 2,
        Cubemap = 3
    }

    [System.Serializable]
    public class TransitioningFloat
    {
        public float percentage;
        public float value;

        public TransitioningFloat(float percentage, float value)
        {
            this.percentage = percentage;
            this.value = value;
        }
    }

    [System.Serializable]
    public class TransitioningVector3
    {
        public float percentage;
        public Vector3 value;

        public TransitioningVector3(float percentage, Vector3 value)
        {
            this.percentage = percentage;
            this.value = value;
        }
    }

    [System.Serializable]
    public class TransitioningVector2
    {
        public float percentage;
        public Vector2 value;

        public TransitioningVector2(float percentage, Vector2 value)
        {
            this.percentage = percentage;
            this.value = value;
        }
    }

    [System.Serializable]
    public class TransitioningVector4
    {
        public float percentage;
        public Vector4 value;

        public TransitioningVector4(float percentage, Vector4 value)
        {
            this.percentage = percentage;
            this.value = value;
        }
    }

    [System.Serializable]
    public class TransitioningQuaternion
    {
        public float percentage;
        public Quaternion value;

        public TransitioningQuaternion(float percentage, Quaternion value)
        {
            this.percentage = percentage;
            this.value = value;
        }
    }

    [System.Serializable]
    public class DirectionalLightAttributes
    {
        public Gradient lightColor;
        public List<TransitioningFloat> intensity;
        public List<TransitioningQuaternion> lightDirection;
        public Gradient tintColor;

        public DirectionalLightAttributes()
        {
            lightColor = new Gradient();
            intensity = new List<TransitioningFloat>();
            lightDirection = new List<TransitioningQuaternion>();
            tintColor = new Gradient();
        }
    }
}