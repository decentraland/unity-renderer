using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public class SkyboxEditorWindow : EditorWindow
    {
        private List<SkyboxConfiguration> configurations;
        private List<string> configurationNames;
        private int selectedConfigurationIndex;
        private int newConfigIndex;
        private SkyboxConfiguration selectedConfiguration;

        public bool isPaused;
        public float timeOfTheDay;
        private Material selectedMat;
        private Vector2 panelScrollPos;
        private Light directionalLight;
        private bool creatingNewConfig;
        private string newConfigName;

        private bool showBackgroundLayer;
        private bool showAmbienLayer;
        private bool showFogLayer;
        private bool showDLLayer;

        public static SkyboxEditorWindow instance { get { return GetWindow<SkyboxEditorWindow>(); } }

        private void OnEnable() { EnsureDependencies(); }

        void OnFocus() { OnEnable(); }

        [MenuItem("Window/Skybox Editor")]
        static void Init()
        {

            SkyboxEditorWindow window = (SkyboxEditorWindow)EditorWindow.GetWindow(typeof(SkyboxEditorWindow));
            //SkyboxEditorWindow.selectedConfiguration = new SkyboxConfiguration();
            window.minSize = new Vector2(500, 500);
            window.Show();
            window.InitializeWindow();
        }

        public void InitializeWindow()
        {
            if (Application.isPlaying)
            {
                if (SkyboxController.i != null)
                {
                    isPaused = SkyboxController.i.IsPaused();
                }

            }
            EnsureDependencies();
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 0, position.width - 20, position.height));

            GUILayout.Space(32);
            RenderConfigurations();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Space(32);
            RenderTimePanel();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(12);

            panelScrollPos = EditorGUILayout.BeginScrollView(panelScrollPos, GUILayout.Width(position.width - 20), GUILayout.Height(position.height - 90));
            GUILayout.Space(32);
            showBackgroundLayer = EditorGUILayout.Foldout(showBackgroundLayer, "BG Layer", true);
            if (showBackgroundLayer)
            {
                RenderBackgroundColorLayer();
            }
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Space(32);
            showAmbienLayer = EditorGUILayout.Foldout(showAmbienLayer, "Ambient Layer", true);
            if (showAmbienLayer)
            {
                RenderAmbientLayer();
            }
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Space(32);
            showFogLayer = EditorGUILayout.Foldout(showFogLayer, "Fog Layer", true);
            if (showFogLayer)
            {
                RenderFogLayer();
            }
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Space(32);
            showDLLayer = EditorGUILayout.Foldout(showDLLayer, "Directional Light Layer", true);
            if (showDLLayer)
            {
                RenderDirectionalLightLayer();
            }
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Space(32);

            // Texture Layers
            for (int i = 0; i < selectedConfiguration.textureLayers.Count; i++)
            {
                // Name and buttons
                EditorGUILayout.BeginHorizontal();
                selectedConfiguration.textureLayers[i].expandedInEditor = EditorGUILayout.Foldout(selectedConfiguration.textureLayers[i].expandedInEditor, "", true);
                selectedConfiguration.textureLayers[i].nameInEditor = EditorGUILayout.TextField(selectedConfiguration.textureLayers[i].nameInEditor);

                if (GUILayout.Button("Remove", GUILayout.Width(100), GUILayout.ExpandWidth(false)))
                {
                    selectedConfiguration.textureLayers.RemoveAt(i);
                    break;
                }

                if (i == 0)
                {
                    GUI.enabled = false;
                }

                if (GUILayout.Button("Move up", GUILayout.Width(100), GUILayout.ExpandWidth(false)))
                {
                    TextureLayer temp = selectedConfiguration.textureLayers[i - 1];
                    selectedConfiguration.textureLayers[i - 1] = selectedConfiguration.textureLayers[i];
                    selectedConfiguration.textureLayers[i] = temp;
                    break;
                }

                GUI.enabled = true;

                if (i == selectedConfiguration.textureLayers.Count - 1)
                {
                    GUI.enabled = false;
                }

                if (GUILayout.Button("Move down", GUILayout.Width(100), GUILayout.ExpandWidth(false)))
                {
                    TextureLayer temp = selectedConfiguration.textureLayers[i + 1];
                    selectedConfiguration.textureLayers[i + 1] = selectedConfiguration.textureLayers[i];
                    selectedConfiguration.textureLayers[i] = temp;
                    break;
                }

                EditorGUILayout.EndHorizontal();

                GUI.enabled = true;

                if (selectedConfiguration.textureLayers[i].expandedInEditor)
                {
                    EditorGUILayout.Separator();
                    RenderTextureLayer(selectedConfiguration.textureLayers[i]);
                }


                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                GUILayout.Space(32);
            }

            GUI.enabled = true;

            //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            if (GUILayout.Button("+", GUILayout.MaxWidth(100)))
            {
                selectedConfiguration.textureLayers.Add(new TextureLayer("Tex Layer " + (selectedConfiguration.textureLayers.Count + 1)));
            }

            GUILayout.Space(120);
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
            if (GUI.changed)
            {
                ApplyOnMaterial();
            }
        }

        private void Update()
        {
            if (selectedConfiguration == null || isPaused)
            {
                return;
            }

            timeOfTheDay += Time.deltaTime;
            timeOfTheDay = Mathf.Clamp(timeOfTheDay, 0.01f, 24);

            ApplyOnMaterial();

            if (timeOfTheDay >= 24)
            {
                timeOfTheDay = 0.01f;
            }
        }

        private void EnsureDependencies()
        {
            if (selectedConfiguration == null)
            {
                UpdateConfigurationsList();
            }

            UpdateMaterial();

            EditorUtility.SetDirty(selectedConfiguration);

            // Cache directional light reference
            directionalLight = GameObject.FindObjectsOfType<Light>(true).Where(s => s.type == LightType.Directional).FirstOrDefault();
        }

        float lastLayerCount = 0;
        //bool initialized = false;
        MaterialReferenceContainer.Mat_Layer matLayer = null;

        void InitializeMaterial()
        {
            lastLayerCount = selectedConfiguration.textureLayers.Count;
            matLayer = MaterialReferenceContainer.i.GetMat_LayerForLayers(selectedConfiguration.textureLayers.Count);

            if (matLayer == null)
            {
                matLayer = MaterialReferenceContainer.i.materials[0];
            }

            selectedMat = matLayer.material;
            selectedConfiguration.ResetMaterial(selectedMat, matLayer.maxLayer);
            RenderSettings.skybox = selectedMat;
            //initialized = true;
        }

        private void UpdateMaterial()
        {
            if (lastLayerCount == selectedConfiguration.textureLayers.Count)
            {
                return;
            }
            InitializeMaterial();
        }

        SkyboxConfiguration AddNewConfiguration(string name)
        {
            SkyboxConfiguration temp = null;
            if (configurations == null || configurations.Count < 1)
            {
                temp = ScriptableObject.CreateInstance<SkyboxConfiguration>();
            }
            else
            {
                temp = Instantiate<SkyboxConfiguration>(configurations[0]);
            }
            //selectedConfiguration = ScriptableObject.CreateInstance<SkyboxConfiguration>();
            temp.skyboxID = name;

            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Scripts/Resources/Skybox Configurations/" + name + ".asset");
            AssetDatabase.CreateAsset(temp, path);
            AssetDatabase.SaveAssets();
            return temp;
        }

        private void RenderConfigurations()
        {
            GUILayout.Label("Configurations", EditorStyles.boldLabel);

            GUIStyle tStyle = new GUIStyle();
            tStyle.alignment = TextAnchor.MiddleCenter;
            tStyle.margin = new RectOffset(150, 200, 0, 0);
            GUILayout.Label("Loaded: " + selectedConfiguration.skyboxID);
            GUILayout.BeginHorizontal(tStyle);

            if (creatingNewConfig)
            {
                GUILayout.Label("Name");
                newConfigName = EditorGUILayout.TextField(newConfigName, GUILayout.Width(200));

                if (GUILayout.Button("Create", GUILayout.Width(50)))
                {
                    // Make new configuration
                    selectedConfiguration = AddNewConfiguration(newConfigName);

                    // Update configuration list
                    UpdateConfigurationsList();
                    creatingNewConfig = false;
                }

                if (GUILayout.Button("Cancel", GUILayout.Width(50)))
                {
                    creatingNewConfig = false;
                }
            }
            else
            {
                newConfigIndex = EditorGUILayout.Popup(selectedConfigurationIndex, configurationNames.ToArray(), GUILayout.Width(200));

                if (newConfigIndex != selectedConfigurationIndex)
                {
                    selectedConfiguration = configurations[newConfigIndex];
                    selectedConfigurationIndex = newConfigIndex;
                }

                if (GUILayout.Button("+", GUILayout.Width(50)))
                {
                    creatingNewConfig = true;
                }
            }


            GUILayout.EndHorizontal();
        }

        private void UpdateConfigurationsList()
        {
            SkyboxConfiguration[] tConfigurations = Resources.LoadAll<SkyboxConfiguration>("Skybox Configurations/");
            configurations = new List<SkyboxConfiguration>(tConfigurations);
            configurationNames = new List<string>();

            // If no configurations exist, make and select new one.
            if (configurations == null || configurations.Count < 1)
            {
                selectedConfiguration = AddNewConfiguration("Generic Skybox");

                configurations = new List<SkyboxConfiguration>();
                configurations.Add(selectedConfiguration);
            }

            if (selectedConfiguration == null)
            {
                selectedConfiguration = configurations[0];
            }

            for (int i = 0; i < configurations.Count; i++)
            {
                configurationNames.Add(configurations[i].skyboxID);
                if (selectedConfiguration == configurations[i])
                {
                    selectedConfigurationIndex = i;
                }
            }

            InitializeMaterial();

            //isPaused = true;

            if (!Application.isPlaying)
            {
                isPaused = true;
            }


        }

        private void RenderTimePanel()
        {

            GUILayout.Label("Preview", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Time : " + timeOfTheDay, EditorStyles.label, GUILayout.MinWidth(100));
            timeOfTheDay = EditorGUILayout.Slider(timeOfTheDay, 0.01f, 24.00f, GUILayout.MinWidth(100), GUILayout.MaxWidth(300));
            EditorGUILayout.LabelField((GetNormalizedDayTime() * 100).ToString("f2"), GUILayout.MaxWidth(50));

            if (isPaused)
            {
                if (GUILayout.Button("Play"))
                {
                    ResumeTime();
                }
            }
            else
            {
                if (GUILayout.Button("Pause"))
                {
                    PauseTime();
                }
            }

            GUILayout.EndHorizontal();
        }

        void ResumeTime()
        {
            isPaused = false;
            if (Application.isPlaying)
            {
                if (SkyboxController.i != null)
                {
                    SkyboxController.i.ResumeTime(true, timeOfTheDay);
                }
            }
        }

        void PauseTime()
        {
            isPaused = true;
            if (Application.isPlaying)
            {
                if (SkyboxController.i != null)
                {
                    SkyboxController.i.PauseTime();
                }
            }
        }

        void RenderBackgroundColorLayer()
        {

            //GUILayout.Label("Background Layer", EditorStyles.boldLabel);
            selectedConfiguration.skyColor = EditorGUILayout.GradientField(new GUIContent("Sky Color"), selectedConfiguration.skyColor);
            selectedConfiguration.groundColor = EditorGUILayout.GradientField("Ground Color", selectedConfiguration.groundColor);
            RenderHorizonLayer();
        }

        void RenderHorizonLayer()
        {
            selectedConfiguration.horizonColor = EditorGUILayout.GradientField("Horizon Color", selectedConfiguration.horizonColor);
            EditorGUILayout.Separator();
            RenderTransitioningFloat(selectedConfiguration.horizonHeight, "Horizon Height");

            EditorGUILayout.Separator();
            RenderTransitioningFloat(selectedConfiguration.horizonWidth, "Horizon Width");
        }

        void RenderAmbientLayer()
        {

            //GUILayout.Label("Ambient Layer", EditorStyles.boldLabel);
            selectedConfiguration.ambientTrilight = EditorGUILayout.Toggle("Use Gradient", selectedConfiguration.ambientTrilight);

            if (selectedConfiguration.ambientTrilight)
            {
                selectedConfiguration.ambientSkyColor = EditorGUILayout.GradientField("Ambient Sky Color", selectedConfiguration.ambientSkyColor);
                selectedConfiguration.ambientEquatorColor = EditorGUILayout.GradientField("Ambient Equator Color", selectedConfiguration.ambientEquatorColor);
                selectedConfiguration.ambientGroundColor = EditorGUILayout.GradientField("Ambient Ground Color", selectedConfiguration.ambientGroundColor);
            }

        }

        void RenderFogLayer()
        {

            //GUILayout.Label("Fog Layer", EditorStyles.boldLabel);
            selectedConfiguration.useFog = EditorGUILayout.Toggle("Use Fog", selectedConfiguration.useFog);
            if (selectedConfiguration.useFog)
            {
                selectedConfiguration.fogColor = EditorGUILayout.GradientField("Fog Color", selectedConfiguration.fogColor);
                selectedConfiguration.fogMode = (FogMode)EditorGUILayout.EnumPopup("Fog Mode", selectedConfiguration.fogMode);

                switch (selectedConfiguration.fogMode)
                {
                    case FogMode.Linear:
                        selectedConfiguration.fogStartDistance = EditorGUILayout.FloatField("Start Distance: ", selectedConfiguration.fogStartDistance);
                        selectedConfiguration.fogEndDistance = EditorGUILayout.FloatField("End Distance: ", selectedConfiguration.fogEndDistance);
                        break;
                    default:
                        selectedConfiguration.fogDensity = EditorGUILayout.FloatField("Density: ", selectedConfiguration.fogDensity);
                        break;
                }
            }

        }

        void RenderDirectionalLightLayer()
        {

            //GUILayout.Label("Directional Light Layer", EditorStyles.boldLabel);
            selectedConfiguration.useDirectionalLight = EditorGUILayout.Toggle("Use Directional Light", selectedConfiguration.useDirectionalLight);

            if (!selectedConfiguration.useDirectionalLight)
            {
                return;
            }
            selectedConfiguration.directionalLightLayer.lightColor = EditorGUILayout.GradientField("Light Color", selectedConfiguration.directionalLightLayer.lightColor);
            selectedConfiguration.directionalLightLayer.tintColor = EditorGUILayout.GradientField(new GUIContent("Tint Color"), selectedConfiguration.directionalLightLayer.tintColor, true);
            GUILayout.Space(10);

            GUILayout.Label("Light Intensity");
            for (int i = 0; i < selectedConfiguration.directionalLightLayer.intensity.Count; i++)
            {
                GUILayout.BeginHorizontal();
                selectedConfiguration.directionalLightLayer.intensity[i].percentage = EditorGUILayout.FloatField("%", selectedConfiguration.directionalLightLayer.intensity[i].percentage);
                selectedConfiguration.directionalLightLayer.intensity[i].value = EditorGUILayout.FloatField("intensity", selectedConfiguration.directionalLightLayer.intensity[i].value);

                if (GUILayout.Button("Capture"))
                {
                    selectedConfiguration.directionalLightLayer.intensity[i].percentage = GetNormalizedDayTime() * 100;
                    selectedConfiguration.directionalLightLayer.intensity[i].value = directionalLight.intensity;
                    break;
                }

                if (GUILayout.Button("Remove"))
                {
                    selectedConfiguration.directionalLightLayer.intensity.RemoveAt(i);
                    break;
                }
                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+", GUILayout.MaxWidth(100)))
            {
                selectedConfiguration.directionalLightLayer.intensity.Add(new TransitioningFloat(GetNormalizedDayTime() * 100, directionalLight.intensity));
            }

            GUILayout.Space(10);

            GUILayout.Label("Light Direction");
            for (int i = 0; i < selectedConfiguration.directionalLightLayer.lightDirection.Count; i++)
            {
                GUILayout.BeginHorizontal();
                selectedConfiguration.directionalLightLayer.lightDirection[i].percentage = EditorGUILayout.FloatField("%", selectedConfiguration.directionalLightLayer.lightDirection[i].percentage);
                selectedConfiguration.directionalLightLayer.lightDirection[i].value = EditorGUILayout.Vector4Field("Direction", selectedConfiguration.directionalLightLayer.lightDirection[i].value);

                if (GUILayout.Button("Capture"))
                {
                    selectedConfiguration.directionalLightLayer.lightDirection[i].percentage = GetNormalizedDayTime() * 100;
                    selectedConfiguration.directionalLightLayer.lightDirection[i].value = QuaternionToVector4(directionalLight.transform.rotation);
                    break;
                }

                if (GUILayout.Button("Remove"))
                {
                    selectedConfiguration.directionalLightLayer.lightDirection.RemoveAt(i);
                    break;
                }
                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+", GUILayout.MaxWidth(100)))
            {

                selectedConfiguration.directionalLightLayer.lightDirection.Add(new TransitioningVector4(GetNormalizedDayTime() * 100, QuaternionToVector4(directionalLight.transform.rotation)));
            }


        }

        void RenderTextureLayer(TextureLayer layer)
        {

            // Time Span
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("Time Span", GUILayout.Width(100), GUILayout.ExpandWidth(false));
            layer.timeSpan_start = EditorGUILayout.FloatField("Starts", layer.timeSpan_start, GUILayout.Width(200), GUILayout.ExpandWidth(false));
            layer.timeSpan_End = EditorGUILayout.FloatField("Ends", layer.timeSpan_End, GUILayout.Width(200), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            // Fading
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            //EditorGUILayout.LabelField("Fading", GUILayout.Width(100), GUILayout.ExpandWidth(false));
            layer.fadingIn = EditorGUILayout.FloatField("Fade Time", layer.fadingIn, GUILayout.Width(200), GUILayout.ExpandWidth(false));
            //layer.fadingOut = EditorGUILayout.FloatField("Out", layer.fadingOut, GUILayout.Width(200), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            // Tint
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));

            layer.tintercentage = EditorGUILayout.FloatField("Tint", layer.tintercentage, GUILayout.Width(200), GUILayout.ExpandWidth(false));

            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            layer.isRadial = EditorGUILayout.Toggle("Radial", layer.isRadial);

            // Texture
            //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("Texture", GUILayout.Width(100), GUILayout.ExpandWidth(false));
            layer.texture = (Texture2D)EditorGUILayout.ObjectField(layer.texture, typeof(Texture2D), false);
            GUILayout.EndHorizontal();

            // Normal Texture
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("Normal Map", GUILayout.Width(100), GUILayout.ExpandWidth(false));
            layer.textureNormal = (Texture2D)EditorGUILayout.ObjectField(layer.textureNormal, typeof(Texture2D), false);
            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            // Render Distance


            //EditorGUILayout.Separator();
            // Normal Intensity
            layer.speed = EditorGUILayout.FloatField("Normal Intensity", layer.normalIntensity, GUILayout.Width(200), GUILayout.ExpandWidth(false));

            EditorGUILayout.Separator();

            // Tiling
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("Tiling", GUILayout.Width(100), GUILayout.ExpandWidth(false));
            layer.tiling = EditorGUILayout.Vector2Field("", layer.tiling, GUILayout.Width(200), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
            EditorGUILayout.Separator();

            // Speed
            layer.speed = EditorGUILayout.FloatField("Speed", layer.speed, GUILayout.Width(200), GUILayout.ExpandWidth(false));
            EditorGUILayout.Separator();

            // Position
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("Position", GUILayout.Width(100), GUILayout.ExpandWidth(false));
            EditorGUILayout.BeginVertical();

            if (layer.position.Count == 0)
            {
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.ExpandWidth(false)))
                {
                    Vector2 tLastPos = Vector2.zero;
                    if (layer.position.Count != 0)
                    {
                        tLastPos = layer.position[layer.position.Count - 1].value;
                    }
                    layer.position.Add(new TransitioningVector2(GetNormalizedDayTime() * 100, tLastPos));
                }
            }

            for (int i = 0; i < layer.position.Count; i++)
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));

                layer.position[i].percentage = EditorGUILayout.FloatField(layer.position[i].percentage, GUILayout.Width(50), GUILayout.ExpandWidth(false));
                GUILayout.Space(10);
                layer.position[i].value = EditorGUILayout.Vector2Field("", layer.position[i].value, GUILayout.Width(200), GUILayout.ExpandWidth(false));

                GUILayout.Space(20);
                if (GUILayout.Button("Remove", GUILayout.Width(100), GUILayout.ExpandWidth(false)))
                {
                    layer.position.RemoveAt(i);
                    //break;
                }

                if (i == (layer.position.Count - 1))
                {
                    GUILayout.Space(20);
                    if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.ExpandWidth(false)))
                    {
                        Vector2 tLastPos = Vector2.zero;
                        if (layer.position.Count != 0)
                        {
                            tLastPos = layer.position[layer.position.Count - 1].value;
                        }
                        layer.position.Add(new TransitioningVector2(GetNormalizedDayTime() * 100, tLastPos));
                        //break;
                    }
                }

                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("Color", GUILayout.Width(100), GUILayout.ExpandWidth(false));
            EditorGUILayout.GradientField(new GUIContent(""), layer.color, true, GUILayout.Width(200), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
        }

        void RenderHorizontalFloatField(ref float val, string label)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, GUILayout.Width(100), GUILayout.ExpandWidth(false));
            val = EditorGUILayout.FloatField(val, GUILayout.Width(200), GUILayout.ExpandWidth(false));
            //EditorGUILayout.FloatField("Out", layer.fadingOut, GUILayout.Width(200), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
        }

        void RenderTransitioningFloat(List<TransitioningFloat> _list, string label)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, GUILayout.Width(100), GUILayout.ExpandWidth(false));
            EditorGUILayout.BeginVertical();

            if (_list.Count == 0)
            {
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.ExpandWidth(false)))
                {
                    float tLast = 0;
                    if (_list.Count != 0)
                    {
                        tLast = _list[_list.Count - 1].value;
                    }
                    _list.Add(new TransitioningFloat(GetNormalizedDayTime() * 100, tLast));
                }
            }

            for (int i = 0; i < _list.Count; i++)
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));

                _list[i].percentage = EditorGUILayout.FloatField(_list[i].percentage, GUILayout.Width(50), GUILayout.ExpandWidth(false));
                GUILayout.Space(10);
                _list[i].value = EditorGUILayout.FloatField(_list[i].value, GUILayout.Width(200), GUILayout.ExpandWidth(false));

                GUILayout.Space(20);
                if (GUILayout.Button("Remove", GUILayout.Width(100), GUILayout.ExpandWidth(false)))
                {
                    _list.RemoveAt(i);
                }

                if (i == (_list.Count - 1))
                {
                    GUILayout.Space(20);
                    if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.ExpandWidth(false)))
                    {
                        float tLast = 0;
                        if (_list.Count != 0)
                        {
                            tLast = _list[_list.Count - 1].value;
                        }
                        _list.Add(new TransitioningFloat(GetNormalizedDayTime() * 100, tLast));
                        //break;
                    }
                }

                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        Vector4 QuaternionToVector4(Quaternion rot) { return new Vector4(rot.x, rot.y, rot.z, rot.w); }

        private void ApplyOnMaterial()
        {
            EnsureDependencies();

            selectedConfiguration.ApplyOnMaterial(selectedMat, timeOfTheDay, GetNormalizedDayTime(), directionalLight);
        }

        private float GetNormalizedDayTime()
        {
            float tTime = 0;

            tTime = timeOfTheDay / 24;

            tTime = Mathf.Clamp(tTime, 0, 1);

            return tTime;
        }
    }

}