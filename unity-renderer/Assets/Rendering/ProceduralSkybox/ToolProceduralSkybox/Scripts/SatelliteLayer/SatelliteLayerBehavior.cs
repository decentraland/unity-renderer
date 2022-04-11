using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{

    public class SatelliteLayerBehavior : MonoBehaviour
    {
        public float timeSpan_start = 0;
        public float timeSpan_end = 24;
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
        public float movementSpeed;             // speed in degree/hr (virtual hour)

        [Header("Satellite Properties")]
        public RotationType satelliteRotation;
        public Vector3 fixedRotation;
        public Vector3 rotateAroundAxis;
        public float rotateSpeed;

        private float currentAngle;
        private float cycleTime = 24;

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

        internal void AssignValues(float inTime, float outTime, float satelliteSize, float radius, float initialAngle, float horizonPlaneRotation, float inclination, float movementSpeed, RotationType satelliteRotation, Vector3 fixedRotation, Vector3 rotateAroundAxis, float rotateSpeed, float timeOfTheDay, float cycleTime)
        {
            this.timeSpan_start = inTime;
            this.timeSpan_end = outTime;
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
            this.cycleTime = cycleTime;

            // Update orbit rotation
            UpdateOrbitRotation();
            // Change satellite size
            UpdateSatelliteSize();

            if (!CheckIfSatelliteInTimeBounds(timeOfTheDay))
            {
                satellite.gameObject.SetActive(false);
                return;
            }
            satellite.gameObject.SetActive(true);

            // Update SatellitePosition
            UpdateSatellitePos(timeOfTheDay);

            UpdateRotation();
        }

        private void UpdateSatellitePos(float timeOfTheDay)
        {
            float timeOfDayEdited = timeOfTheDay;

            if (timeOfTheDay < timeSpan_start)
            {
                timeOfDayEdited += cycleTime;
            }

            float diff = timeOfDayEdited - timeSpan_start;
            currentAngle = initialAngle + (diff * movementSpeed);

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
            angle = angle % 360;
            float angleEdited = (90 - angle) * Mathf.Deg2Rad;
            float x = radius * Mathf.Cos(angleEdited);
            float y = radius * Mathf.Sin(angleEdited);
            return new Vector3(x, y, 0);
        }

        private bool CheckIfSatelliteInTimeBounds(float timeOfTheDay)
        {
            // Calculate edited time for the case of out time less than in time (over the day scenario)
            float outTimeEdited = timeSpan_end;
            float timeOfTheDayEdited = timeOfTheDay;

            if (timeSpan_end < timeSpan_start)
            {
                outTimeEdited += cycleTime;
            }

            if (timeOfTheDay < timeSpan_start)
            {
                timeOfTheDayEdited += cycleTime;
            }

            if (timeOfTheDayEdited >= timeSpan_start && timeOfTheDayEdited <= outTimeEdited)
            {
                return true;
            }
            else
            {
                return false;
            }
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