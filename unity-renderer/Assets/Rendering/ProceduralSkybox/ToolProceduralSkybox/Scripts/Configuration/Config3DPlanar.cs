using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    [System.Serializable]
    public class Config3DPlanar
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
        public float radius;
        public float yPos = 0;
        public bool followCamera;
        public bool renderWithMainCamera;
        public bool validPrefab;

        private ParticleSystem particles;
        [NonSerialized] public string inValidStr = "";

        public Config3DPlanar(string name) { this.nameInEditor = name; }

        public void AssignNewPrefab(GameObject tempPrefab)
        {
            if (tempPrefab == prefab)
            {
                return;
            }

            if (tempPrefab == null)
            {
                prefab = tempPrefab;
                return;
            }

            // Check if prefab contains particle system
            particles = tempPrefab.GetComponent<ParticleSystem>();
            if (particles == null)
            {
                validPrefab = false;
                inValidStr = "Particle system not present";
            }
            else
            {
                // Check if particle system is of type circle
                var shape = particles.shape;
                if (shape.shapeType == ParticleSystemShapeType.Circle || shape.shapeType == ParticleSystemShapeType.Hemisphere)
                {
                    validPrefab = true;
                    radius = shape.radius;
                    prefab = tempPrefab;
                    inValidStr = "valid!";
                }
                else
                {
                    validPrefab = false;
                    inValidStr = "Particle system not of type Circle or Hemisphere";
                }

            }
        }
    }
}