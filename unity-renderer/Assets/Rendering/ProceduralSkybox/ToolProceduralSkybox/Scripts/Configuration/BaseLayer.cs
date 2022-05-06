using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace DCL.Skybox
{
    [System.Serializable]
    public class TextureLayer
    {
        public int slotID = 0;                              // Slot ID
        public LayerRenderType renderType;
        public LayerType layerType;
        public bool enabled;
        public string nameInEditor;
        public bool expandedInEditor;
        public float timeSpan_start;
        public float timeSpan_End;
        public float fadingInTime = 1;
        public float fadingOutTime = 1;

        [FormerlySerializedAs("tintercentage")]
        public float tintPercentage;
        public List<TransitioningFloat> renderDistance;

        public Texture2D texture;
        public Cubemap cubemap;
        public Gradient color;
        public Vector2 tiling;

        [FormerlySerializedAs("flipBookRowsAndColumns")]
        public Vector2 flipbookRowsAndColumns = Vector2.one;
        public float flipbookAnimSpeed = 1;

        // Offset
        public List<TransitioningVector2> offset;

        // satellite
        public List<TransitioningVector2> satelliteWidthHeight;

        // Rotations
        [FormerlySerializedAs("rotation_float")]
        public List<TransitioningFloat> rotations_float;
        [FormerlySerializedAs("cubemapRotations")]
        public List<TransitioningVector3> rotations_Vector3;

        public Vector3 speed_Vector3;
        [FormerlySerializedAs("speed_Vec2")]
        public Vector2 speed_Vector2;
        [FormerlySerializedAs("speed")]
        public float speed_float;

        // Particles
        public bool particleExpanded;
        public Vector2 particleTiling = Vector2.one;
        public Vector2 particlesOffset = Vector2.zero;
        public List<TransitioningVector3> particleRotation = new List<TransitioningVector3>();
        // Particle Primary Param
        public float particlesAmount = 1;
        public float particleMinSize = 1;
        public float particleMaxSize = 1;
        // Particle Secondary Param
        public float particlesHorizontalSpread = 1;
        public float particlesVerticalSpread = 1;
        public float particleMinFade = 1;
        public float particleMaxFade = 1;

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
            speed_Vector2 = new Vector2(0, 0);
            speed_Vector3 = new Vector3(0, 0, 0);
            nameInEditor = name;
            offset = new List<TransitioningVector2>();
            renderDistance = new List<TransitioningFloat>();
            rotations_float = new List<TransitioningFloat>();
            satelliteWidthHeight = new List<TransitioningVector2>();
            rotations_Vector3 = new List<TransitioningVector3>();
            color = new Gradient();
            movementTypeCubemap = MovementType.Speed;

            distortIntensity = new List<TransitioningFloat>();
            distortSize = new List<TransitioningFloat>();
            distortSpeed = new List<TransitioningVector2>();
            distortSharpness = new List<TransitioningVector2>();

            particleRotation = new List<TransitioningVector3>();
        }

        // Copy details of present layer on the passed layer
        public TextureLayer DeepCopy()
        {
            TextureLayer layer = (TextureLayer)this.MemberwiseClone();

            layer.color = this.color.GetCopy();

            layer.renderDistance = new List<TransitioningFloat>();
            for (int i = 0; i < this.renderDistance.Count; i++)
            {
                layer.renderDistance.Add(this.renderDistance[i].GetCopy());
            }

            layer.offset = new List<TransitioningVector2>();
            for (int i = 0; i < this.offset.Count; i++)
            {
                layer.offset.Add(this.offset[i].GetCopy());
            }

            layer.satelliteWidthHeight = new List<TransitioningVector2>(this.satelliteWidthHeight);
            for (int i = 0; i < this.satelliteWidthHeight.Count; i++)
            {
                layer.satelliteWidthHeight.Add(this.satelliteWidthHeight[i].GetCopy());
            }

            layer.rotations_float = new List<TransitioningFloat>();
            for (int i = 0; i < this.rotations_float.Count; i++)
            {
                layer.rotations_float.Add(this.rotations_float[i].GetCopy());
            }

            layer.rotations_Vector3 = new List<TransitioningVector3>();
            for (int i = 0; i < this.rotations_Vector3.Count; i++)
            {
                layer.rotations_Vector3.Add(this.rotations_Vector3[i].GetCopy());
            }

            layer.particleRotation = new List<TransitioningVector3>();
            for (int i = 0; i < this.particleRotation.Count; i++)
            {
                layer.particleRotation.Add(this.particleRotation[i].GetCopy());
            }

            layer.distortIntensity = new List<TransitioningFloat>();
            for (int i = 0; i < this.distortIntensity.Count; i++)
            {
                layer.distortIntensity.Add(this.distortIntensity[i].GetCopy());
            }

            layer.distortSize = new List<TransitioningFloat>();
            for (int i = 0; i < this.distortSize.Count; i++)
            {
                layer.distortSize.Add(this.distortSize[i].GetCopy());
            }

            layer.distortSpeed = new List<TransitioningVector2>();
            for (int i = 0; i < this.distortSpeed.Count; i++)
            {
                layer.distortSpeed.Add(this.distortSpeed[i].GetCopy());
            }

            layer.distortSharpness = new List<TransitioningVector2>();
            for (int i = 0; i < this.distortSharpness.Count; i++)
            {
                layer.distortSharpness.Add(this.distortSharpness[i].GetCopy());
            }

            return layer;
        }

    }

    static class ExtensionMethods
    {
        public static Gradient GetCopy(this Gradient gradient)
        {
            Gradient obj = new Gradient();

            GradientColorKey[] colorKeys = new GradientColorKey[gradient.colorKeys.Length];
            for (int i = 0; i < gradient.colorKeys.Length; i++)
            {
                colorKeys[i] = gradient.colorKeys[i];
            }

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[gradient.alphaKeys.Length];
            for (int i = 0; i < gradient.alphaKeys.Length; i++)
            {
                alphaKeys[i] = gradient.alphaKeys[i];
            }
            obj.SetKeys(colorKeys, alphaKeys);
            return obj;
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

    public enum LayerRenderType
    {
        Rendering,
        NotRendering,
        Conflict_Playing,
        Conflict_NotPlaying
    }

    public enum MovementType
    {
        Speed,
        PointBased
    }

    public enum RotationType
    {
        Fixed,
        Rotate,
        LookAtOrbit
    }

    [System.Serializable]
    public class TimelineTagsDuration
    {
        public string tag;
        public float startTime = 0;
        public float endTime = 0;
        public bool isTrigger = true;              // If true, this event is trigger event
        [System.NonSerialized]
        public bool startEventExecuted;             // Also works for trigger events

        public TimelineTagsDuration(float startTime, string tag = "", bool isTrigger = true)
        {
            this.tag = tag;
            this.startTime = startTime;
            this.isTrigger = isTrigger;
        }
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

        internal TransitioningFloat GetCopy() { return (TransitioningFloat)this.MemberwiseClone(); }
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

        internal TransitioningVector3 GetCopy() { return (TransitioningVector3)this.MemberwiseClone(); }
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

        internal TransitioningVector2 GetCopy() { return (TransitioningVector2)this.MemberwiseClone(); }
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

        internal TransitioningVector4 GetCopy() { return new TransitioningVector4(this.percentage, this.value); }
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

        internal TransitioningQuaternion GetCopy() { return new TransitioningQuaternion(this.percentage, this.value); }
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