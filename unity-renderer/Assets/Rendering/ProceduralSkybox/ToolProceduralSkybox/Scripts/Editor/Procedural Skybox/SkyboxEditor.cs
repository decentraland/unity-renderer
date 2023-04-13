using DCL.Providers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace DCL.Skybox
{
    public class SkyboxEditorWindow : EditorWindow
    {
        private EditorToolMeasurements toolSize;

        private List<SkyboxConfiguration> configurations;
        private List<string> configurationNames;
        private int selectedConfigurationIndex;
        private SkyboxConfiguration selectedConfiguration;

        public bool isPaused;
        public float timeOfTheDay;
        public float lifecycleDuration = 1;

        private Material selectedMat;
        private Vector2 leftPanelScrollPos;
        private Vector2 rightPanelScrollPos;
        private Vector2 topScroll;
        private Light directionalLight;
        private bool creatingNewConfig;
        private string newConfigName;
        private bool overridingController;
        private List<string> renderingOrderList;

        private float leftPanelWidth;
        private List<RightPanelPins> rightPanelPins = new List<RightPanelPins>() { new RightPanelPins { part = SkyboxEditorToolsParts.BG_Layer, name = "Background Layer" } };

        private CopyFunctionality copyPasteObj;
        private static SkyboxElements skyboxElements;

        private static SkyboxConfiguration[] tConfigurations;
        private static MaterialReferenceContainer materialReferenceContainer;
        [MenuItem("Window/Skybox Editor")]
        private static async void Init()
        {
            AddressableResourceProvider addressableResourceProvider = new AddressableResourceProvider();
            materialReferenceContainer = await addressableResourceProvider.GetAddressable<MaterialReferenceContainer>("SkyboxMaterialData.asset");
            IList<SkyboxConfiguration> configurations = await addressableResourceProvider.GetAddressablesList<SkyboxConfiguration>("SkyboxConfiguration");
            tConfigurations = configurations.ToArray();
            skyboxElements = new SkyboxElements();
            await skyboxElements.Initialize(addressableResourceProvider, materialReferenceContainer);

            SkyboxEditorWindow window = (SkyboxEditorWindow)EditorWindow.GetWindow(typeof(SkyboxEditorWindow));
            window.minSize = new Vector2(500, 500);
            window.Initialize();
            window.Show();
            window.InitializeWindow();
        }

        private void OnDisable() { skyboxElements?.Dispose(); }

        void Initialize() { toolSize = AssetDatabase.LoadAssetAtPath<EditorToolMeasurements>(SkyboxEditorLiterals.Paths.toolMeasurementPath); }

        public void InitializeWindow() { EnsureDependencies(); }

        private void Update()
        {
            if (selectedConfiguration == null || isPaused)
            {
                return;
            }

            float timeNormalizationFactor = lifecycleDuration * 60 / SkyboxUtils.CYCLE_TIME;
            timeOfTheDay += Time.deltaTime / timeNormalizationFactor;
            timeOfTheDay = Mathf.Clamp(timeOfTheDay, 0.01f, SkyboxUtils.CYCLE_TIME);

            ApplyOnMaterial(false);

            if (timeOfTheDay >= SkyboxUtils.CYCLE_TIME)
            {
                timeOfTheDay = 0.01f;
                selectedConfiguration.CycleResets();
            }

            Repaint();
        }

        private void OnGUI()
        {
            EditorGUI.DrawRect(new Rect(toolSize.topPanelLeftStart, toolSize.topPanelTopStart, position.width - toolSize.toolRightPadding, toolSize.topPanelHeight), toolSize.panelBGColor);
            GUILayout.BeginArea(new Rect(toolSize.topPanelLeftStart, toolSize.topPanelTopStart, position.width - toolSize.toolRightPadding, toolSize.topPanelHeight));
            RenderTopPanel();
            GUILayout.EndArea();

            // Left Panel
            float topLeft = toolSize.topPanelLeftStart;
            float top = toolSize.topPanelTopStart + toolSize.topPanelHeight + toolSize.panelsPadding;
            float width = (position.width - toolSize.toolRightPadding) * toolSize.leftPanelWidthPercentage;
            float height = (position.height - top - toolSize.panelsPadding);
            leftPanelWidth = width - toolSize.leftPanelPadding.xMax;
            EditorGUI.DrawRect(new Rect(topLeft, top, width, height), toolSize.panelBGColor);
            GUILayout.BeginArea(new Rect(topLeft + toolSize.leftPanelPadding.xMin, top + toolSize.leftPanelPadding.yMin, width - toolSize.leftPanelPadding.xMax, height - toolSize.leftPanelPadding.yMax));
            RenderLeftPanel();
            GUILayout.EndArea();

            // Right Panel
            topLeft = toolSize.topPanelLeftStart + width + toolSize.panelsPadding;
            width = position.width - toolSize.toolRightPadding - topLeft;
            GUILayout.BeginArea(new Rect(topLeft + toolSize.rightPanelPadding.xMin, top + toolSize.rightPanelPadding.yMin, width - toolSize.rightPanelPadding.xMax, height - toolSize.rightPanelPadding.yMax));
            RenderRightPanel(topLeft + toolSize.rightPanelPadding.xMin, top + toolSize.rightPanelPadding.yMin, width - toolSize.rightPanelPadding.xMax, height - toolSize.rightPanelPadding.yMax);
            GUILayout.EndArea();

            if (GUI.changed)
            {
                ApplyOnMaterial(true);
            }
        }

        #region Top Panel

        void RenderTopPanel()
        {
            topScroll = EditorGUILayout.BeginScrollView(topScroll);
            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.MiddleLeft;

            style.fixedWidth = position.width - toolSize.toolRightPadding;
            EditorGUILayout.BeginHorizontal(style);

            style.fixedWidth = (position.width - toolSize.toolRightPadding) / 2;
            EditorGUILayout.BeginVertical(style);
            RenderProfileControl();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(style);
            RenderTimePanel();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            // Timeline Tags
            EditorGUILayout.BeginHorizontal();

            style = new GUIStyle();
            style.alignment = TextAnchor.MiddleLeft;
            style.fixedWidth = (position.width - toolSize.toolRightPadding) / 2;
            EditorGUILayout.BeginVertical(style);
            if (GUILayout.Button(SkyboxEditorLiterals.Labels.timeLineTags, GUILayout.Width(100)))
            {
                AddToRightPanel(new RightPanelPins { part = SkyboxEditorToolsParts.Timeline_Tags, name = SkyboxEditorLiterals.Labels.timeLineTags });
            }
            EditorGUILayout.EndVertical();

            style = new GUIStyle();
            style.alignment = TextAnchor.MiddleRight;
            style.fixedWidth = 100;
            EditorGUILayout.BeginVertical(style);
            if (GUILayout.Button(SkyboxEditorLiterals.Labels.config))
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<EditorToolMeasurements>(SkyboxEditorLiterals.Paths.toolMeasurementPath);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
            if (Application.isPlaying && SkyboxController.i == null)
                EditorGUILayout.HelpBox("There's no SkyboxController in the scene, try this in InitialScene!", MessageType.Warning);
        }

        private void RenderProfileControl()
        {
            if (creatingNewConfig)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(SkyboxEditorLiterals.Labels.name);
                newConfigName = EditorGUILayout.TextField(newConfigName, GUILayout.Width(200));

                if (GUILayout.Button(SkyboxEditorLiterals.Labels.create, GUILayout.Width(50)))
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

                if (GUILayout.Button(SkyboxEditorLiterals.Labels.cancel, GUILayout.Width(50)))
                {
                    creatingNewConfig = false;
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(SkyboxEditorLiterals.Labels.currentProfile);
                selectedConfiguration = (SkyboxConfiguration)EditorGUILayout.ObjectField(selectedConfiguration, typeof(SkyboxConfiguration), false);

                if (selectedConfiguration != configurations[selectedConfigurationIndex])
                {
                    configurations[selectedConfigurationIndex] = selectedConfiguration;

                    if (Application.isPlaying && SkyboxController.i != null && overridingController)
                    {
                        SkyboxController.i.UpdateConfigurationTimelineEvent(selectedConfiguration);
                    }
                }

                if (GUILayout.Button(SkyboxEditorLiterals.Characters.sign_add, GUILayout.Width(50)))
                {
                    creatingNewConfig = true;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void RenderTimePanel()
        {

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Time : " + timeOfTheDay.ToString("f2"));
            timeOfTheDay = EditorGUILayout.Slider(timeOfTheDay, 0.01f, SkyboxUtils.CYCLE_TIME);
            if (isPaused)
            {
                if (GUILayout.Button(SkyboxEditorLiterals.Labels.play))
                {
                    ResumeTime();
                }
            }
            else
            {
                if (GUILayout.Button(SkyboxEditorLiterals.Labels.pause))
                {
                    PauseTime();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(SkyboxEditorLiterals.Labels.cycle);
            lifecycleDuration = EditorGUILayout.FloatField(lifecycleDuration);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Left Panel

        private void RenderLeftPanel()
        {
            leftPanelScrollPos = EditorGUILayout.BeginScrollView(leftPanelScrollPos, true, false);
            EditorGUILayout.BeginVertical();
            // Render BG Layer Button
            if (GUILayout.Button(SkyboxEditorLiterals.Layers.backgroundLayer, EditorStyles.toolbarButton))
            {
                AddToRightPanel(new RightPanelPins { part = SkyboxEditorToolsParts.BG_Layer, name = SkyboxEditorLiterals.Layers.backgroundLayer });
            }

            EditorGUILayout.Space(toolSize.leftPanelButtonSpace);
            if (GUILayout.Button(SkyboxEditorLiterals.Layers.horizonPlane, EditorStyles.toolbarButton))
            {
                AddToRightPanel(new RightPanelPins { part = SkyboxEditorToolsParts.Horizon_Plane, name = SkyboxEditorLiterals.Layers.horizonPlane });
            }

            EditorGUILayout.Space(toolSize.leftPanelButtonSpace);
            if (GUILayout.Button(SkyboxEditorLiterals.Layers.ambientLayer, EditorStyles.toolbarButton))
            {
                AddToRightPanel(new RightPanelPins { part = SkyboxEditorToolsParts.Ambient_Layer, name = SkyboxEditorLiterals.Layers.ambientLayer });
            }

            EditorGUILayout.Space(toolSize.leftPanelButtonSpace);
            if (GUILayout.Button(SkyboxEditorLiterals.Layers.avatarLayer, EditorStyles.toolbarButton))
            {
                AddToRightPanel(new RightPanelPins { part = SkyboxEditorToolsParts.Avatar_Layer, name = SkyboxEditorLiterals.Layers.avatarLayer });
            }

            EditorGUILayout.Space(toolSize.leftPanelButtonSpace);
            if (GUILayout.Button(SkyboxEditorLiterals.Layers.fogLayer, EditorStyles.toolbarButton))
            {
                AddToRightPanel(new RightPanelPins { part = SkyboxEditorToolsParts.Fog_Layer, name = SkyboxEditorLiterals.Layers.fogLayer });
            }

            EditorGUILayout.Space(toolSize.leftPanelButtonSpace);
            if (GUILayout.Button(SkyboxEditorLiterals.Layers.directionalLightLayer, EditorStyles.toolbarButton))
            {
                AddToRightPanel(new RightPanelPins { part = SkyboxEditorToolsParts.Directional_Light_Layer, name = SkyboxEditorLiterals.Layers.directionalLightLayer });
            }

            EditorGUILayout.Space(toolSize.leftPanelButtonSpace);

            // Render Base 2D layers
            EditorGUILayout.LabelField(SkyboxEditorLiterals.Layers.twoDLayers, EditorStyles.label, GUILayout.Width(leftPanelWidth - 10), GUILayout.ExpandWidth(false));

            EditorGUILayout.Space(toolSize.leftPanelButtonSpace);

            RenderLeftPanelBaseSkyboxLayers.Render(ref timeOfTheDay, toolSize, selectedConfiguration, AddToRightPanel, renderingOrderList, copyPasteObj);

            // Render Domes List
            EditorGUILayout.LabelField(SkyboxEditorLiterals.Layers.RenderDomeLayers, EditorStyles.label, GUILayout.Width(leftPanelWidth - 10), GUILayout.ExpandWidth(false));

            EditorGUILayout.Space(toolSize.leftPanelButtonSpace);

            RenderLeftPanelDomeLayers.Render(ref timeOfTheDay, toolSize, selectedConfiguration, AddToRightPanel, copyPasteObj);

            EditorGUILayout.Space(toolSize.leftPanelButtonSpace);

            // Render Satellite list
            EditorGUILayout.LabelField(SkyboxEditorLiterals.Layers.RenderSatelliteLayers, EditorStyles.label, GUILayout.Width(leftPanelWidth - 10), GUILayout.ExpandWidth(false));

            EditorGUILayout.Space(toolSize.leftPanelButtonSpace);

            RenderLeftPanelSatelliteLayers.Render(ref timeOfTheDay, toolSize, selectedConfiguration, AddToRightPanel, copyPasteObj);

            // Render Satellite list
            EditorGUILayout.LabelField(SkyboxEditorLiterals.Layers.RenderPlanarLayers, EditorStyles.label, GUILayout.Width(leftPanelWidth - 10), GUILayout.ExpandWidth(false));

            EditorGUILayout.Space(toolSize.leftPanelButtonSpace);

            RenderLeftPanelPlanarLayers.Render(ref timeOfTheDay, toolSize, selectedConfiguration, AddToRightPanel, copyPasteObj);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        #endregion

        #region Right Panel

        private void AddToRightPanel(RightPanelPins obj)
        {
            List<RightPanelPins> pinnedPanels = new List<RightPanelPins>();
            // Remove
            for (int i = 0; i < rightPanelPins.Count; i++)
            {
                if (rightPanelPins[i].pinned)
                {
                    pinnedPanels.Add(rightPanelPins[i]);
                }
            }
            pinnedPanels.Add(obj);
            // Add
            rightPanelPins = pinnedPanels;
        }

        void RenderRightPanel(float topLeft, float top, float width, float height)
        {
            RefreshPinnedPanels();

            rightPanelScrollPos = EditorGUILayout.BeginScrollView(rightPanelScrollPos);

            topLeft = 0;
            top = 0;

            for (int i = 0; i < rightPanelPins.Count - 1; i++)
            {
                // Make a box for pinned panel
                EditorGUI.DrawRect(new Rect(0 + toolSize.pinnedPanelBGOffset.x, top + toolSize.pinnedPanelBGOffset.y, width + toolSize.pinnedPanelBGOffset.width, toolSize.pinnedPanelHeight + toolSize.pinnedPanelBGOffset.height), toolSize.panelBGColor);
                GUILayout.BeginArea(new Rect(0, top, width - toolSize.rightPanelPadding.xMax, toolSize.pinnedPanelHeight));
                RenderPinnedRightPanel(rightPanelPins[i]);
                GUILayout.EndArea();

                topLeft = 0;
                top = top + toolSize.pinnedPanelHeight + toolSize.pinnedPanelBGOffset.yMax + toolSize.pinnedPanelBGOffset.yMax;
            }

            // Make a box for pinned panel
            EditorGUI.DrawRect(new Rect(0 + toolSize.pinnedPanelBGOffset.x, top + toolSize.pinnedPanelBGOffset.y, width + toolSize.pinnedPanelBGOffset.width, height), toolSize.panelBGColor);
            GUILayout.BeginArea(new Rect(0, top, width - toolSize.rightPanelPadding.xMax, height - top));

            if (rightPanelPins.Count > 0)
            {
                RenderPinnedRightPanel(rightPanelPins[rightPanelPins.Count - 1]);
            }

            GUILayout.EndArea();

            EditorGUILayout.EndScrollView();
        }

        private void RefreshPinnedPanels()
        {
            List<RightPanelPins> pinnedPanels = new List<RightPanelPins>();

            for (int i = 0; i < rightPanelPins.Count; i++)
            {
                // Check if pinned panel object (in case of Base_Skybox and Elements3D_Dome) is deleted or not if deleted continue
                if (rightPanelPins[i].part == SkyboxEditorToolsParts.Base_Skybox)
                {
                    if (!selectedConfiguration.layers.Contains(rightPanelPins[i].baseSkyboxTargetLayer))
                    {
                        continue;
                    }
                }

                if (rightPanelPins[i].part == SkyboxEditorToolsParts.Elements3D_Dome)
                {
                    if (!selectedConfiguration.additional3Dconfig.Contains(rightPanelPins[i].targetDomeElement))
                    {
                        continue;
                    }
                }

                if (rightPanelPins[i].pinned)
                {
                    pinnedPanels.Add(rightPanelPins[i]);
                }
                else if (i == (rightPanelPins.Count - 1))
                {
                    pinnedPanels.Add(rightPanelPins[i]);
                }
            }
            rightPanelPins = pinnedPanels;
        }

        private void RenderPinnedRightPanel(RightPanelPins obj)
        {
            RenderRightPanelHeading(obj.name, obj);
            obj.scroll = EditorGUILayout.BeginScrollView(obj.scroll);

            EditorGUILayout.Space(5);
            switch (obj.part)
            {
                case SkyboxEditorToolsParts.Timeline_Tags:
                    RenderTimelineTags.RenderLayer(ref timeOfTheDay, toolSize, selectedConfiguration);
                    break;
                case SkyboxEditorToolsParts.BG_Layer:
                    RenderBackgroundColorLayer.RenderLayer(ref timeOfTheDay, toolSize, selectedConfiguration);
                    break;
                case SkyboxEditorToolsParts.Horizon_Plane:
                    RenderHorizonPlane.RenderLayer(ref timeOfTheDay, toolSize, selectedConfiguration);
                    break;
                case SkyboxEditorToolsParts.Ambient_Layer:
                    RenderAmbientLayer.RenderLayer(ref timeOfTheDay, toolSize, selectedConfiguration);
                    break;
                case SkyboxEditorToolsParts.Avatar_Layer:
                    RenderAvatarLayer.RenderLayer(ref timeOfTheDay, toolSize, selectedConfiguration);
                    break;
                case SkyboxEditorToolsParts.Fog_Layer:
                    RenderFogLayer.RenderLayer(ref timeOfTheDay, toolSize, selectedConfiguration);
                    break;
                case SkyboxEditorToolsParts.Directional_Light_Layer:
                    DirectionalLightLayer.RenderLayer(ref timeOfTheDay, toolSize, selectedConfiguration, directionalLight);
                    break;
                case SkyboxEditorToolsParts.Base_Skybox:
                    RenderTextureLayer.RenderLayer(ref timeOfTheDay, toolSize, obj.baseSkyboxTargetLayer);
                    break;
                case SkyboxEditorToolsParts.Elements3D_Dome:
                    RenderDome3DLayer.RenderLayer(ref timeOfTheDay, toolSize, obj.targetDomeElement);
                    break;
                case SkyboxEditorToolsParts.Elements3D_Satellite:
                    RenderSatellite3DLayer.RenderLayer(ref timeOfTheDay, toolSize, obj.targetSatelliteElement);
                    break;
                case SkyboxEditorToolsParts.Elements3D_Planar:
                    RenderPlanar3DLayer.RenderLayer(ref timeOfTheDay, toolSize, obj.targetPlanarElement);
                    break;
                default:
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        void RenderRightPanelHeading(string text, RightPanelPins obj)
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniBoldLabel);
            style.normal.background = toolSize.rightPanelHeadingState.backgroundTex;
            EditorGUILayout.BeginHorizontal(style);
            style = new GUIStyle(EditorStyles.boldLabel);
            style.normal.textColor = toolSize.rightPanelHeadingTextColor.textColor;
            EditorGUILayout.LabelField(text, style, GUILayout.Width(200));

            string btnTxt = (obj.pinned) ? SkyboxEditorLiterals.Labels.unpin : SkyboxEditorLiterals.Labels.pin;
            if (GUILayout.Button(btnTxt, GUILayout.Width(50)))
            {
                obj.pinned = !obj.pinned;
            }
            EditorGUILayout.EndHorizontal();
        }

        #endregion

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

            if (selectedMat == null)
            {
                UpdateMaterial();
            }

            if (copyPasteObj == null)
            {
                copyPasteObj = new CopyFunctionality();
            }

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
                GameObject temp = new GameObject(SkyboxEditorLiterals.Labels.sunObjectName);
                // Add the light component
                directionalLight = temp.AddComponent<Light>();
                directionalLight.type = LightType.Directional;
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
                skyboxElements = SkyboxController.i.GetSkyboxElements();
                UpdateConfigurationsList();
            }
        }

        void InitializeMaterial()
        {
            selectedMat = materialReferenceContainer.skyboxMat;
            selectedConfiguration.ResetMaterial(selectedMat, materialReferenceContainer.skyboxMatSlots);
            RenderSettings.skybox = selectedMat;
        }

        private void UpdateMaterial() { InitializeMaterial(); }

        private SkyboxConfiguration AddNewConfiguration(string name)
        {
            SkyboxConfiguration temp = null;
            temp = ScriptableObject.CreateInstance<SkyboxConfiguration>();
            temp.skyboxID = name;
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Rendering/ProceduralSkybox/SkyboxAddressables/Skybox Configurations/" + name + ".asset");
            AssetDatabase.CreateAsset(temp, path);
            AssetDatabase.SaveAssets();

            return temp;
        }

        private void UpdateConfigurationsList()
        {
            configurations = new List<SkyboxConfiguration>(tConfigurations);
            configurationNames = new List<string>();

            // If no configurations exist, make and select new one.
            if (configurations == null || configurations.Count < 1)
            {
                selectedConfiguration = AddNewConfiguration(SkyboxEditorLiterals.Labels.defaultskyboxName);

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

        void ResumeTime() { isPaused = false; }

        void PauseTime() { isPaused = true; }

        private void ApplyOnMaterial(bool isEditor)
        {
            EnsureDependencies();
            float normalizedDayTime = SkyboxUtils.GetNormalizedDayTime(timeOfTheDay);
            selectedConfiguration.ApplyOnMaterial(selectedMat, timeOfTheDay, normalizedDayTime, materialReferenceContainer.skyboxMatSlots, directionalLight);

            skyboxElements?.ApplyConfigTo3DElements(selectedConfiguration, timeOfTheDay, normalizedDayTime, directionalLight, SkyboxUtils.CYCLE_TIME, isEditor);

            // If in play mode, call avatar color from skybox controller class
            if (Application.isPlaying && SkyboxController.i != null)
            {
                SkyboxController.i.ApplyAvatarColor(normalizedDayTime);
            }
        }
    }
}
