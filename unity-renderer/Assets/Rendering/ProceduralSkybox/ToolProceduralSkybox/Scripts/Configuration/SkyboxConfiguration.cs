using System.Collections.Generic;
using DCL.Shaders;
using UnityEngine;

namespace DCL.Skybox
{
    [CreateAssetMenu(fileName = "Skybox Configuration", menuName = "ScriptableObjects/SkyboxConfiguration", order = 1)]
    public class SkyboxConfiguration : ScriptableObject
    {
        public delegate void TimelineEvents(string tag, bool enable, bool trigger);
        public event TimelineEvents OnTimelineEvent;

        // ID
        public string skyboxID;

        // Background Color
        public Gradient skyColor = new Gradient();
        public Gradient horizonColor = new Gradient();
        public Gradient groundColor = new Gradient();
        public List<TransitioningFloat> horizonWidth = new List<TransitioningFloat>();
        public List<TransitioningFloat> horizonHeight = new List<TransitioningFloat>();

        // Horizon Layer
        public Texture2D horizonMask;
        public Vector2 horizonMaskTiling = new Vector2(0, 0);
        public Vector2 horizonMaskOffset = new Vector2(0, 0);

        // Horizon Plane Layer
        public Texture2D horizonPlaneTexture;
        public Vector2 horizonPlaneTiling = new Vector2(0, 0);
        public Vector2 horizonPlaneOffset = new Vector2(0, 0);
        public Gradient horizonPlaneColor = new Gradient();
        public List<TransitioningFloat> horizonPlaneHeight = new List<TransitioningFloat>();
        public Vector2 horizonPlaneSmoothRange = new Vector2(0, 0);
        public float horizonLightIntensity = 0;

        // Ambient Color
        public bool ambientTrilight = true;
        public Gradient ambientSkyColor = new Gradient();
        public Gradient ambientEquatorColor = new Gradient();
        public Gradient ambientGroundColor = new Gradient();

        // Avatar Color
        public bool useAvatarGradient = true;
        public Gradient avatarTintGradient = new Gradient();
        public Color avatarTintColor = Color.white;
        public bool useAvatarRealtimeDLDirection = true;
        public Vector3 avatarLightConstantDir = new Vector3(-18f, 144f, -72f);
        public bool useAvatarRealtimeLightColor = true;
        public Gradient avatarLightColorGradient = new Gradient();

        // Avatar Editor Color
        public Color avatarEditorTintColor = Color.white;
        public Vector3 avatarEditorLightDir = new Vector3(-18f, 144f, -72f);
        public Color avatarEditorLightColor = Color.white;

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

        // Layers
        public List<TextureLayer> layers = new List<TextureLayer>();

        // Timeline Tags
        public List<TimelineTagsDuration> timelineTags = new List<TimelineTagsDuration>();

        // 3D Components
        public List<Config3DDome> additional3Dconfig = new List<Config3DDome>();
        public List<Config3DSatellite> satelliteLayers = new List<Config3DSatellite>();
        public List<Config3DPlanar> planarLayers = new List<Config3DPlanar>();

        private float cycleTime = 24;

        public void ApplyOnMaterial(Material selectedMat, float dayTime, float normalizedDayTime, int slotCount = 5, Light directionalLightGO = null, float cycleTime = 24)
        {
            float percentage = normalizedDayTime * 100;

            // General Values
            selectedMat.SetColor(SkyboxShaderUtils.LightTint, directionalLightLayer.tintColor.Evaluate(normalizedDayTime));
            selectedMat.SetVector(SkyboxShaderUtils.LightDirection, directionalLightGO.transform.rotation.eulerAngles);

            // Apply Base Values
            selectedMat.SetColor(SkyboxShaderUtils.SkyColor, skyColor.Evaluate(normalizedDayTime));
            selectedMat.SetColor(SkyboxShaderUtils.GroundColor, groundColor.Evaluate(normalizedDayTime));

            // Apply Horizon Values
            selectedMat.SetColor(SkyboxShaderUtils.HorizonColor, horizonColor.Evaluate(normalizedDayTime));
            selectedMat.SetFloat(SkyboxShaderUtils.HorizonHeight, TransitioningValues.GetTransitionValue(horizonHeight, percentage, 0f));
            selectedMat.SetFloat(SkyboxShaderUtils.HorizonWidth, TransitioningValues.GetTransitionValue(horizonWidth, percentage, 0f));
            selectedMat.SetTexture(SkyboxShaderUtils.HorizonMask, horizonMask);
            selectedMat.SetVector(SkyboxShaderUtils.HorizonMaskValues, new Vector4(horizonMaskTiling.x, horizonMaskTiling.y, horizonMaskOffset.x, horizonMaskOffset.y));
            selectedMat.SetTexture(SkyboxShaderUtils.HorizonPlane, horizonPlaneTexture);
            selectedMat.SetVector(SkyboxShaderUtils.HorizonPlaneValues, new Vector4(horizonPlaneTiling.x, horizonPlaneTiling.y, horizonPlaneOffset.x, horizonPlaneOffset.y));
            selectedMat.SetColor(SkyboxShaderUtils.HorizonPlaneColor, horizonPlaneColor.Evaluate(normalizedDayTime));
            selectedMat.SetFloat(SkyboxShaderUtils.HorizonPlaneHeight, TransitioningValues.GetTransitionValue(horizonPlaneHeight, percentage, 0.0f));
            selectedMat.SetVector(SkyboxShaderUtils.PlaneSmoothRange, horizonPlaneSmoothRange);
            selectedMat.SetFloat(SkyboxShaderUtils.HorizonLightIntensity, horizonLightIntensity);


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

            //Fog Values
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

            //Directional List
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

            // Check and Fire timeline events
            CheckAndFireTimelineEvents(dayTime);

            ApplyAllSlots(selectedMat, layers, dayTime, normalizedDayTime, slotCount, cycleTime);
        }

