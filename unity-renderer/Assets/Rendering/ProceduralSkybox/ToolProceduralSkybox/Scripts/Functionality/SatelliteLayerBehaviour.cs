using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{

    public class SatelliteLayerBehaviour : MonoBehaviour
    {
        Config3DSatellite layerProperties;
        public GameObject satelliteOrbit;
        public GameObject dummyObj;
        public GameObject satellite;

        public float thickness = 1;

        private float currentAngle;
        private float cycleTime = 24;
        private List<Material> materials;
        private Transform camTransform;

        private void UpdateRotation()
        {
            if (satellite == null)
            {
                return;
            }

            if (camTransform == null)
            {
                if (Camera.main != null)
                {
                    camTransform = Camera.main.transform;
                }
            }

            switch (layerProperties.satelliteRotation)
            {
                case RotationType.Fixed:
                    satellite.transform.localRotation = Quaternion.Euler(layerProperties.fixedRotation);
                    break;
                case RotationType.Rotate:
                    satellite.transform.Rotate(layerProperties.rotateAroundAxis, layerProperties.rotateSpeed * Time.deltaTime, Space.Self);
                    break;
                case RotationType.LookAtOrbit:
                    if (camTransform != null)
                    {
                        satellite.transform.LookAt(camTransform);
                    }
                    break;
                default:
                    break;
            }
        }

        internal void AssignValues(Config3DSatellite properties, float timeOfTheDay, float cycleTime, bool isEditor = false)
        {
            // Check and assign Materials
            CheckAndAssignMats();

            layerProperties = properties;
            layerProperties.radius = Mathf.Clamp(layerProperties.radius, 0, Mathf.Infinity);

            this.cycleTime = cycleTime;

            if (!CheckIfSatelliteInTimeBounds(timeOfTheDay))
            {
                satellite.gameObject.SetActive(false);
                ChangeRenderType(LayerRenderType.NotRendering);
                return;
            }

            satellite.gameObject.SetActive(true);
            ChangeRenderType(LayerRenderType.Rendering);

            // Apply fade
            ApplyFade(timeOfTheDay);

            // Update orbit rotation
            UpdateOrbit();
            // Change satellite size
            UpdateSatelliteSize();

            // Update SatellitePosition
            UpdateSatellitePos(timeOfTheDay);

            UpdateRotation();
        }

        private void CheckAndAssignMats()
        {
            if (materials == null)
            {
                materials = new List<Material>();
                Renderer[] renderers = GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renderers.Length; i++)
                {
                    for (int j = 0; j < renderers[i].sharedMaterials.Length; j++)
                    {
                        materials.Add(renderers[i].sharedMaterials[j]);
                    }
                }
            }
        }

        public void ChangeRenderType(LayerRenderType type)
        {
            if (layerProperties == null)
            {
                return;
            }

            layerProperties.renderType = type;
        }

        private void UpdateSatellitePos(float timeOfTheDay)
        {
            float timeOfDayEdited = timeOfTheDay;

            if (timeOfTheDay < layerProperties.timeSpanStart)
            {
                timeOfDayEdited += cycleTime;
            }

            float diff = timeOfDayEdited - layerProperties.timeSpanStart;
            currentAngle = layerProperties.initialAngle + (diff * layerProperties.movementSpeed);

            dummyObj.transform.localPosition = GetSatellitePosition(layerProperties.radius, currentAngle);
            satellite.transform.position = dummyObj.transform.position;
        }

        private void UpdateSatelliteSize()
        {
            if (satellite == null)
            {
                return;
            }
            // change satellite size
            satellite.transform.localScale = Vector3.one * layerProperties.satelliteSize;
        }

        /// <summary>
        /// Update Rotation and position
        /// </summary>
        private void UpdateOrbit()
        {
            if (satelliteOrbit == null)
            {
                Debug.LogError("Satellite Orbit not assigned");
                return;
            }

            // Orbit vertical placement
            Vector3 pos = satelliteOrbit.transform.localPosition;
            pos.y = layerProperties.orbitYOffset;
            satelliteOrbit.transform.localPosition = pos;

            //  Rotate orbit plane along horizon line
            Vector3 rot = satelliteOrbit.transform.localRotation.eulerAngles;
            rot.z = 0;
            rot.y = layerProperties.horizonPlaneRotation;
            //  Rotate orbit plane along inclination line
            rot.x = layerProperties.inclination;
            satelliteOrbit.transform.localRotation = Quaternion.Euler(rot);
        }

        private Vector3 GetSatellitePosition(float radius, float angle)
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
            float outTimeEdited = layerProperties.timeSpanEnd;
            float timeOfTheDayEdited = timeOfTheDay;

            if (layerProperties.timeSpanEnd < layerProperties.timeSpanStart)
            {
                outTimeEdited += cycleTime;
            }

            if (timeOfTheDay < layerProperties.timeSpanStart)
            {
                timeOfTheDayEdited += cycleTime;
            }

            if (timeOfTheDayEdited >= layerProperties.timeSpanStart && timeOfTheDayEdited <= outTimeEdited)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ApplyFade(float timeOfTheDay)
        {
            float fadeAmount = 1;

            if (CheckFadingIn(timeOfTheDay, out fadeAmount))
            {
                for (int i = 0; i < materials.Count; i++)
                {
                    materials[i].SetFloat(SkyboxShaderUtils.Opacity, fadeAmount);
                }
            }
            else if (CheckFadingOut(timeOfTheDay, out fadeAmount))
            {
                for (int i = 0; i < materials.Count; i++)
                {
                    materials[i].SetFloat(SkyboxShaderUtils.Opacity, fadeAmount);
                }
            }
            else
            {
                for (int i = 0; i < materials.Count; i++)
                {
                    materials[i].SetFloat(SkyboxShaderUtils.Opacity, fadeAmount);
                }

            }
        }

        private bool CheckFadingIn(float dayTime, out float fadeAmt)
        {
            fadeAmt = 1;
            bool fadeChanged = false;
            float fadeInCompletionTime = layerProperties.timeSpanStart + layerProperties.fadeInTime;
            float dayTimeEdited = dayTime;
            if (dayTime < layerProperties.timeSpanStart)
            {
                dayTimeEdited = cycleTime + dayTime;
            }

            if (dayTimeEdited < fadeInCompletionTime)
            {
                float percentage = Mathf.InverseLerp(layerProperties.timeSpanStart, fadeInCompletionTime, dayTimeEdited);
                fadeAmt = percentage;
                fadeChanged = true;
            }

            return fadeChanged;
        }

        private bool CheckFadingOut(float dayTime, out float fadeAmt)
        {
            fadeAmt = 1;
            bool fadeChanged = false;
            float endTimeEdited = layerProperties.timeSpanEnd;
            float dayTimeEdited = dayTime;

            if (layerProperties.timeSpanEnd < layerProperties.timeSpanStart)
            {
                endTimeEdited = cycleTime + layerProperties.timeSpanEnd;
            }

            if (dayTime < layerProperties.timeSpanStart)
            {
                dayTimeEdited = cycleTime + dayTime;
            }


            float fadeOutStartTime = endTimeEdited - layerProperties.fadeOutTime;

            if (dayTimeEdited > fadeOutStartTime)
            {
                float percentage = Mathf.InverseLerp(endTimeEdited, fadeOutStartTime, dayTimeEdited);
                fadeAmt = percentage;
                fadeChanged = true;
            }

            return fadeChanged;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var tr = transform;
            var position = tr.position;
            Handles.color = Color.gray;
            Handles.DrawWireDisc(position, Vector3.up, 1000, thickness);

            if (satelliteOrbit == null || layerProperties == null)
            {
                return;
            }
            position = satelliteOrbit.transform.position;
            // Draw wire disc of green color for orbit orthogonal to y = 1
            Handles.color = Color.green;
            Handles.DrawWireDisc(position, satelliteOrbit.transform.forward, layerProperties.radius, thickness);
        }
#endif
    }
}