using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    [System.Serializable]
    public class Planar3DConfig
    {
        public bool enabled = true;
        public string nameInEditor;
        public LayerRenderType renderType;
        public GameObject prefab;
        public float timeSpan_start = 0;
        public float timeSpan_End = 24;
        public float fadeInTime = 0;
        public float fadeOutTime = 0;
        public float satelliteSize;
        public float radius;                    // Need to initialize with particle system prefab's radius
        public float yPos = 0;
        public bool validPrefab;

        public Planar3DConfig(string name) { this.nameInEditor = name; }

        public void CheckPrefabValidity(GameObject tempPrefab)
        {
            if (tempPrefab == prefab)
            {
                return;
            }

            // Check if prefab contains particle system
            ParticleSystem particles = tempPrefab.GetComponent<ParticleSystem>();
            if (particles == null)
            {
                validPrefab = false;
            }
            else
            {
                // Check if particle system is of type circle
                var shape = particles.shape;
                if (shape.shapeType == ParticleSystemShapeType.Circle)
                {
                    validPrefab = true;
                    radius = shape.radius;
                    prefab = tempPrefab;
                }
                else
                {
                    validPrefab = false;
                }

            }
        }
    }
}