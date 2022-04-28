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

        [MenuItem("Window/Skybox Editor")]
        static void Init()
        {
            SkyboxEditorWindow window = (SkyboxEditorWindow)EditorWindow.GetWindow(typeof(SkyboxEditorWindow));
            window.minSize = new Vector2(500, 500);
            window.Initialize();
            window.Show();
            window.InitializeWindow();
        }

        void Initialize() { toolSize = AssetDatabase.LoadAssetAtPath<EditorToolMeasurements>(SkyboxEditorLiterals.toolMeasurementPath); }

        public void InitializeWindow() { EnsureDependencies(); }

        private void Update()
        {
            if (selectedConfiguration == null || isPaused)
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
                ApplyOnMaterial();
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
            if (GUILayout.Button(SkyboxEditorLiterals.timeLineTags, GUILayout.Width(100)))
            {
                AddToRightPanel(new RightPanelPins { part = SkyboxEditorToolsParts.Timeline_Tags, name = SkyboxEditorLiterals.timeLineTags });
            }
            EditorGUILayout.EndVertical();

            style = new GUIStyle();
            style.alignment = TextAnchor.MiddleRight;
            style.fixedWidth = 100;
            EditorGUILayout.BeginVertical(style);
            if (GUILayout.Button(SkyboxEditorLiterals.config))
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<EditorToolMeasurements>(SkyboxEditorLiterals.toolMeasurementPath);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
        }

        private void RenderProfileControl()
        {
            if (creatingNewConfig)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(SkyboxEditorLiterals.name);
                newConfigName = EditorGUILayout.TextField(newConfigName, GUILayout.Width(200));

                if (GUILayout.Button(SkyboxEditorLiterals.create, GUILayout.Width(50)))
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

                if (GUILayout.Button(SkyboxEditorLiterals.cancel, GUILayout.Width(50)))
                {
                    creatingNewConfig = false;
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(SkyboxEditorLiterals.currentProfile);
                selectedConfiguration = (SkyboxConfiguration)EditorGUILayout.ObjectField(selectedConfiguration, typeof(SkyboxConfiguration), false);

                if (selectedConfiguration != configurations[selectedConfigurationIndex])
                {
                    UpdateConfigurationsList();

                    if (Application.isPlaying && SkyboxController.i != null && overridingController)
                    {
                        SkyboxController.i.UpdateConfigurationTimelineEvent(selectedConfiguration);
                    }
                }

                if (GUILayout.Button(SkyboxEditorLiterals.sign_add, GUILayout.Width(50)))
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
            timeOfTheDay = EditorGUILayout.Slider(timeOfTheDay, 0.01f, 24.00f);
            if (isPaused)
            {
                if (GUILayout.Button(SkyboxEditorLiterals.play))
                {
                    ResumeTime();
                }
            }
            else
            {
                if (GUILayout.Button(SkyboxEditorLiterals.pause))
                {
                    PauseTime();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(SkyboxEditorLiterals.cycle);
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
            if (GUILayout.Button(SkyboxEditorLiterals.backgroundLayer, EditorStyles.toolbarButton))
            {
                AddToRightPanel(new RightPanelPins { part = SkyboxEditorToolsParts.BG_Layer, name = SkyboxEditorLiterals.backgroundLayer });
            }

            EditorGUILayout.Space(toolSize.leftPanelButtonSpace);
            if (GUILayout.Button(SkyboxEditorLiterals.ambientLayer, EditorStyles.toolbarButton))
            {
                AddToRightPanel(new RightPanelPins { part = SkyboxEditorToolsParts.Ambient_Layer, name = SkyboxEditorLiterals.ambientLayer });
            }

            EditorGUILayout.Space(toolSize.leftPanelButtonSpace);
            if (GUILayout.Button(SkyboxEditorLiterals.avatarLayer, EditorStyles.toolbarButton))
            {
                AddToRightPanel(new RightPanelPins { part = SkyboxEditorToolsParts.Avatar_Layer, name = SkyboxEditorLiterals.avatarLayer });
            }

            EditorGUILayout.Space(toolSize.leftPanelButtonSpace);
            if (GUILayout.Button(SkyboxEditorLiterals.fogLayer, EditorStyles.toolbarButton))
            {
                AddToRightPanel(new RightPanelPins { part = SkyboxEditorToolsParts.Fog_Layer, name = SkyboxEditorLiterals.fogLayer });
            }

            EditorGUILayout.Space(toolSize.leftPanelButtonSpace);
            if (GUILayout.Button(SkyboxEditorLiterals.directionalLightLayer, EditorStyles.toolbarButton))
            {
                AddToRightPanel(new RightPanelPins { part = SkyboxEditorToolsParts.Directional_Light_Layer, name = SkyboxEditorLiterals.directionalLightLayer });
            }

            EditorGUILayout.Space(toolSize.leftPanelButtonSpace);

            // Render Base 2D layers
            EditorGUILayout.LabelField(SkyboxEditorLiterals.twoDLayers, EditorStyles.label, GUILayout.Width(leftPanelWidth - 10), GUILayout.ExpandWidth(false));

            EditorGUILayout.Space(toolSize.leftPanelButtonSpace);

            RenderLeftPanelBaseSkyboxLayers.Render(ref timeOfTheDay, toolSize, selectedConfiguration, AddToRightPanel, renderingOrderList, copyPasteObj);

            // Render 3D layers
            EditorGUILayout.LabelField(SkyboxEditorLiterals.threeDLayers, EditorStyles.label, GUILayout.Width(leftPanelWidth - 10), GUILayout.ExpandWidth(false));

            EditorGUILayout.Space(toolSize.leftPanelButtonSpace);

            RenderLeftPanel3DLayers.Render(ref timeOfTheDay, toolSize, selectedConfiguration, AddToRightPanel, copyPasteObj);

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
                    RenderTextureLayer.RenderLayer(ref timeOfTheDay, toolSize, obj.targetDomeElement.layers);
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

            string btnTxt = (obj.pinned) ? SkyboxEditorLiterals.unpin : SkyboxEditorLiterals.pin;
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
                GameObject temp = new GameObject(SkyboxEditorLiterals.sunObjectName);
                // Add the light component
                directionalLight = temp.AddComponent<Light>();
                directionalLight.type = LightType.Directional;
            }

            // Init 3D
            if (skyboxObjects == null)
            {
                skyboxObjects = new SkyboxElements();
                skyboxObjects.Initialize3DObjects(selectedConfiguration);
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
            selectedMat = MaterialReferenceContainer.i.skyboxMat;
            selectedConfiguration.ResetMaterial(selectedMat, MaterialReferenceContainer.i.skyboxMatSlots);
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

        private void UpdateConfigurationsList()
        {
            SkyboxConfiguration[] tConfigurations = Resources.LoadAll<SkyboxConfiguration>("Skybox Configurations/");
            configurations = new List<SkyboxConfiguration>(tConfigurations);
            configurationNames = new List<string>();

            // If no configurations exist, make and select new one.
            if (configurations == null || configurations.Count < 1)
            {
                selectedConfiguration = AddNewConfiguration(SkyboxEditorLiterals.defaultskyboxName);

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

        private SkyboxElements skyboxObjects;
        private void ApplyOnMaterial()
        {
            EnsureDependencies();
            selectedConfiguration.ApplyOnMaterial(selectedMat, timeOfTheDay, SkyboxEditorUtils.GetNormalizedDayTime(timeOfTheDay), MaterialReferenceContainer.i.skyboxMatSlots, directionalLight);
            selectedConfiguration.ApplyDomeConfigurations(skyboxObjects.GetOrderedGameobjectList(selectedConfiguration.additional3Dconfig), timeOfTheDay, SkyboxEditorUtils.GetNormalizedDayTime(timeOfTheDay), 1, directionalLight);

            // If in play mode, call avatar color from skybox controller class
            if (Application.isPlaying && SkyboxController.i != null)
            {
                SkyboxController.i.ApplyAvatarColor(SkyboxEditorUtils.GetNormalizedDayTime(timeOfTheDay));
            }
        }
    }
}