        public void ApplyInWorldAvatarColor(float normalizedDayTime, GameObject directionalLightGO)
        {
            if (useAvatarGradient)
            {
                Shader.SetGlobalColor(ShaderUtils.TintColor, avatarTintGradient.Evaluate(normalizedDayTime));
            }
            else
            {
                Shader.SetGlobalColor(ShaderUtils.TintColor, avatarTintColor);
            }

            // Apply Avatar Light Direction
            if (!useAvatarRealtimeDLDirection || directionalLightGO == null || !useDirectionalLight)
            {
                Shader.SetGlobalVector(ShaderUtils.LightDir, avatarLightConstantDir / 180);
            }
            else
            {
                Vector3 tempDir = directionalLightGO.transform.rotation.eulerAngles;
                Shader.SetGlobalVector(ShaderUtils.LightDir, tempDir / 180);
            }

            // Apply Avatar Light Color
            if (!useAvatarRealtimeLightColor || directionalLightGO == null || !useDirectionalLight)
            {
                Shader.SetGlobalColor(ShaderUtils.LightColor, avatarLightColorGradient.Evaluate(normalizedDayTime));
            }
            else
            {
                Shader.SetGlobalColor(ShaderUtils.LightColor, directionalLightLayer.lightColor.Evaluate(normalizedDayTime));
            }
        }

        public void ApplyEditorAvatarColor()
        {
            Shader.SetGlobalColor(ShaderUtils.TintColor, avatarEditorTintColor);
            Shader.SetGlobalVector(ShaderUtils.LightDir, avatarEditorLightDir / 180);
            Shader.SetGlobalColor(ShaderUtils.LightColor, avatarEditorLightColor);
        }

        private void ApplyAllSlots(Material selectedMat, List<TextureLayer> layers, float dayTime, float normalizedDayTime, int slotCount, float cycleTime = 24)
        {
            for (int i = 0; i < slotCount; i++)
            {
                TextureLayer layer = GetActiveLayer(layers, dayTime, i);

                if (layer == null || !layer.enabled)
                {
                    ResetSlot(selectedMat, i);
                    continue;
                }

                TextureLayerFunctionality.ApplyTextureLayer(selectedMat, dayTime, normalizedDayTime, i, layer, cycleTime);
            }
        }

        public TextureLayer GetActiveLayer(List<TextureLayer> layers, float currentTime, int slotID)
        {
            TextureLayer temp = null;

            for (int i = 0; i < layers.Count; i++)
            {
                if (layers[i].slotID != slotID)
                {
                    continue;
                }

                float endTimeEdited = layers[i].timeSpan_End;
                float dayTimeEdited = currentTime;

                if (layers[i].timeSpan_End < layers[i].timeSpan_start)
                {
                    endTimeEdited = cycleTime + layers[i].timeSpan_End;
                    if (currentTime < layers[i].timeSpan_start)
                    {
                        dayTimeEdited = cycleTime + currentTime;
                    }
                }

                if (dayTimeEdited >= layers[i].timeSpan_start && dayTimeEdited <= endTimeEdited)
                {
                    if (layers[i].enabled)
                    {
                        if (temp == null)
                        {
                            layers[i].renderType = LayerRenderType.Rendering;
                            temp = layers[i];
                        }
                        else
                        {
                            temp.renderType = LayerRenderType.Conflict_Playing;
                            layers[i].renderType = LayerRenderType.Conflict_NotPlaying;
                        }
                    }
                    else
                    {
                        layers[i].renderType = LayerRenderType.NotRendering;
                    }
                }
                else
                {
                    layers[i].renderType = LayerRenderType.NotRendering;
                }


            }
            return temp;
        }

        #region Timeline Events

