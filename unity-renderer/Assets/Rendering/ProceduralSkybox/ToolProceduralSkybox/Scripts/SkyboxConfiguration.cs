using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Skybox
{
    //[CreateAssetMenu(fileName = "Skybox Configuration", menuName = "ScriptableObjects/SkyboxConfiguration", order = 1)]
    public class SkyboxConfiguration : ScriptableObject
    {
        // ID
        public string skyboxID;

        // Background Color
        public Gradient skyColor = new Gradient();
        public Gradient horizonColor = new Gradient();
        public Gradient groundColor = new Gradient();

        // Horizon Layer
        public List<TransitioningFloat> horizonWidth = new List<TransitioningFloat>();
        public List<TransitioningFloat> horizonHeight = new List<TransitioningFloat>();
        public Texture horizonMask;
        public Vector3 horizonMaskValues = new Vector3(0, 0, 0);

        // Ambient Color
        public bool ambientTrilight = true;
        public Gradient ambientSkyColor = new Gradient();
        public Gradient ambientEquatorColor = new Gradient();
        public Gradient ambientGroundColor = new Gradient();

        // Fog Properties
        public bool useFog = false;
        public FogMode fogMode = FogMode.ExponentialSquared;
        public Gradient fogColor = new Gradient();
        public float fogDensity = 0.05f;
        public float fogStartDistance = 0;
        public float fogEndDistance = 300;

        // DirectionalLight Properties
        public bool useDirectionalLight = true;
        public DirectionalLightAttributes directionalLightLayer = new DirectionalLightAttributes();

        // Slots
        public List<SkyboxSlots> slots = new List<SkyboxSlots>();

        public void AddSlots() { slots.Add(new SkyboxSlots()); }

        // Texture Layer Properties
        public List<TextureLayer> textureLayers = new List<TextureLayer>();

        public void ApplyOnMaterial(Material selectedMat, float dayTime, float normalizedDayTime, Light directionalLightGO = null)
        {
            float percentage = normalizedDayTime * 100;

            // General Values
            selectedMat.SetFloat("_dayTime", dayTime);
            selectedMat.SetColor("_lightTint", directionalLightLayer.tintColor.Evaluate(normalizedDayTime));
            selectedMat.SetVector("_lightDirection", directionalLightGO.transform.rotation.eulerAngles);

            // Apply Base Base Values
            selectedMat.SetColor("_skyColor", skyColor.Evaluate(normalizedDayTime));
            selectedMat.SetColor("_groundColor", groundColor.Evaluate(normalizedDayTime));

            // Apply Horizon Values
            selectedMat.SetColor("_horizonColor", horizonColor.Evaluate(normalizedDayTime));
            selectedMat.SetFloat("_horizonHeight", GetTransitionValue(horizonHeight, percentage, 0f));
            selectedMat.SetFloat("_horizonWidth", GetTransitionValue(horizonWidth, percentage, 0f));
            selectedMat.SetTexture("_horizonMask_", horizonMask);
            selectedMat.SetVector("_horizonMaskValues", horizonMaskValues);


            // Apply Ambient colors to the rendering settings
            if (ambientTrilight)
            {
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
                RenderSettings.ambientSkyColor = ambientSkyColor.Evaluate(normalizedDayTime);
                RenderSettings.ambientEquatorColor = ambientEquatorColor.Evaluate(normalizedDayTime);
                RenderSettings.ambientGroundColor = ambientGroundColor.Evaluate(normalizedDayTime);
            }
            else
            {
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
            }

            // Fog Values
            RenderSettings.fog = useFog;
            if (useFog)
            {
                RenderSettings.fogColor = fogColor.Evaluate(normalizedDayTime);
                RenderSettings.fogMode = fogMode;
                switch (fogMode)
                {
                    case FogMode.Linear:
                        RenderSettings.fogStartDistance = fogStartDistance;
                        RenderSettings.fogEndDistance = fogEndDistance;
                        break;
                    default:
                        RenderSettings.fogDensity = fogDensity;
                        break;
                }
            }

            // Directional List
            if (useDirectionalLight)
            {
                // Apply Direction light color
                directionalLightGO.color = directionalLightLayer.lightColor.Evaluate(normalizedDayTime);

                // Apply direction
                ApplyDLDirection(normalizedDayTime, directionalLightGO);

                // Apply Intensity
                ApplyDLIntensity(normalizedDayTime, directionalLightGO);
            }
            else
            {
                directionalLightGO.gameObject.SetActive(false);
            }

            ApplyAllSlots(selectedMat, dayTime, normalizedDayTime);

            //for (int i = 0; i < textureLayers.Count; i++)
            //{
            //    ApplyTextureLayer(selectedMat, dayTime, normalizedDayTime, i, textureLayers[i]);
            //}
        }

        void ApplyAllSlots(Material selectedMat, float dayTime, float normalizedDayTime)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].disabled)
                {
                    ResetSlot(selectedMat, i);
                    continue;
                }

                TextureLayer layer = slots[i].GetActiveLayer(dayTime);

                if (layer == null || layer.disabled)
                {
                    ResetSlot(selectedMat, i);
                    continue;
                }

                ApplyTextureLayer(selectedMat, dayTime, normalizedDayTime, i, layer);
            }
        }

        #region Directional Light

        void ApplyDLIntensity(float normalizedDayTime, Light lightGO)
        {
            if (lightGO == null)
            {
                Debug.LogError("Directional Light option is on, with light reference");
                return;
            }

            if (directionalLightLayer.intensity.Count == 0)
            {
                Debug.Log("Directional Light option is on with no intensity data");
                return;
            }

            if (directionalLightLayer.intensity.Count == 1)
            {
                lightGO.intensity = directionalLightLayer.intensity[0].value;
                return;
            }


            lightGO.intensity = GetTransitionValue(directionalLightLayer.intensity, normalizedDayTime * 100);
        }

        void ApplyDLDirection(float normalizedDayTime, Light lightGO)
        {
            if (lightGO == null)
            {
                Debug.LogError("Directional Light option is on, with light reference");
                return;
            }

            if (directionalLightLayer.lightDirection.Count == 0)
            {
                Debug.Log("Directional Light option is on with no rotation data");
                return;
            }

            if (directionalLightLayer.lightDirection.Count == 1)
            {
                lightGO.transform.rotation = directionalLightLayer.lightDirection[0].value;
                return;
            }

            float percentage = normalizedDayTime * 100;
            TransitioningQuaternion min = directionalLightLayer.lightDirection[0], max = directionalLightLayer.lightDirection[0];

            // Apply Direction
            for (int i = 0; i < directionalLightLayer.lightDirection.Count; i++)
            {
                if (percentage <= directionalLightLayer.lightDirection[i].percentage)
                {
                    max = directionalLightLayer.lightDirection[i];

                    if ((i - 1) > 0)
                    {
                        min = directionalLightLayer.lightDirection[i - 1];
                    }

                    break;
                }
            }

            float t = Mathf.InverseLerp(min.percentage, max.percentage, percentage);
            lightGO.transform.rotation = Quaternion.Lerp(min.value, max.value, t);
        }

        #endregion

        #region Apply texture layers

        void ApplyTextureLayer(Material selectedMat, float dayTime, float normalizedDayTime, int layerNum, TextureLayer layer, bool changeAlllValues = true)
        {
            // Time not reached for current layer
            if (dayTime < layer.timeSpan_start || dayTime > layer.timeSpan_End)
            {
                return;
            }
            float normalizedLayerTime = Mathf.InverseLerp(layer.timeSpan_start, layer.timeSpan_End, dayTime);
            selectedMat.SetFloat("_layerType_" + layerNum, (int)layer.layerType);

            if (layer.layerType == LayerType.Cubemap)
            {
                ApplyCubemapTextureLayer(selectedMat, dayTime, normalizedLayerTime, layerNum, layer, true);
                return;
            }
            else if (layer.layerType == LayerType.Planar || layer.layerType == LayerType.Radial)
            {
                ApplyPlanarTextureLayer(selectedMat, dayTime, normalizedLayerTime, layerNum, layer, true);
                return;
            }
            else
            {
                ApplySatelliteTextureLayer(selectedMat, dayTime, normalizedLayerTime, layerNum, layer, true);
            }
        }

        void ApplyCubemapTextureLayer(Material selectedMat, float dayTime, float normalizedLayerTime, int layerNum, TextureLayer layer, bool changeAlllValues = true)
        {
            if (changeAlllValues)
            {
                selectedMat.SetTexture("_cubemap_" + layerNum, layer.cubemap);

                selectedMat.SetTexture("_tex_" + layerNum, null);
                selectedMat.SetTexture("_normals_" + layerNum, null);
                selectedMat.SetFloat("_normalIntensity_" + layerNum, 0);
                selectedMat.SetFloat("_rotation_" + layerNum, 0);
                selectedMat.SetVector("_timeFrame_" + layerNum, new Vector4(layer.timeSpan_start, layer.timeSpan_End));
                selectedMat.SetFloat("_fadeTime_" + layerNum, layer.fadingIn);
                selectedMat.SetFloat("_lightIntensity_" + layerNum, layer.tintercentage / 100);
                selectedMat.SetFloat("_RenderDistance_" + layerNum, 0);
            }


            selectedMat.SetColor("_color_" + layerNum, layer.color.Evaluate(normalizedLayerTime));

            // Set cubemap rotation. (Shader variable reused)   
            Vector3 currentRotation = GetTransitionValue(layer.cubemapRotations, normalizedLayerTime * 100);
            selectedMat.SetVector("_tilingAndOffset_" + layerNum, new Vector4(currentRotation.x, currentRotation.y, currentRotation.z, 0));

            // Set speed float to vector2
            //selectedMat.SetVector("_speed_" + layerNum, new Vector2(layer.speed, 0));
        }

        void ApplyPlanarTextureLayer(Material selectedMat, float dayTime, float normalizedLayerTime, int layerNum, TextureLayer layer, bool changeAlllValues = true)
        {
            if (layer.layerType == LayerType.Planar)
            {
                selectedMat.SetFloat("_RenderDistance_" + layerNum, GetTransitionValue(layer.renderDistance, normalizedLayerTime * 100, 3.4f));
            }

            selectedMat.SetTexture("_tex_" + layerNum, layer.texture);
            selectedMat.SetTexture("_normals_" + layerNum, layer.textureNormal);
            selectedMat.SetTexture("_cubemap_" + layerNum, null);

            selectedMat.SetColor("_color_" + layerNum, layer.color.Evaluate(normalizedLayerTime));

            // Tiling and Offset
            Vector2 currentOffset = GetTransitionValue(layer.offset, normalizedLayerTime * 100);
            Vector4 t = new Vector4(layer.tiling.x, layer.tiling.y, currentOffset.x, currentOffset.y);
            selectedMat.SetVector("_tilingAndOffset_" + layerNum, t);

            // Set speed float to vector2
            //selectedMat.SetVector("_speed_" + layerNum, layer.speed_Vec2);
            // speed and Rotation
            float rot = 0;
            if (layer.layerType == LayerType.Planar)
            {
                rot = GetTransitionValue(layer.rotation_float, normalizedLayerTime * 100);
            }

            selectedMat.SetVector("_speedAndRotation_" + layerNum, new Vector4(layer.speed_Vec2.x, layer.speed_Vec2.y, rot));

            // Time frame
            selectedMat.SetVector("_timeFrame_" + layerNum, new Vector4(layer.timeSpan_start, layer.timeSpan_End));
            //Fade time
            selectedMat.SetFloat("_fadeTime_" + layerNum, layer.fadingIn);
            // normal intensity
            selectedMat.SetFloat("_normalIntensity_" + layerNum, layer.normalIntensity);
            // Tint
            selectedMat.SetFloat("_lightIntensity_" + layerNum, layer.tintercentage / 100);
        }

        void ApplySatelliteTextureLayer(Material selectedMat, float dayTime, float normalizedLayerTime, int layerNum, TextureLayer layer, bool changeAlllValues = true)
        {
            //selectedMat.SetFloat("_RenderDistance_" + layerNum, GetTransitionValue(layer.renderDistance, normalizedLayerTime * 100, 0.0f));

            selectedMat.SetTexture("_tex_" + layerNum, layer.texture);
            selectedMat.SetTexture("_normals_" + layerNum, layer.textureNormal);
            selectedMat.SetTexture("_cubemap_" + layerNum, null);

            selectedMat.SetColor("_color_" + layerNum, layer.color.Evaluate(normalizedLayerTime));

            // Tiling and Offset
            Vector2 currentOffset = GetTransitionValue(layer.offset, normalizedLayerTime * 100);
            Vector2 currentWidthHeight = GetTransitionValue(layer.satelliteWidthHeight, normalizedLayerTime * 100, new Vector2(1, 1));
            Vector4 t = new Vector4(currentWidthHeight.x, currentWidthHeight.y, currentOffset.x, currentOffset.y);
            selectedMat.SetVector("_tilingAndOffset_" + layerNum, t);

            // Set speed float to vector2
            //selectedMat.SetVector("_speed_" + layerNum, layer.speed_Vec2);
            // speed and Rotation
            float rot = GetTransitionValue(layer.rotation_float, normalizedLayerTime * 100);
            selectedMat.SetVector("_speedAndRotation_" + layerNum, new Vector4(layer.speed_Vec2.x, layer.speed_Vec2.y, rot));

            // Time frame
            selectedMat.SetVector("_timeFrame_" + layerNum, new Vector4(layer.timeSpan_start, layer.timeSpan_End));
            //Fade time
            selectedMat.SetFloat("_fadeTime_" + layerNum, layer.fadingIn);
            // normal intensity
            selectedMat.SetFloat("_normalIntensity_" + layerNum, layer.normalIntensity);
            // Tint
            selectedMat.SetFloat("_lightIntensity_" + layerNum, layer.tintercentage / 100);
        }

        #endregion

        #region Transition Values Utility Methods

        float GetTransitionValue(List<TransitioningFloat> _list, float percentage, float defaultVal = 0)
        {
            if (_list == null || _list.Count < 1)
            {
                return defaultVal;
            }

            if (_list.Count == 1)
            {
                return _list[0].value;
            }

            TransitioningFloat min = _list[0], max = _list[0];

            // Apply Direction
            for (int i = 0; i < _list.Count; i++)
            {
                if (percentage <= _list[i].percentage)
                {
                    max = _list[i];

                    if ((i - 1) > 0)
                    {
                        min = _list[i - 1];
                    }

                    break;
                }
            }

            float t = Mathf.InverseLerp(min.percentage, max.percentage, percentage);
            return Mathf.Lerp(min.value, max.value, t);
        }

        Vector2 GetTransitionValue(List<TransitioningVector2> _list, float percentage, Vector2 defaultVal = default(Vector2))
        {
            Vector2 offset = defaultVal;

            if (_list == null || _list.Count == 0)
            {
                return offset;
            }

            if (_list.Count == 1)
            {
                offset = _list[0].value;
                return offset;
            }


            TransitioningVector2 min = _list[0], max = _list[0];

            for (int i = 0; i < _list.Count; i++)
            {
                if (percentage <= _list[i].percentage)
                {
                    max = _list[i];

                    if ((i - 1) > 0)
                    {
                        min = _list[i - 1];
                    }

                    break;
                }
            }
            float t = Mathf.InverseLerp(min.percentage, max.percentage, percentage);
            offset = Vector2.Lerp(min.value, max.value, t);

            return offset;
        }

        Vector3 GetTransitionValue(List<TransitioningVector3> _list, float percentage)
        {
            Vector3 offset = new Vector3(0, 0, 0);

            if (_list == null || _list.Count == 0)
            {
                return offset;
            }

            if (_list.Count == 1)
            {
                offset = _list[0].value;
                return offset;
            }


            TransitioningVector3 min = _list[0], max = _list[0];

            for (int i = 0; i < _list.Count; i++)
            {
                if (percentage <= _list[i].percentage)
                {
                    max = _list[i];

                    if ((i - 1) > 0)
                    {
                        min = _list[i - 1];
                    }

                    break;
                }
            }

            float t = Mathf.InverseLerp(min.percentage, max.percentage, percentage);
            offset = Vector3.Lerp(min.value, max.value, t);

            return offset;
        }

        #endregion

        public void ResetMaterial(Material selectedMat, int slotCount)
        {
            for (int i = 0; i < slotCount; i++)
            {
                ResetSlot(selectedMat, i);
            }
        }

        public void ResetSlot(Material selectedMat, int slotCount)
        {
            selectedMat.SetFloat("_RenderDistance_" + slotCount, 3.4f);
            selectedMat.SetFloat("_layerType_" + slotCount, 0);
            selectedMat.SetTexture("_tex_" + slotCount, null);
            selectedMat.SetTexture("_cubemap_" + slotCount, null);
            selectedMat.SetTexture("_normals_" + slotCount, null);
            selectedMat.SetColor("_color_" + slotCount, new Color(1, 1, 1, 0));
            selectedMat.SetVector("_tilingAndOffset_" + slotCount, new Vector4(1, 1, 0, 0));
            selectedMat.SetVector("_speedAndRotation_" + slotCount, new Vector4(0, 0, 0));
            selectedMat.SetVector("_timeFrame_" + slotCount, new Vector4(0, 0));
            selectedMat.SetFloat("_fadeTime_" + slotCount, 0);
            selectedMat.SetFloat("_normalIntensity_" + slotCount, 0);
            selectedMat.SetFloat("_lightIntensity_" + slotCount, 0);
            selectedMat.SetVector("_distortIntAndSize_" + slotCount, new Vector2(0, 0));
            selectedMat.SetVector("_distortSpeedAndSharp_" + slotCount, new Vector4(0, 0, 0, 0));
        }

        Quaternion Vector4ToQuaternion(Vector4 val) { return new Quaternion(val.x, val.y, val.z, val.w); }
    }
}