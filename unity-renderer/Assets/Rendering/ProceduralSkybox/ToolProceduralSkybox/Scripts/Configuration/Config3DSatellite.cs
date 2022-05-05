using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    [System.Serializable]
    public class Config3DSatellite
    {
        public bool enabled = true;
        public string nameInEditor;
        public float timeSpan_start;
        public float timeSpan_End;
        public float fadeInTime = 0;
        public float fadeOutTime = 0;

        // Satellite orbit properties
        public GameObject satellite;
        public float satelliteSize = 50;
        public float radius = 300;
        [Range(0, 360)]
        public float initialAngle = 0;
        [Range(0, 180)]
        public float horizonPlaneRotation = 0;
        [Range(0, 180)]
        public float inclination = 0;
        public float movementSpeed = 25;

        // Satellite Properties
        public RotationType satelliteRotation;
        public Vector3 fixedRotation = Vector3.zero;
        public Vector3 rotateAroundAxis = Vector3.zero;
        public float rotateSpeed = 25;
        public LayerRenderType renderType;

        public Config3DSatellite(string name = "Noname")
        {
            nameInEditor = name;
            timeSpan_start = 0;
            timeSpan_End = 24;
        }

        public Config3DSatellite DeepCopy()
        {
            Config3DSatellite satellite = (Config3DSatellite)this.MemberwiseClone();
            return satellite;
        }
    }
}