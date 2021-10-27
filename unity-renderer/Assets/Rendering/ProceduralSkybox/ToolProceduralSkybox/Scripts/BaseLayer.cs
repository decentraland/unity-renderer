using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    [System.Serializable]
    public class TextureLayer
    {
        public LayerType layerType;
        public bool enabled;
        public string nameInEditor;
        public bool expandedInEditor;
        public float timeSpan_start;
        public float timeSpan_End;
        public float fadingInTime = 1;
        public float fadingOutTime = 1;
        public float tintercentage;
        public List<TransitioningFloat> renderDistance;

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

        public Vector3 speed_Vector3;
        public Vector2 speed_Vec2;
        public float speed;
        public float normalIntensity;

        // Particles
        public bool particleExpanded;
        //public bool useParticles;
        public Vector2 particlesRowsAndColumns = Vector2.one;
        public Vector2 particleTiling = Vector2.one;
        public Vector2 particlesOffset = Vector2.zero;
        public Vector3 particleRotation = Vector3.zero;
        // Particle Primary Param
        public float particleAnimSpeed = 1;
        public float particlesAmount = 1;
        public float particleMinSize = 1;
        public float particleMaxSize = 1;
        // Particle Secondary Param
        public float particlesHorizontalSpread = 1;
        public float particlesVerticalSpread = 1;
        public float particleMinFade = 1;
        public float particleMaxFade = 1;

        //public Vector4 particlesMainParams;
        //public Vector4 particlesSecondaryParams;
        //public float particlesSpeed;
        //public float particlesFrequency;

        // Distortion Values
        public bool distortionExpanded;
        public List<TransitioningFloat> distortIntensity;
        public List<TransitioningFloat> distortSize;
        public List<TransitioningVector2> distortSpeed;
        public List<TransitioningVector2> distortSharpness;

        public MovementType movementTypeCubemap;
        public MovementType movementTypePlanar_Radial;
        public MovementType movementTypeSatellite;

        public TextureLayer(string name = "noname")
        {
            enabled = true;
            tiling = new Vector2(1, 1);
            speed_Vec2 = new Vector2(0, 0);
            speed_Vector3 = new Vector3(0, 0, 0);
            nameInEditor = name;
            offset = new List<TransitioningVector2>();
            renderDistance = new List<TransitioningFloat>();
            rotation_float = new List<TransitioningFloat>();
            satelliteWidthHeight = new List<TransitioningVector2>();
            cubemapRotations = new List<TransitioningVector3>();
            color = new Gradient();
            movementTypeCubemap = MovementType.Speed;

            distortIntensity = new List<TransitioningFloat>();
            distortSize = new List<TransitioningFloat>();
            distortSpeed = new List<TransitioningVector2>();
            distortSharpness = new List<TransitioningVector2>();
        }
    }

    public enum LayerType
    {
        Planar = 0,
        Radial = 1,
        Satellite = 2,
        Particles = 3,
        Cubemap = 4,
    }

    public enum MovementType
    {
        Speed,
        PointBased
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