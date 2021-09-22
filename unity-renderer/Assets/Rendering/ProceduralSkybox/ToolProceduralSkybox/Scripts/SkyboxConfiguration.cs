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
            selectedMat.SetFloat("_horizonHeight", GetTansitionValue(horizonHeight, percentage, 0f));
            selectedMat.SetFloat("_horizonWidth", GetTansitionValue(horizonWidth, percentage, 0f));


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

            for (int i = 0; i < textureLayers.Count; i++)
            {
                ApplyTextureLayer(selectedMat, normalizedDayTime, i, textureLayers[i]);
            }
        }

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


            lightGO.intensity = GetTansitionValue(directionalLightLayer.intensity, normalizedDayTime * 100);
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
                lightGO.transform.rotation = Vector4ToQuaternion(directionalLightLayer.lightDirection[0].value);
                return;
            }

            float percentage = normalizedDayTime * 100;
            TransitioningVector4 min = directionalLightLayer.lightDirection[0], max = directionalLightLayer.lightDirection[0];

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
            Quaternion lightDirection = Quaternion.Lerp(Vector4ToQuaternion(min.value), Vector4ToQuaternion(max.value), t);


            lightGO.transform.rotation = lightDirection;
        }

        void ApplyTextureLayer(Material selectedMat, float normalizedDayTime, int layerNum, TextureLayer layer, bool changeAlllValues = true)
        {
            if (changeAlllValues)
            {
                //if (layer.isRadial)
                //{
                //    selectedMat.SetFloat("_isRadial_" + layerNum, 1);
                //}
                //else
                //{
                //    selectedMat.SetFloat("_isRadial_" + layerNum, 0);
                //}

                selectedMat.SetFloat("_isRadial_" + layerNum, (int)layer.layerType);

                if (layer.layerType == LayerType.Cubemap)
                {
                    selectedMat.SetTexture("_cubemap_" + layerNum, layer.cubemap);
                }
                else
                {
                    selectedMat.SetTexture("_tex_" + layerNum, layer.texture);
                    selectedMat.SetTexture("_normals_" + layerNum, layer.textureNormal);
                    selectedMat.SetFloat("_normalIntensity_" + layerNum, layer.normalIntensity);
                }


                selectedMat.SetFloat("_speed_" + layerNum, layer.speed);
                selectedMat.SetVector("_timeFrame_" + layerNum, new Vector4(layer.timeSpan_start, layer.timeSpan_End));
                selectedMat.SetFloat("_fadeTime_" + layerNum, layer.fadingIn);
                selectedMat.SetFloat("_lightIntensity_" + layerNum, layer.tintercentage / 100);
            }
            selectedMat.SetFloat("_RenderDistance_" + layerNum, GetTansitionValue(layer.renderDistance, normalizedDayTime * 100, 3.4f));

            Vector2 currentOffset = GetTransitionValue(layer.position, normalizedDayTime * 100);
            Vector4 t = new Vector4(layer.tiling.x, layer.tiling.y, currentOffset.x, currentOffset.y);
            selectedMat.SetVector("_tilingAndOffset_" + layerNum, t);

            selectedMat.SetColor("_color_" + layerNum, layer.color.Evaluate(normalizedDayTime));
        }

        Quaternion Vector4ToQuaternion(Vector4 val) { return new Quaternion(val.x, val.y, val.z, val.w); }

        float GetTansitionValue(List<TransitioningFloat> _list, float percentage, float defaultVal = 0)
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

        Vector2 GetTransitionValue(List<TransitioningVector2> _list, float percentage)
        {
            Vector2 offset = new Vector2(0, 0);

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

        public void ResetMaterial(Material selectedMat, int layerNum)
        {
            for (int i = 0; i < layerNum; i++)
            {
                selectedMat.SetFloat("_RenderDistance_" + i, 3.4f);
                selectedMat.SetFloat("_isRadial_" + i, 0);
                selectedMat.SetTexture("_tex_" + i, null);
                selectedMat.SetTexture("_normals_" + i, null);
                selectedMat.SetTexture("_cubemap_" + i, null);
                selectedMat.SetFloat("_normalIntensity_" + i, 0);
                selectedMat.SetFloat("_speed_" + i, 0);
                selectedMat.SetVector("_timeFrame_" + i, new Vector4(0, 0));
                selectedMat.SetFloat("_fadeTime_" + i, 0);
                selectedMat.SetFloat("_lightIntensity_" + i, 0);
                selectedMat.SetVector("_tilingAndOffset_" + layerNum, new Vector4(1, 1, 0, 0));
                selectedMat.SetColor("_color_" + i, new Color(1, 1, 1, 0));
            }
        }
    }

    [System.Serializable]
    public class TransitioningFloat
    {
        public float percentage;
        public float value;

        public TransitioningFloat(float percentage, float value)
        {
            this.percentage = percentage;
            this.value = value;
        }
    }

    [System.Serializable]
    public class TransitioningVector3
    {
        public float percentage;
        public Vector3 value;

        public TransitioningVector3(float percentage, Vector3 value)
        {
            this.percentage = percentage;
            this.value = value;
        }
    }

    [System.Serializable]
    public class TransitioningVector2
    {
        public float percentage;
        public Vector2 value;

        public TransitioningVector2(float percentage, Vector2 value)
        {
            this.percentage = percentage;
            this.value = value;
        }
    }

    [System.Serializable]
    public class TransitioningVector4
    {
        public float percentage;
        public Vector4 value;

        public TransitioningVector4(float percentage, Vector4 value)
        {
            this.percentage = percentage;
            this.value = value;
        }
    }

    [System.Serializable]
    public class DirectionalLightAttributes
    {
        public Gradient lightColor;
        public List<TransitioningFloat> intensity;
        public List<TransitioningVector4> lightDirection;
        public Gradient tintColor;

        public DirectionalLightAttributes()
        {
            lightColor = new Gradient();
            intensity = new List<TransitioningFloat>();
            lightDirection = new List<TransitioningVector4>();
            tintColor = new Gradient();
        }
    }

    [System.Serializable]
    public class TextureLayer
    {
        public string nameInEditor;
        public bool expandedInEditor;
        public float timeSpan_start;
        public float timeSpan_End;
        public float fadingIn, fadingOut;
        public float tintercentage;
        internal List<TransitioningFloat> renderDistance;
        internal LayerType layerType;
        public Texture2D texture;
        public Texture2D textureNormal;
        public Cubemap cubemap;
        public Gradient color;
        public Vector2 tiling;
        public List<TransitioningVector2> position;                     // Offset
        public float speed;
        public float normalIntensity;

        public TextureLayer(string name = "noname")
        {
            tiling = new Vector2(1, 1);
            nameInEditor = name;
            position = new List<TransitioningVector2>();
            renderDistance = new List<TransitioningFloat>();
            color = new Gradient();
        }
    }

    internal enum LayerType
    {
        Planar = 0,
        Radial = 1,
        Cubemap = 2
    }
}