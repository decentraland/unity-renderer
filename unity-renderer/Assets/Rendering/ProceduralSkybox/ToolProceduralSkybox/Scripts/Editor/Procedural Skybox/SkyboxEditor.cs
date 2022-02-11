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
        public float lifecycleDuration = 1;

        private Material selectedMat;
        private Vector2 panelScrollPos;
        private Light directionalLight;
        private bool creatingNewConfig;
        private string newConfigName;
        private bool overridingController;

        private bool showBackgroundLayer;
        private bool showAmbienLayer;
        private bool showFogLayer;
        private bool showDLLayer;
        private bool showAvatarLayer;
        private bool showTimelineTags;
        private bool showHorizonLayer;
        private MaterialReferenceContainer.Mat_Layer matLayer = null;

        private GUIStyle foldoutStyle;
        private GUIStyle renderingMarkerStyle;
        private GUIStyle configurationStyle;
        private GUIStyle percentagePartStyle;
        private GUIStyle customLabelStyle;

        private List<string> renderingOrderList;

        public static SkyboxEditorWindow instance { get { return GetWindow<SkyboxEditorWindow>(); } }

        #region Unity Callbacks

        private void OnEnable() { EnsureDependencies(); }

        private void OnDestroy()
        {
            // If in play mode and editor is closed
            // Transfer control back to skybox controller with the new values in the editor
            if (SkyboxController.i == null)
            {
                return;
            }

            if (Application.isPlaying && SkyboxController.i != null)
            {
                overridingController = SkyboxController.i.GetControlBackFromEditor(selectedConfiguration.name, timeOfTheDay, lifecycleDuration, isPaused);
            }
        }

        void OnFocus() { EnsureDependencies(); }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 0, position.width - 20, position.height));

            GUILayout.Space(32);
            RenderConfigurations();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Space(32);
            RenderTimePanel();
            GUILayout.Space(12);

            showTimelineTags = EditorGUILayout.Foldout(showTimelineTags, "Timeline Tags", true);

            if (showTimelineTags)
            {
                EditorGUI.indentLevel++;
                RenderTimelineTags();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(5);

            panelScrollPos = EditorGUILayout.BeginScrollView(panelScrollPos, "box");

            GUILayout.Space(10);

            showBackgroundLayer = EditorGUILayout.Foldout(showBackgroundLayer, "BG Layer", true);
            if (showBackgroundLayer)
            {
                EditorGUI.indentLevel++;
                RenderBackgroundColorLayer();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Space(32);

            showHorizonLayer = EditorGUILayout.Foldout(showHorizonLayer, "Horizon Plane", true);
            if (showHorizonLayer)
            {
                EditorGUI.indentLevel++;
                RenderHorizonPlaneValues();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Space(32);

            showAmbienLayer = EditorGUILayout.Foldout(showAmbienLayer, "Ambient Layer", true);
            if (showAmbienLayer)
            {
                EditorGUI.indentLevel++;
                RenderAmbientLayer();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Space(32);
            showAvatarLayer = EditorGUILayout.Foldout(showAvatarLayer, "Avatar Layer", true);
            if (showAvatarLayer)
            {
                EditorGUI.indentLevel++;
                RenderAvatarColorLayer();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Space(32);
            showFogLayer = EditorGUILayout.Foldout(showFogLayer, "Fog Layer", true);
            if (showFogLayer)
            {
                EditorGUI.indentLevel++;
                RenderFogLayer();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Space(32);
            showDLLayer = EditorGUILayout.Foldout(showDLLayer, "Directional Light Layer", true);
            if (showDLLayer)
            {
                EditorGUI.indentLevel++;
                RenderDirectionalLightLayer();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Space(32);


            // Render Layers
            RenderTextureLayers(selectedConfiguration.layers);

            EditorGUILayout.EndScrollView();
            GUILayout.Space(10);
            GUILayout.EndArea();

            if (GUI.changed)
            {
                ApplyOnMaterial();
            }
        }

        private void Update()
        {
            if (selectedConfiguration == null)
            {
                return;
            }

            if (isPaused)
            {
                return;
            }

            float timeNormalizationFactor = lifecycleDuration * 60 / 24;
            timeOfTheDay += Time.deltaTime / timeNormalizationFactor;
            timeOfTheDay = Mathf.Clamp(timeOfTheDay, 0.01f, 24);

            ApplyOnMaterial();

            if (timeOfTheDay >= 24)
            {
                timeOfTheDay = 0.01f;
                selectedConfiguration.CycleResets();
            }

            Repaint();
        }

        #endregion

        [MenuItem("Window/Skybox Editor")]
        static void Init()
        {
            SkyboxEditorWindow window = (SkyboxEditorWindow)EditorWindow.GetWindow(typeof(SkyboxEditorWindow));
            window.minSize = new Vector2(500, 500);
            window.Show();
            window.InitializeWindow();
        }

        public void InitializeWindow() { EnsureDependencies(); }

        private void EnsureDependencies()
        {
            if (!Application.isPlaying)
            {
                overridingController = false;
            }

            if (Application.isPlaying && !overridingController)
            {
                TakeControlAtRuntime();
            }

            if (selectedConfiguration == null)
            {
                UpdateConfigurationsList();
            }

            if (matLayer == null || selectedMat == null)
            {
                UpdateMaterial();
            }

            CheckAndAssignAllStyles();

            EditorUtility.SetDirty(selectedConfiguration);

            // Fill rendering order array
            if (renderingOrderList == null)
            {
                renderingOrderList = new List<string>();

                for (int i = 0; i < 5; i++)
                {
                    renderingOrderList.Add((i + 1).ToString());
                }
            }

            if (directionalLight != null)
            {
                return;
            }

            // Cache directional light reference
            directionalLight = GameObject.FindObjectsOfType<Light>(true).Where(s => s.type == LightType.Directional).FirstOrDefault();

            // Make a directional light object if can't find
            if (directionalLight == null)
            {
                GameObject temp = new GameObject("The Sun_Temp");
                // Add the light component
                directionalLight = temp.AddComponent<Light>();
                directionalLight.type = LightType.Directional;
            }
        }

        private void CheckAndAssignAllStyles()
        {
            if (foldoutStyle == null)
            {
                foldoutStyle = new GUIStyle(EditorStyles.foldout);
                foldoutStyle.fixedWidth = 2;
            }

            if (renderingMarkerStyle == null)
            {
                renderingMarkerStyle = new GUIStyle(EditorStyles.label);
                renderingMarkerStyle.fontSize = 18;
            }

            if (configurationStyle == null)
            {
                configurationStyle = new GUIStyle();
                configurationStyle.alignment = TextAnchor.MiddleCenter;
                configurationStyle.margin = new RectOffset(150, 200, 0, 0);
            }

            if (percentagePartStyle == null)
            {
                percentagePartStyle = new GUIStyle();
                percentagePartStyle.alignment = TextAnchor.MiddleCenter;
            }

            if (customLabelStyle == null)
            {
                customLabelStyle = new GUIStyle(EditorStyles.label);
                customLabelStyle.wordWrap = true;
            }
        }

        void TakeControlAtRuntime()
        {
            if (SkyboxController.i != null)
            {
                isPaused = SkyboxController.i.IsPaused();
                lifecycleDuration = (float)SkyboxController.i.lifecycleDuration;
                selectedConfiguration = SkyboxController.i.GetCurrentConfiguration();
                overridingController = SkyboxController.i.SetOverrideController(true);
                timeOfTheDay = SkyboxController.i.GetCurrentTimeOfTheDay();
                UpdateConfigurationsList();
            }
        }

        void InitializeMaterial()
        {
            matLayer = MaterialReferenceContainer.i.GetMat_LayerForLayers(5);

            if (matLayer == null)
            {
                matLayer = MaterialReferenceContainer.i.materials[0];
            }

            selectedMat = matLayer.material;
            selectedConfiguration.ResetMaterial(selectedMat, matLayer.numberOfSlots);
            RenderSettings.skybox = selectedMat;
        }

        private void UpdateMaterial() { InitializeMaterial(); }

        private SkyboxConfiguration AddNewConfiguration(string name)
        {
            SkyboxConfiguration temp = null;
            temp = ScriptableObject.CreateInstance<SkyboxConfiguration>();
            temp.skyboxID = name;

            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Rendering/ProceduralSkybox/Resources/Skybox Configurations/" + name + ".asset");
            AssetDatabase.CreateAsset(temp, path);
            AssetDatabase.SaveAssets();

            return temp;
        }

        private void RenderConfigurations()
        {
            GUILayout.Label("Configurations", EditorStyles.boldLabel);


            GUILayout.Label("Loaded: " + selectedConfiguration.skyboxID);
            GUILayout.BeginHorizontal(configurationStyle);

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

                    if (Application.isPlaying && SkyboxController.i != null && overridingController)
                    {
                        SkyboxController.i.UpdateConfigurationTimelineEvent(selectedConfiguration);
                    }
                }

                if (GUILayout.Button("Cancel", GUILayout.Width(50)))
                {
                    creatingNewConfig = false;
                }
            }
            else
            {
                EditorGUILayout.BeginVertical();
                newConfigIndex = EditorGUILayout.Popup(selectedConfigurationIndex, configurationNames.ToArray(), GUILayout.Width(200));
                selectedConfiguration = (SkyboxConfiguration)EditorGUILayout.ObjectField(selectedConfiguration, typeof(SkyboxConfiguration), false);

                EditorGUILayout.EndVertical();

                if (newConfigIndex != selectedConfigurationIndex)
                {
                    selectedConfiguration = configurations[newConfigIndex];
                    selectedConfigurationIndex = newConfigIndex;

                    if (Application.isPlaying && SkyboxController.i != null && overridingController)
                    {
                        SkyboxController.i.UpdateConfigurationTimelineEvent(selectedConfiguration);
                    }
                }

                if (selectedConfiguration != configurations[selectedConfigurationIndex])
                {
                    UpdateConfigurationsList();

                    if (Application.isPlaying && SkyboxController.i != null && overridingController)
                    {
                        SkyboxController.i.UpdateConfigurationTimelineEvent(selectedConfiguration);
                    }
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
                configurations[i].skyboxID = configurations[i].name;

                configurationNames.Add(configurations[i].skyboxID);
                if (selectedConfiguration == configurations[i])
                {
                    selectedConfigurationIndex = i;
                }
            }

            InitializeMaterial();

            if (!Application.isPlaying)
            {
                isPaused = true;
            }
        }

        private void RenderTimePanel()
        {

            GUILayout.Label("Preview", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Time : " + timeOfTheDay.ToString("f2"), EditorStyles.label, GUILayout.Width(70));

            EditorGUILayout.Space(20);

            EditorGUILayout.BeginVertical();
            timeOfTheDay = EditorGUILayout.Slider(timeOfTheDay, 0.01f, 24.00f, GUILayout.MinWidth(150));
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("cycle (minutes)", GUILayout.Width(95));
            lifecycleDuration = EditorGUILayout.FloatField(lifecycleDuration, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField((GetNormalizedDayTime() * 100).ToString("f2") + "%", GUILayout.MaxWidth(50));

            if (isPaused)
            {
                if (GUILayout.Button("Play", GUILayout.ExpandWidth(false)))
                {
                    ResumeTime();
                }
            }
            else
            {
                if (GUILayout.Button("Pause", GUILayout.ExpandWidth(false)))
                {
                    PauseTime();
                }
            }

            GUILayout.EndHorizontal();
        }

        void ResumeTime() { isPaused = false; }

        void PauseTime() { isPaused = true; }

        #region Render Base Layeyrs

        void RenderBackgroundColorLayer()
        {
            RenderColorGradientField(selectedConfiguration.skyColor, "Sky Color", 0, 24);
            RenderColorGradientField(selectedConfiguration.horizonColor, "Horizon Color", 0, 24);
            RenderColorGradientField(selectedConfiguration.groundColor, "Ground Color", 0, 24);
            EditorGUILayout.Separator();
            RenderTransitioningFloat(selectedConfiguration.horizonHeight, "Horizon Height", "%", "value", true, -1, 1);

            EditorGUILayout.Space(10);
            RenderTransitioningFloat(selectedConfiguration.horizonWidth, "Horizon Width", "%", "value", true, -1, 1);

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Horizon Mask", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            // Horizon Mask
            RenderTexture("Texture", ref selectedConfiguration.horizonMask);

            // Horizon Mask values
            EditorGUILayout.LabelField("Horizon Mask Values", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            // Tiling
            RenderVector2Field("Tiling", ref selectedConfiguration.horizonMaskTiling);
            // Offset
            RenderVector2Field("Offset", ref selectedConfiguration.horizonMaskOffset);
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
        }

        void RenderHorizonPlaneValues()
        {
            // Horizon Plane
            RenderTexture("Texture", ref selectedConfiguration.horizonPlaneTexture);

            // Horizon Plane values
            EditorGUILayout.LabelField("Horizon Plane Values", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            // Tiling
            RenderVector2Field("Tiling", ref selectedConfiguration.horizonPlaneTiling);
            // Offset
            RenderVector2Field("Offset", ref selectedConfiguration.horizonPlaneOffset);
            EditorGUI.indentLevel--;

            // Horizon Plane color
            RenderColorGradientField(selectedConfiguration.horizonPlaneColor, "Horizon Plane Color", 0, 24);

            // Horizon light intensity
            RenderFloatFieldAsSlider("Light Intensity", ref selectedConfiguration.horizonLightIntensity, 0, 1);

            // Horizon Height
            RenderTransitioningFloat(selectedConfiguration.horizonPlaneHeight, "Horizon Plane Height", "%", "value", true, -1, 0);

            EditorGUILayout.Space(10);

            // Plane smooth range
            RenderMinMaxSlider("Plane Smoothness", ref selectedConfiguration.horizonPlaneSmoothRange.x, ref selectedConfiguration.horizonPlaneSmoothRange.y, 0, 1);

        }

        void RenderAmbientLayer()
        {
            selectedConfiguration.ambientTrilight = EditorGUILayout.Toggle("Use Gradient", selectedConfiguration.ambientTrilight);

            if (selectedConfiguration.ambientTrilight)
            {
                RenderColorGradientField(selectedConfiguration.ambientSkyColor, "Ambient Sky Color", 0, 24, true);
                RenderColorGradientField(selectedConfiguration.ambientEquatorColor, "Ambient Equator Color", 0, 24, true);
                RenderColorGradientField(selectedConfiguration.ambientGroundColor, "Ambient Ground Color", 0, 24, true);
            }

        }

        void RenderAvatarColorLayer()
        {
            EditorGUILayout.LabelField("In World", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            // Avatar Color
            selectedConfiguration.useAvatarGradient = EditorGUILayout.Toggle("Color Gradient", selectedConfiguration.useAvatarGradient, GUILayout.Width(500));

            if (selectedConfiguration.useAvatarGradient)
            {
                RenderColorGradientField(selectedConfiguration.avatarTintGradient, "Tint Gradient", 0, 24, true);
            }
            else
            {
                selectedConfiguration.avatarTintColor = EditorGUILayout.ColorField("Tint Color", selectedConfiguration.avatarTintColor, GUILayout.Width(400));
                EditorGUILayout.Separator();
            }

            // Avatar Light Direction
            selectedConfiguration.useAvatarRealtimeDLDirection = EditorGUILayout.Toggle("Realtime DL Direction", selectedConfiguration.useAvatarRealtimeDLDirection);

            if (!selectedConfiguration.useAvatarRealtimeDLDirection)
            {
                RenderVector3Field("Light Direction", ref selectedConfiguration.avatarLightConstantDir);
            }

            EditorGUILayout.Separator();

            // Avatar Light Color
            selectedConfiguration.useAvatarRealtimeLightColor = EditorGUILayout.Toggle("Realtime Light Color", selectedConfiguration.useAvatarRealtimeLightColor);

            if (!selectedConfiguration.useAvatarRealtimeLightColor)
            {
                RenderColorGradientField(selectedConfiguration.avatarLightColorGradient, "Light Color", 0, 24);
                EditorGUILayout.Separator();
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField("In Editor (Backpack)", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            selectedConfiguration.avatarEditorTintColor = EditorGUILayout.ColorField("Tint Color", selectedConfiguration.avatarEditorTintColor, GUILayout.Width(400));
            RenderVector3Field("Light Direction", ref selectedConfiguration.avatarEditorLightDir);
            selectedConfiguration.avatarEditorLightColor = EditorGUILayout.ColorField("Light Color", selectedConfiguration.avatarEditorLightColor, GUILayout.Width(400));
            EditorGUILayout.Separator();
            EditorGUI.indentLevel--;
        }

        void RenderFogLayer()
        {
            selectedConfiguration.useFog = EditorGUILayout.Toggle("Use Fog", selectedConfiguration.useFog);
            if (selectedConfiguration.useFog)
            {
                RenderColorGradientField(selectedConfiguration.fogColor, "Fog Color", 0, 24);
                selectedConfiguration.fogMode = (FogMode)EditorGUILayout.EnumPopup("Fog Mode", selectedConfiguration.fogMode);

                switch (selectedConfiguration.fogMode)
                {
                    case FogMode.Linear:
                        RenderFloatField("Start Distance:", ref selectedConfiguration.fogStartDistance);
                        RenderFloatField("End Distance:", ref selectedConfiguration.fogEndDistance);
                        break;
                    default:
                        RenderFloatField("Density: ", ref selectedConfiguration.fogDensity);
                        break;
                }

                RenderFloatFieldAsSlider("Fog Intensity", ref selectedConfiguration.fogIntensityOnLayer, 0, 1);
            }

        }

        void RenderDirectionalLightLayer()
        {
            selectedConfiguration.useDirectionalLight = EditorGUILayout.Toggle("Use Directional Light", selectedConfiguration.useDirectionalLight);

            if (!selectedConfiguration.useDirectionalLight)
            {
                return;
            }
            RenderColorGradientField(selectedConfiguration.directionalLightLayer.lightColor, "Light Color", 0, 24);
            RenderColorGradientField(selectedConfiguration.directionalLightLayer.tintColor, "Tint Color", 0, 24, true);

            GUILayout.Space(10);

            // Light Intesity
            RenderTransitioningFloat(selectedConfiguration.directionalLightLayer.intensity, "Light Intensity", "%", "Intensity");

            GUILayout.Space(10);

            RenderTransitioningQuaternionAsVector3(selectedConfiguration.directionalLightLayer.lightDirection, "Light Direction", "%", "Direction", GetDLDirection, 0, 24);
        }

        private Quaternion GetDLDirection() { return directionalLight.transform.rotation; }

        private void RenderTimelineTags()
        {
            if (selectedConfiguration.timelineTags == null)
            {
                selectedConfiguration.timelineTags = new List<TimelineTagsDuration>();
            }

            for (int i = 0; i < selectedConfiguration.timelineTags.Count; i++)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));

                // Text field for name of event
                EditorGUILayout.LabelField("Name: ", GUILayout.Width(50));
                selectedConfiguration.timelineTags[i].tag = EditorGUILayout.TextField(selectedConfiguration.timelineTags[i].tag, GUILayout.Width(70));

                // Start time
                EditorGUILayout.LabelField("Start", GUILayout.Width(45));
                GUILayout.Space(0);
                selectedConfiguration.timelineTags[i].startTime = EditorGUILayout.FloatField(selectedConfiguration.timelineTags[i].startTime, GUILayout.Width(50));
                ClampToDayTime(ref selectedConfiguration.timelineTags[i].startTime);

                // End time
                if (!selectedConfiguration.timelineTags[i].isTrigger)
                {
                    EditorGUILayout.LabelField("End", GUILayout.Width(40));
                    GUILayout.Space(0);
                    selectedConfiguration.timelineTags[i].endTime = EditorGUILayout.FloatField(selectedConfiguration.timelineTags[i].endTime, GUILayout.Width(50));
                    ClampToDayTime(ref selectedConfiguration.timelineTags[i].endTime);
                }
                else
                {
                    GUILayout.Space(97);
                }

                // no end time
                selectedConfiguration.timelineTags[i].isTrigger = EditorGUILayout.ToggleLeft("Trigger", selectedConfiguration.timelineTags[i].isTrigger, GUILayout.Width(80));

                // Remove Button
                if (GUILayout.Button("-", GUILayout.Width(30)))
                {
                    selectedConfiguration.timelineTags.RemoveAt(i);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);
            if (GUILayout.Button("+", GUILayout.Width(30)))
            {
                selectedConfiguration.timelineTags.Add(new TimelineTagsDuration(timeOfTheDay));
            }
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Render Slots and Layers

        void RenderTextureLayers(List<TextureLayer> layers)
        {

            for (int i = 0; i < layers.Count; i++)
            {
                // Name and buttons
                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                layers[i].enabled = EditorGUILayout.Toggle(layers[i].enabled, GUILayout.Width(20), GUILayout.Height(10));
                GUILayout.Space(10);
                layers[i].expandedInEditor = EditorGUILayout.Foldout(layers[i].expandedInEditor, GUIContent.none, true, foldoutStyle);
                layers[i].nameInEditor = EditorGUILayout.TextField(layers[i].nameInEditor, GUILayout.Width(100), GUILayout.ExpandWidth(false));

                // Slot ID
                layers[i].slotID = EditorGUILayout.Popup(layers[i].slotID, renderingOrderList.ToArray(), GUILayout.Width(50));

                if (i == 0)
                {
                    GUI.enabled = false;
                }
                if (GUILayout.Button(('\u25B2').ToString(), GUILayout.Width(50), GUILayout.ExpandWidth(false)))
                {
                    TextureLayer temp = null;

                    if (i >= 1)
                    {
                        temp = layers[i - 1];
                        layers[i - 1] = layers[i];
                        layers[i] = temp;
                    }
                }

                GUI.enabled = true;

                if (i == layers.Count - 1)
                {
                    GUI.enabled = false;
                }

                if (GUILayout.Button(('\u25BC').ToString(), GUILayout.Width(50), GUILayout.ExpandWidth(false)))
                {
                    TextureLayer temp = null;
                    if (i < (layers.Count - 1))
                    {
                        temp = layers[i + 1];
                        layers[i + 1] = layers[i];
                        layers[i] = temp;
                    }
                    break;
                }

                GUI.enabled = true;

                if (GUILayout.Button("-", GUILayout.Width(50), GUILayout.ExpandWidth(false)))
                {
                    layers.RemoveAt(i);
                    break;
                }

                Color circleColor = Color.green;
                switch (layers[i].renderType)
                {
                    case LayerRenderType.Rendering:
                        circleColor = Color.green;
                        break;
                    case LayerRenderType.NotRendering:
                        circleColor = Color.gray;
                        break;
                    case LayerRenderType.Conflict_Playing:
                        circleColor = Color.yellow;
                        break;
                    case LayerRenderType.Conflict_NotPlaying:
                        circleColor = Color.red;
                        break;
                    default:
                        break;
                }

                Color normalContentColor = GUI.color;
                GUI.color = circleColor;

                EditorGUILayout.LabelField(('\u29BF').ToString(), renderingMarkerStyle, GUILayout.Width(20), GUILayout.Height(20));

                GUI.color = normalContentColor;

                EditorGUILayout.EndHorizontal();

                if (layers[i].expandedInEditor)
                {
                    EditorGUILayout.Separator();
                    EditorGUI.indentLevel++;
                    RenderTextureLayer(layers[i]);

                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                GUILayout.Space(32);
            }

            GUI.enabled = true;

            if (GUILayout.Button("+", GUILayout.MaxWidth(20)))
            {
                layers.Add(new TextureLayer("Tex Layer " + (layers.Count + 1)));
            }
        }

        void RenderTextureLayer(TextureLayer layer)
        {
            EditorGUILayout.Separator();

            // Layer Type
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("Layer Type:", GUILayout.Width(150), GUILayout.ExpandWidth(false));
            layer.layerType = (LayerType)EditorGUILayout.EnumPopup(layer.layerType, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.Separator();

            // Time Span
            RenderSepratedFloatFields("Time Span", "Starts", ref layer.timeSpan_start, "Ends", ref layer.timeSpan_End);
            ClampToDayTime(ref layer.timeSpan_start);
            ClampToDayTime(ref layer.timeSpan_End);

            // Fading
            RenderSepratedFloatFields("Fading", "In", ref layer.fadingInTime, "Out", ref layer.fadingOutTime);

            // Tint
            RenderFloatFieldAsSlider("Tint", ref layer.tintPercentage, 0, 100);

            if (layer.layerType == LayerType.Cubemap)
            {
                RenderCubemapLayer(layer);

            }
            else if (layer.layerType == LayerType.Planar)
            {
                RenderPlanarLayer(layer);

            }
            else if (layer.layerType == LayerType.Radial)
            {
                RenderPlanarLayer(layer, true);
            }
            else if (layer.layerType == LayerType.Satellite)
            {
                RenderSatelliteLayer(layer);
            }
            else if (layer.layerType == LayerType.Particles)
            {
                RenderParticleLayer(layer);
            }
        }

        void RenderCubemapLayer(TextureLayer layer)
        {
            // Cubemap
            RenderCubemapTexture("Cubemap", ref layer.cubemap);

            // Gradient
            RenderColorGradientField(layer.color, "color", layer.timeSpan_start, layer.timeSpan_End, true);

            EditorGUILayout.Separator();

            // Movement Type
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("Movemnt Type", GUILayout.Width(150), GUILayout.ExpandWidth(false));
            layer.movementTypeCubemap = (MovementType)EditorGUILayout.EnumPopup(layer.movementTypeCubemap, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            // Rotation
            if (layer.movementTypeCubemap == MovementType.PointBased)
            {
                RenderTransitioningVector3(layer.rotations_Vector3, "Rotation", "%", "Rot:", layer.timeSpan_start, layer.timeSpan_End);

            }
            else
            {
                RenderVector3Field("Speed", ref layer.speed_Vector3);
            }
        }

        void RenderPlanarLayer(TextureLayer layer, bool isRadial = false)
        {
            // Texture
            RenderTexture("Texture", ref layer.texture);

            // Row and Coloumns
            RenderVector2Field("Rows and Columns", ref layer.flipbookRowsAndColumns);

            // Anim Speed
            RenderFloatField("Anim Speed", ref layer.flipbookAnimSpeed);

            // Gradient
            RenderColorGradientField(layer.color, "color", layer.timeSpan_start, layer.timeSpan_End, true);

            // Tiling
            RenderVector2Field("Tiling", ref layer.tiling);

            // Movement Type
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("Movemnt Type", GUILayout.Width(150), GUILayout.ExpandWidth(false));
            layer.movementTypePlanar_Radial = (MovementType)EditorGUILayout.EnumPopup(layer.movementTypePlanar_Radial, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            EditorGUILayout.Separator();

            if (layer.movementTypePlanar_Radial == MovementType.Speed)
            {
                // Speed
                RenderVector2Field("Speed", ref layer.speed_Vector2);
            }
            else
            {
                // Offset
                RenderTransitioningVector2(layer.offset, "Position", "%", "", layer.timeSpan_start, layer.timeSpan_End);
            }

            EditorGUI.indentLevel--;

            EditorGUILayout.Space(15);

            // Render Distance
            RenderTransitioningFloat(layer.renderDistance, "Render Distance", "", "", true, 0, 20, layer.timeSpan_start, layer.timeSpan_End);

            EditorGUILayout.Space(15);

            // Rotation
            if (!isRadial)
            {
                RenderTransitioningFloat(layer.rotations_float, "Rotation", "", "", true, 0, 360, layer.timeSpan_start, layer.timeSpan_End);
                EditorGUILayout.Separator();
            }

            RenderDistortionVariables(layer);

            EditorGUILayout.Space(10);
        }

        void RenderSatelliteLayer(TextureLayer layer)
        {
            // Texture
            RenderTexture("Texture", ref layer.texture);

            // Row and Coloumns
            RenderVector2Field("Rows and Columns", ref layer.flipbookRowsAndColumns);

            // Anim Speed
            RenderFloatField("Anim Speed", ref layer.flipbookAnimSpeed);

            // Gradient
            RenderColorGradientField(layer.color, "color", layer.timeSpan_start, layer.timeSpan_End, true);

            EditorGUILayout.Space(10);
            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField("Movemnt Type", GUILayout.Width(150), GUILayout.ExpandWidth(false));
            layer.movementTypeSatellite = (MovementType)EditorGUILayout.EnumPopup(layer.movementTypeSatellite, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            EditorGUILayout.Separator();

            if (layer.movementTypeSatellite == MovementType.Speed)
            {
                // Speed
                RenderVector2Field("Speed", ref layer.speed_Vector2);
            }
            else
            {
                // Offset
                RenderTransitioningVector2(layer.offset, "Position", "%", "", layer.timeSpan_start, layer.timeSpan_End);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space(20);

            // Rotation
            RenderTransitioningFloat(layer.rotations_float, "Rotation", "", "", true, 0, 360, layer.timeSpan_start, layer.timeSpan_End);

            EditorGUILayout.Space(12);

            // Size
            RenderTransitioningVector2(layer.satelliteWidthHeight, "Width & Height", "%", "", layer.timeSpan_start, layer.timeSpan_End);
        }

        void RenderParticleLayer(TextureLayer layer)
        {
            // Texture
            RenderTexture("Texture", ref layer.texture);

            // Row and Coloumns
            RenderVector2Field("Rows and Columns", ref layer.flipbookRowsAndColumns);

            // Anim Speed
            RenderFloatField("Anim Speed", ref layer.flipbookAnimSpeed);

            // Gradient
            RenderColorGradientField(layer.color, "color", layer.timeSpan_start, layer.timeSpan_End, true);

            // Tiling
            RenderVector2Field("Tiling", ref layer.particleTiling);

            // Offset
            RenderVector2Field("Offset", ref layer.particlesOffset);

            // Amount
            RenderFloatField("Amount", ref layer.particlesAmount);

            // Size
            RenderSepratedFloatFields("Size", "Min", ref layer.particleMinSize, "Max", ref layer.particleMaxSize);

            // Spread
            RenderSepratedFloatFields("Spread", "Horizontal", ref layer.particlesHorizontalSpread, "Vertical", ref layer.particlesVerticalSpread);

            // Fade
            RenderSepratedFloatFields("Fade", "Min", ref layer.particleMinFade, "Max", ref layer.particleMaxFade);

            // Particle Rotation
            RenderTransitioningVector3(layer.particleRotation, "Rotation", "%", "value", layer.timeSpan_start, layer.timeSpan_End);
        }

        void RenderDistortionVariables(TextureLayer layer)
        {
            layer.distortionExpanded = EditorGUILayout.Foldout(layer.distortionExpanded, "Distortion Values", true, EditorStyles.foldoutHeader);

            if (!layer.distortionExpanded)
            {
                return;
            }

            EditorGUILayout.Space(10);

            EditorGUI.indentLevel++;

            // Distortion Intensity
            RenderTransitioningFloat(layer.distortIntensity, "Intensity", "%", "Value", false, 0, 1, layer.timeSpan_start, layer.timeSpan_End);

            EditorGUILayout.Space(10);

            // Distortion Size
            RenderTransitioningFloat(layer.distortSize, "Size", "%", "Value", false, 0, 1, layer.timeSpan_start, layer.timeSpan_End);

            EditorGUILayout.Space(10);

            // Distortion Speed
            RenderTransitioningVector2(layer.distortSpeed, "Speed", "%", "Value", layer.timeSpan_start, layer.timeSpan_End);

            EditorGUILayout.Space(10);

            // Distortion Sharpness
            RenderTransitioningVector2(layer.distortSharpness, "Sharpness", "%", "Value", layer.timeSpan_start, layer.timeSpan_End);

            EditorGUILayout.Space(10);

            EditorGUI.indentLevel--;
        }

        #endregion

        #region Render simple Variables

        void RenderSepratedFloatFields(string label, string label1, ref float value1, string label2, ref float value2)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label1, GUILayout.Width(90), GUILayout.ExpandWidth(false));
            value1 = EditorGUILayout.FloatField("", value1, GUILayout.Width(90));
            EditorGUILayout.LabelField(label2, GUILayout.Width(90), GUILayout.ExpandWidth(false));
            value2 = EditorGUILayout.FloatField("", value2, GUILayout.Width(90));
            GUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        }

        void RenderFloatField(string label, ref float value)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            value = EditorGUILayout.FloatField(value, GUILayout.Width(90));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        }

        void RenderFloatFieldAsSlider(string label, ref float value, float min, float max)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, GUILayout.Width(120), GUILayout.ExpandWidth(false));
            EditorGUILayout.BeginVertical();
            value = EditorGUILayout.Slider(value, min, max, GUILayout.Width(200));
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        }

        void RenderVector3Field(string label, ref Vector3 value)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            value = EditorGUILayout.Vector3Field("", value, GUILayout.Width(200), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        }

        void RenderVector4Field(string label, ref Vector4 value)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            value = EditorGUILayout.Vector4Field("", value, GUILayout.Width(300), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        }

        void RenderVector2Field(string label, ref Vector2 value)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            value = EditorGUILayout.Vector2Field("", value, GUILayout.Width(200), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        }

        void RenderTexture(string label, ref Texture2D tex)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            tex = (Texture2D)EditorGUILayout.ObjectField(tex, typeof(Texture2D), false, GUILayout.Width(200));
            GUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        }

        void RenderCubemapTexture(string label, ref Cubemap tex)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            tex = (Cubemap)EditorGUILayout.ObjectField(tex, typeof(Cubemap), false, GUILayout.Width(200));
            GUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        }

        void RenderMinMaxSlider(string label, ref float minVal, ref float maxVal, float minLimit, float maxLimit)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(minVal.ToString("f3"), GUILayout.Width(55), GUILayout.ExpandWidth(false));
            EditorGUILayout.MinMaxSlider(ref minVal, ref maxVal, minLimit, maxLimit, GUILayout.Width(150));
            EditorGUILayout.LabelField(maxVal.ToString("f3"), GUILayout.Width(150), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        }

        #endregion

        #region Render Transitioning Variables

        void RenderTransitioningVector3(List<TransitioningVector3> list, string label, string percentTxt, string valueText, float layerStartTime = 0, float layerEndTime = 24)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, GUILayout.Width(120), GUILayout.ExpandWidth(false));
            EditorGUILayout.BeginVertical();

            if (list.Count == 0)
            {
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.ExpandWidth(false)))
                {
                    Vector3 tLastPos = Vector3.zero;
                    if (list.Count != 0)
                    {
                        tLastPos = list[list.Count - 1].value;
                    }
                    list.Add(new TransitioningVector3(GetNormalizedLayerCurrentTime(layerStartTime, layerEndTime) * 100, tLastPos));
                }
            }

            for (int i = 0; i < list.Count; i++)
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));

                if (GUILayout.Button(">", GUILayout.ExpandWidth(false)))
                {
                    timeOfTheDay = GetDayTimeForLayerNormalizedTime(layerStartTime, layerEndTime, list[i].percentage / 100);
                }

                // Percentage
                GUILayout.Label(percentTxt, GUILayout.ExpandWidth(false));

                RenderPercentagePart(layerStartTime, layerEndTime, ref list[i].percentage);

                GUILayout.Space(10);

                GUILayout.Label(valueText, GUILayout.ExpandWidth(false));

                GUILayout.Space(10);
                list[i].value = EditorGUILayout.Vector3Field("", list[i].value, GUILayout.Width(200), GUILayout.ExpandWidth(false));

                GUILayout.Space(20);
                if (GUILayout.Button("Remove", GUILayout.Width(100), GUILayout.ExpandWidth(false)))
                {
                    list.RemoveAt(i);
                }

                if (i == (list.Count - 1))
                {
                    GUILayout.Space(20);
                    if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.ExpandWidth(false)))
                    {
                        Vector3 tLastPos = Vector3.zero;
                        if (list.Count != 0)
                        {
                            tLastPos = list[list.Count - 1].value;
                        }
                        list.Add(new TransitioningVector3(GetNormalizedLayerCurrentTime(layerStartTime, layerEndTime) * 100, tLastPos));
                    }
                }

                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            GUILayout.EndHorizontal();

            Rect lastRect = GUILayoutUtility.GetLastRect();
            lastRect.y -= 5;
            lastRect.height += 10;
            GUI.Box(lastRect, "", EditorStyles.helpBox);
        }

        void RenderTransitioningVector2(List<TransitioningVector2> list, string label, string percentTxt, string valueText, float layerStartTime = 0, float layerEndTime = 24)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, GUILayout.Width(120), GUILayout.ExpandWidth(false));
            EditorGUILayout.BeginVertical();

            if (list.Count == 0)
            {
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.ExpandWidth(false)))
                {
                    Vector2 tLastPos = Vector2.zero;
                    if (list.Count != 0)
                    {
                        tLastPos = list[list.Count - 1].value;
                    }
                    list.Add(new TransitioningVector2(GetNormalizedLayerCurrentTime(layerStartTime, layerEndTime) * 100, tLastPos));
                }
            }

            for (int i = 0; i < list.Count; i++)
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));

                if (GUILayout.Button(">", GUILayout.ExpandWidth(false)))
                {
                    timeOfTheDay = GetDayTimeForLayerNormalizedTime(layerStartTime, layerEndTime, list[i].percentage / 100);
                }

                // Percentage
                GUILayout.Label(percentTxt, GUILayout.ExpandWidth(false));

                RenderPercentagePart(layerStartTime, layerEndTime, ref list[i].percentage);

                GUILayout.Label(valueText, GUILayout.ExpandWidth(false));

                GUILayout.Space(10);
                list[i].value = EditorGUILayout.Vector2Field("", list[i].value, GUILayout.Width(200), GUILayout.ExpandWidth(false));

                GUILayout.Space(20);
                if (GUILayout.Button("Remove", GUILayout.Width(100), GUILayout.ExpandWidth(false)))
                {
                    list.RemoveAt(i);
                }

                if (i == (list.Count - 1))
                {
                    GUILayout.Space(20);
                    if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.ExpandWidth(false)))
                    {
                        Vector2 tLastPos = Vector2.zero;
                        if (list.Count != 0)
                        {
                            tLastPos = list[list.Count - 1].value;
                        }
                        list.Add(new TransitioningVector2(GetNormalizedLayerCurrentTime(layerStartTime, layerEndTime) * 100, tLastPos));
                    }
                }

                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            GUILayout.EndHorizontal();

            Rect lastRect = GUILayoutUtility.GetLastRect();
            lastRect.y -= 5;
            lastRect.height += 10;
            GUI.Box(lastRect, "", EditorStyles.helpBox);
        }

        void RenderTransitioningFloat(List<TransitioningFloat> list, string label, string percentTxt, string valueText, bool slider = false, float min = 0, float max = 1, float layerStartTime = 0, float layerEndTime = 24)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, customLabelStyle, GUILayout.Width(120), GUILayout.ExpandWidth(false));
            EditorGUILayout.BeginVertical();

            if (list.Count == 0)
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.ExpandWidth(false)))
                {
                    float tLast = 0;
                    if (list.Count != 0)
                    {
                        tLast = list[list.Count - 1].value;
                    }
                    list.Add(new TransitioningFloat(GetNormalizedLayerCurrentTime(layerStartTime, layerEndTime) * 100, tLast));
                }
                GUILayout.EndHorizontal();
            }

            for (int i = 0; i < list.Count; i++)
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));

                GUILayout.Space(10);
                if (GUILayout.Button(">", GUILayout.ExpandWidth(false)))
                {
                    timeOfTheDay = GetDayTimeForLayerNormalizedTime(layerStartTime, layerEndTime, list[i].percentage / 100);
                }
                GUILayout.Label(percentTxt, GUILayout.ExpandWidth(false));

                RenderPercentagePart(layerStartTime, layerEndTime, ref list[i].percentage);

                GUILayout.Label(valueText, GUILayout.ExpandWidth(false));

                if (slider)
                {
                    list[i].value = EditorGUILayout.Slider(list[i].value, min, max, GUILayout.Width(200), GUILayout.ExpandWidth(false));
                }
                else
                {
                    list[i].value = EditorGUILayout.FloatField("", list[i].value, GUILayout.Width(200), GUILayout.ExpandWidth(false));
                }


                GUILayout.Space(20);
                if (GUILayout.Button("Remove", GUILayout.Width(100), GUILayout.ExpandWidth(false)))
                {
                    list.RemoveAt(i);
                }

                if (i == (list.Count - 1))
                {
                    GUILayout.Space(20);
                    if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.ExpandWidth(false)))
                    {
                        float tLast = 0;
                        if (list.Count != 0)
                        {
                            tLast = list[list.Count - 1].value;
                        }
                        list.Add(new TransitioningFloat(GetNormalizedLayerCurrentTime(layerStartTime, layerEndTime) * 100, tLast));
                    }
                }

                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            GUILayout.EndHorizontal();

            Rect lastRect = GUILayoutUtility.GetLastRect();
            lastRect.y -= 5;
            lastRect.height += 10;
            GUI.Box(lastRect, "", EditorStyles.helpBox);
        }

        void RenderColorGradientField(Gradient color, string label = "color", float startTime = -1, float endTime = -1, bool hdr = false)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            EditorGUILayout.LabelField(label, GUILayout.Width(150), GUILayout.ExpandWidth(false));

            if (startTime != -1)
            {
                EditorGUILayout.LabelField(startTime + "Hr", GUILayout.Width(65), GUILayout.ExpandWidth(false));
            }

            color = EditorGUILayout.GradientField(new GUIContent(""), color, hdr, GUILayout.Width(250), GUILayout.ExpandWidth(false));

            if (endTime != 1)
            {
                EditorGUILayout.LabelField(endTime + "Hr", GUILayout.Width(65), GUILayout.ExpandWidth(false));
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        }

        void RenderTransitioningQuaternionAsVector3(List<TransitioningQuaternion> list, string label, string percentTxt, string valueText, Func<Quaternion> GetCurrentRotation, float layerStartTime = 0, float layerEndTime = 24)
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            GUILayout.Label(label, GUILayout.Width(120), GUILayout.ExpandWidth(false));

            GUILayout.BeginVertical();

            if (list.Count == 0)
            {
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.ExpandWidth(false)))
                {
                    list.Add(new TransitioningQuaternion(GetNormalizedLayerCurrentTime(layerStartTime, layerEndTime) * 100, GetCurrentRotation()));
                }
                GUILayout.EndHorizontal();
            }

            for (int i = 0; i < list.Count; i++)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(">", GUILayout.ExpandWidth(false)))
                {
                    timeOfTheDay = GetDayTimeForLayerNormalizedTime(layerStartTime, layerEndTime, list[i].percentage / 100);
                }

                GUILayout.Label(percentTxt, GUILayout.ExpandWidth(false));

                RenderPercentagePart(layerStartTime, layerEndTime, ref list[i].percentage);

                GUILayout.Space(10);

                GUILayout.Label(valueText, GUILayout.ExpandWidth(false));

                // Convert Quaternion to Vector3
                list[i].value = Quaternion.Euler(EditorGUILayout.Vector3Field("", list[i].value.eulerAngles, GUILayout.ExpandWidth(false)));

                if (GUILayout.Button("Capture", GUILayout.Width(100), GUILayout.ExpandWidth(false)))
                {
                    selectedConfiguration.directionalLightLayer.lightDirection[i].percentage = GetNormalizedLayerCurrentTime(layerStartTime, layerEndTime) * 100;
                    selectedConfiguration.directionalLightLayer.lightDirection[i].value = GetCurrentRotation();
                    break;
                }

                if (GUILayout.Button("Remove", GUILayout.Width(100), GUILayout.ExpandWidth(false)))
                {
                    list.RemoveAt(i);
                    break;
                }

                if (i == (list.Count - 1))
                {
                    GUILayout.Space(20);
                    if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.ExpandWidth(false)))
                    {
                        list.Add(new TransitioningQuaternion(GetNormalizedLayerCurrentTime(layerStartTime, layerEndTime) * 100, GetCurrentRotation()));
                    }
                }

                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            Rect lastRect = GUILayoutUtility.GetLastRect();
            lastRect.y -= 5;
            lastRect.height += 10;
            GUI.Box(lastRect, "", EditorStyles.helpBox);
        }

        void RenderPercentagePart(float layerStartTime, float layerEndTime, ref float percentage)
        {
            GUILayout.Label(layerStartTime + "Hr", GUILayout.Width(35), GUILayout.ExpandWidth(false));

            GUILayout.BeginVertical(percentagePartStyle, GUILayout.ExpandWidth(false), GUILayout.Width(150));
            float time = GetDayTimeForLayerNormalizedTime(layerStartTime, layerEndTime, percentage / 100);
            GUILayout.Label(time.ToString("f2") + " Hr", GUILayout.ExpandWidth(false));
            percentage = EditorGUILayout.Slider(percentage, 0, 100, GUILayout.Width(150), GUILayout.ExpandWidth(false));
            GUILayout.EndVertical();

            GUILayout.Label(layerEndTime + "Hr", GUILayout.Width(35), GUILayout.ExpandWidth(false));
        }

        #endregion

        private void ApplyOnMaterial()
        {
            EnsureDependencies();
            selectedConfiguration.ApplyOnMaterial(selectedMat, timeOfTheDay, GetNormalizedDayTime(), matLayer.numberOfSlots, directionalLight);

            // If in play mode, call avatar color from skybox controller class
            if (Application.isPlaying && SkyboxController.i != null)
            {
                SkyboxController.i.ApplyAvatarColor(GetNormalizedDayTime());
            }
        }

        private float GetNormalizedDayTime()
        {
            float tTime = 0;
            tTime = timeOfTheDay / 24;
            tTime = Mathf.Clamp(tTime, 0, 1);
            return tTime;
        }

        private float GetNormalizedLayerCurrentTime(float startTime, float endTime)
        {
            float editedEndTime = endTime;
            float editedDayTime = timeOfTheDay;
            if (endTime < startTime)
            {
                editedEndTime = 24 + endTime;
                if (timeOfTheDay < startTime)
                {
                    editedDayTime = 24 + timeOfTheDay;
                }
            }
            return Mathf.InverseLerp(startTime, editedEndTime, editedDayTime);
        }

        private float GetDayTimeForLayerNormalizedTime(float startTime, float endTime, float normalizeTime)
        {
            float editedEndTime = endTime;
            if (endTime < startTime)
            {
                editedEndTime = 24 + endTime;
            }
            float time = Mathf.Lerp(startTime, editedEndTime, normalizeTime);

            if (time > 24)
            {
                time -= 24;
            }

            return time;
        }

        private void ClampToDayTime(ref float value) { value = Mathf.Clamp(value, 0, 24); }
    }

}