        /// <summary>
        /// Check if any event has to be fired
        /// TODO:: Can be optimised.
        /// </summary>
        /// <param name="dayTime">Current day time</param>
        private void CheckAndFireTimelineEvents(float dayTime)
        {
            float endTimeEdited = 0;
            float dayTimeEdited = dayTime;
            for (int i = 0; i < timelineTags.Count; i++)
            {
                // If current event is a trigger event, fire trigger event. It's boolean will be disable when cycle resets
                if (timelineTags[i].isTrigger)
                {
                    if (dayTime > timelineTags[i].startTime && !timelineTags[i].startEventExecuted)
                    {
                        OnTimelineEvent?.Invoke(timelineTags[i].tag, true, true);
                        timelineTags[i].startEventExecuted = true;
                    }
                    continue;
                }


                endTimeEdited = timelineTags[i].endTime;
                dayTimeEdited = dayTime;

                // Change time if this is over a day scenario
                if (timelineTags[i].endTime < timelineTags[i].startTime)
                {
                    endTimeEdited = cycleTime + timelineTags[i].endTime;

                    if (dayTime < timelineTags[i].startTime)
                    {
                        dayTimeEdited = cycleTime + dayTime;
                    }
                }

                // If current time is between start and end time and start event is not executed, Fire start event
                if (dayTimeEdited >= timelineTags[i].startTime && dayTimeEdited < endTimeEdited && !timelineTags[i].startEventExecuted)
                {
                    OnTimelineEvent?.Invoke(timelineTags[i].tag, true, false);
                    timelineTags[i].startEventExecuted = true;
                }

                // If current time is greater than end time and start event is executed, fire end event
                if (dayTimeEdited >= endTimeEdited && timelineTags[i].startEventExecuted)
                {
                    OnTimelineEvent?.Invoke(timelineTags[i].tag, false, false);
                    timelineTags[i].startEventExecuted = false;
                }

            }
        }

        public void CycleResets()
        {
            // Resets all timeline triggers and normal flow events
            for (int i = 0; i < timelineTags.Count; i++)
            {
                if (timelineTags[i].isTrigger)
                {
                    timelineTags[i].startEventExecuted = false;
                    continue;
                }

                if (timelineTags[i].endTime > timelineTags[i].startTime)
                {
                    timelineTags[i].startEventExecuted = false;
                }
            }
        }

        #endregion

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


            lightGO.intensity = TransitioningValues.GetTransitionValue(directionalLightLayer.intensity, normalizedDayTime * 100);
        }

        void ApplyDLDirection(float normalizedDayTime, Light lightGO)
        {
            if (lightGO == null)
            {
                return;
            }

            if (directionalLightLayer.lightDirection.Count == 0)
            {
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

        public void ResetMaterial(Material selectedMat, int slotCount)
        {
            for (int i = 0; i < slotCount; i++)
            {
                ResetSlot(selectedMat, i);
            }
        }

        public void ResetSlot(Material selectedMat, int slotCount)
        {
            selectedMat.SetFloat(SkyboxShaderUtils.GetLayerProperty("_layerType_" , slotCount), 0);
            selectedMat.SetTexture(SkyboxShaderUtils.GetLayerProperty("_tex_" , slotCount), null);
            selectedMat.SetTexture(SkyboxShaderUtils.GetLayerProperty("_cubemap_" , slotCount), null);
            selectedMat.SetTexture(SkyboxShaderUtils.GetLayerProperty("_normals_" , slotCount), null);
            selectedMat.SetColor(SkyboxShaderUtils.GetLayerProperty("_color_" , slotCount), new Color(1, 1, 1, 0));
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_tilingAndOffset_" , slotCount), new Vector4(1, 1, 0, 0));
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_speedAndRotation_" , slotCount), new Vector4(0, 0, 0));
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_timeFrame_" , slotCount), new Vector4(0, 0));
            selectedMat.SetFloat(SkyboxShaderUtils.GetLayerProperty("_fadeTime_" , slotCount), 1);
            selectedMat.SetFloat(SkyboxShaderUtils.GetLayerProperty("_normalIntensity_" , slotCount), 0);
            selectedMat.SetFloat(SkyboxShaderUtils.GetLayerProperty("_lightIntensity_" , slotCount), 0);
            selectedMat.SetFloat(SkyboxShaderUtils.GetLayerProperty("_RenderDistance_" , slotCount), 3.4f);
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_distortIntAndSize_" , slotCount), new Vector2(0, 0));
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_distortSpeedAndSharp_" , slotCount), new Vector4(0, 0, 0, 0));
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_rowAndCollumns_" , slotCount), new Vector4(1, 1));
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_particlesMainParameters_" , slotCount), new Vector4(0, 0, 0, 0));
            selectedMat.SetVector(SkyboxShaderUtils.GetLayerProperty("_particlesSecondaryParameters_" , slotCount), new Vector4(0, 0, 0, 0));
        }

    }
}
