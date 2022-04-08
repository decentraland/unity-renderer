using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{

    public class SatelliteLayerBehavior : MonoBehaviour
    {
        public Transform followTarget;
        public GameObject satelliteOrbit;
        public GameObject satellite;
        public float satelliteSize = 1;
        public float radius = 10;
        public float thickness = 1;
        [Range(0, 360)]
        public float initialAngle = 0;
        [Range(0, 180)]
        public float horizonPlaneRotation = 0;
        [Range(0, 180)]
        public float inclination = 0;

        public bool startMovement;
        public float movementSpeed;

        [Header("Satellite Properties")]
        public RotationType satelliteRotation;
        public Vector3 fixedRotation;
        public Vector3 rotateAroundAxis;
        public float rotateSpeed;

        float currentAngle;
        float timeOfTheDay;

        private void Update()
        {
            UpdateRotation();

            // Start rotating
            UpdateMovement();
        }

        public void UpdateMovement()
        {
            if (!startMovement)
            {
                return;
            }
            currentAngle += movementSpeed * Time.deltaTime;

            if (currentAngle > 360)
            {
                currentAngle = 0;
            }
            initialAngle = currentAngle;
            satellite.transform.localPosition = GetSatellitePosition(radius, currentAngle);
        }

        public void UpdateRotation()
        {
            if (satellite == null)
            {
                return;
            }


            switch (satelliteRotation)
            {
                case RotationType.Fixed:
                    satellite.transform.localRotation = Quaternion.Euler(fixedRotation);
                    break;
                case RotationType.Rotate:
                    satellite.transform.Rotate(rotateAroundAxis, rotateSpeed * Time.deltaTime, Space.Self);
                    break;
                case RotationType.LookAtOrbit:
                    if (Camera.main != null)
                    {
                        satellite.transform.LookAt(Camera.main.transform);
                    }
                    break;
                default:
                    break;
            }
        }

        internal void AssignValues(float satelliteSize, float radius, float initialAngle, float horizonPlaneRotation, float inclination, float movementSpeed, RotationType satelliteRotation, Vector3 fixedRotation, Vector3 rotateAroundAxis, float rotateSpeed, float timeOfTheDay)
        {
            this.satelliteSize = satelliteSize;
            this.radius = radius;
            radius = Mathf.Clamp(radius, 0, Mathf.Infinity);
            this.initialAngle = initialAngle;
            this.horizonPlaneRotation = horizonPlaneRotation;
            this.inclination = inclination;
            this.movementSpeed = movementSpeed;
            this.satelliteRotation = satelliteRotation;
            this.fixedRotation = fixedRotation;
            this.rotateAroundAxis = rotateAroundAxis;
            this.rotateSpeed = rotateSpeed;
            this.timeOfTheDay = timeOfTheDay;

            // Change satellite size
            UpdateSatelliteSize();
            // Update orbit rotation
            UpdateOrbitRotation();
            // Update SatellitePosition
            UpdateSatellitePos();
        }

        private void UpdateSatellitePos()
        {
            //float percentage = timeOfTheDay - (int)timeOfTheDay;
            //percentage /= movementSpeed;
            //currentAngle = 360 * percentage;
            //currentAngle += initialAngle;
            //float diff = 0;
            //if (currentAngle > 360)
            //{
            //    diff = currentAngle - 360;
            //    currentAngle = diff;
            //}

            currentAngle = initialAngle;

            if (currentAngle > 360)
            {
                currentAngle = 0;
            }
            satellite.transform.localPosition = GetSatellitePosition(radius, currentAngle);
        }

        private void UpdateSatelliteSize()
        {
            if (satellite == null)
            {
                return;
            }
            // change satellite size
            satellite.transform.localScale = Vector3.one * satelliteSize;
        }

        private void UpdateOrbitRotation()
        {
            if (satelliteOrbit == null)
            {
                Debug.LogError("Satellite Orbit not assigned");
                return;
            }

            //  Rotate orbit plane along horizon line
            Vector3 rot = satelliteOrbit.transform.localRotation.eulerAngles;
            rot.z = 0;
            rot.y = horizonPlaneRotation;
            //  Rotate orbit plane along inclination line
            rot.x = inclination;
            satelliteOrbit.transform.localRotation = Quaternion.Euler(rot);
        }

        Vector3 GetSatellitePosition(float radius, float angle)
        {
            angle = Mathf.Clamp(angle, 0, 360);
            float angleEdited = (90 - angle) * Mathf.Deg2Rad;
            float x = radius * Mathf.Cos(angleEdited);
            float y = radius * Mathf.Sin(angleEdited);
            return new Vector3(x, y, 0);
        }

        private void OnDrawGizmos()
        {
            var tr = transform;
            var position = tr.position;
            Handles.color = Color.gray;
            Handles.DrawWireDisc(position, Vector3.up, radius, thickness);

            if (satelliteOrbit == null)
            {
                return;
            }
            // Draw wire disc of green color for orbit orthogonal to y = 1
            Handles.color = Color.green;
            Handles.DrawWireDisc(position, satelliteOrbit.transform.forward, radius, thickness);
        }

    }
}