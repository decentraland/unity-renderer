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
        public float satelliteSize = 1;
        public float radius = 10;
        [Range(0, 360)]
        public float initialAngle = 0;
        [Range(0, 180)]
        public float horizonPlaneRotation = 0;
        [Range(0, 180)]
        public float inclination = 0;
        public float movementSpeed;

        // Satellite Properties
        public RotationType satelliteRotation;
        public Vector3 fixedRotation;
        public Vector3 rotateAroundAxis;
        public float rotateSpeed;
    }
}