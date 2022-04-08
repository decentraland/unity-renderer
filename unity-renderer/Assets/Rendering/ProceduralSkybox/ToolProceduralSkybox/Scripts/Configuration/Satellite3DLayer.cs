using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    [System.Serializable]
    public class Satellite3DLayer
    {
        // Satellite orbit properties
        public GameObject satellite;
        public float satelliteSize = 25;
        public float radius = 57;
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
    }
}