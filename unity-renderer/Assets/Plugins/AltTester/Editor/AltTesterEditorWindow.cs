
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Altom.AltDriver;
using Altom.AltTester;
using Altom.AltTesterEditor.Logging;
using NLog.Layouts;
using NUnit.Framework.Internal;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;

namespace Altom.AltTesterEditor
{
    public class AltTesterEditorWindow : UnityEditor.EditorWindow
    {
        private static readonly NLog.Logger logger = EditorLogManager.Instance.GetCurrentClassLogger();

        public static bool NeedsRepainting = false;
        public static AltEditorConfiguration EditorConfiguration;
        public static AltTesterEditorWindow Window;
        public static int SelectedTest = -1;

        // TestResult after running a test
        public static bool IsTestRunResultAvailable = false;
        public static int ReportTestPassed;
        public static int ReportTestFailed;
        public static double TimeTestRan;

        public static List<AltDevice> Devices = new List<AltDevice>();

        UnityEngine.Object obj;
        private static UnityEngine.Texture2D passIcon;
        private static UnityEngine.Texture2D failIcon;
        private static UnityEngine.Texture2D downArrowIcon;
        private static UnityEngine.Texture2D upArrowIcon;
        private static UnityEngine.Texture2D xIcon;
        private static UnityEngine.Texture2D reloadIcon;

        private static UnityEngine.Color greenColor = new UnityEngine.Color(0.0f, 0.5f, 0.2f, 1f);
        private static UnityEngine.Color redColor = new UnityEngine.Color(0.7f, 0.15f, 0.15f, 1f);
        private static UnityEngine.Color grayColor = new UnityEngine.Color(0.5f, 0.5f, 0.5f, 1f);
        private static UnityEngine.Color selectedTestColor = new UnityEngine.Color(1f, 1f, 1f, 1f);
        private static UnityEngine.Color selectedTestColorDark = new UnityEngine.Color(0.6f, 0.6f, 0.6f, 1f);
        private static UnityEngine.Color oddNumberTestColor = new UnityEngine.Color(0.75f, 0.75f, 0.75f, 1f);
        private static UnityEngine.Color evenNumberTestColor = new UnityEngine.Color(0.7f, 0.7f, 0.7f, 1f);
        private static UnityEngine.Color oddNumberTestColorDark = new UnityEngine.Color(0.23f, 0.23f, 0.23f, 1f);
        private static UnityEngine.Color evenNumberTestColorDark = new UnityEngine.Color(0.25f, 0.25f, 0.25f, 1f);
        private static UnityEngine.Color borderColorDark = new UnityEngine.Color(0.18f, 0.18f, 0.18f, 1f);
        private static UnityEngine.Color borderColor = new UnityEngine.Color(0.6f, 0.6f, 0.6f, 1f);
        private static UnityEngine.Texture2D selectedTestTexture;
        private static UnityEngine.Texture2D oddNumberTestTexture;
        private static UnityEngine.Texture2D borderTexture;
        private static UnityEngine.Texture2D evenNumberTestTexture;
        private static UnityEngine.Texture2D verticalSplitTexture;
        private static UnityEngine.Texture2D horizontalSplitTexture;
        public static UnityEngine.Texture2D PortForwardingTexture;
        public static UnityEngine.Texture2D selectedTestsCountTexture;

        private static string downloadURl;
        private const string RELEASENOTESURL = "https://altom.com/alttester/docs/desktop/pages/release-notes.html";
        private const string PREFABNAME = "AltTesterPrefab";
        private static string version;
        private static UnityEngine.GUIStyle gUIStyleButton;
        private static UnityEngine.GUIStyle gUIStyleText;
        private static UnityEngine.GUIStyle gUIStyleHistoryChanges;

        private static UnityEngine.GUIStyle labelStyle;

        private static long timeSinceLastClick;
        private UnityEngine.Vector2 scrollPositonTestResult;
        private static UnityEngine.Font font;

        private bool foldOutScenes = true;
        private bool foldOutTestRunSettings = true;
        private bool foldOutBuildSettings = true;
        private bool foldOutIosSettings = true;
        private bool foldOutPortForwarding = true;
        UnityEngine.Rect popUpPosition;
        UnityEngine.Rect popUpContentPosition;
        UnityEngine.Rect closeButtonPosition;
        UnityEngine.Rect downloadButtonPosition;
        UnityEngine.Rect checkVersionChangesButtonPosition;
        float splitNormalizedPosition = 0.33f;
        float splitNormalizedPositionHorizontal = 0.33f;
        UnityEngine.Rect resizeHandleRect;
        UnityEngine.Rect resizeHandleRectHorizontal;

        private static bool insideMultilineComment = false;

        bool resize;
        bool resizeHorizontal;
        public UnityEngine.Vector2 scrollPositionVertical;
        public UnityEngine.Vector2 scrollPositionVerticalSecond;
        public UnityEngine.Vector2 scrollPositionHorizontal;
        public UnityEngine.Vector2 scrollPositionVerticalRightSide;

        public static bool loadTestCompleted = true;

        UnityEngine.Rect availableRect;
        UnityEngine.Rect availableRectHorizontal;

        private bool PlayInEditorPressed;
        private static UnityWebRequest www;

        #region UnityEditor MenuItems
        // Add menu item named "My Window" to the Window menu
        [UnityEditor.MenuItem("AltTester/AltTester Editor", false, 80)]
        public static void ShowWindow()
        {
            Window = (AltTesterEditorWindow)GetWindow(typeof(AltTesterEditorWindow));
            Window.minSize = new UnityEngine.Vector2(600, 100);
            Window.titleContent = new UnityEngine.GUIContent("AltTester Editor");
            Window.Show();
        }

        [UnityEditor.MenuItem("Assets/Create/AltTest", true, 80)]
        public static bool CanCreateAltTest()
        {
            return (getPathForSelectedItem() + "/").Contains("/Editor/");
        }

        [UnityEditor.MenuItem("Assets/Create/AltTest", false, 80)]
        public static void CreateAltTest()
        {
            var templatePath = UnityEditor.AssetDatabase.GUIDToAssetPath(UnityEditor.AssetDatabase.FindAssets("DefaultTestExample")[0]);
            string folderPath = getPathForSelectedItem();
            string newFilePath = System.IO.Path.Combine(folderPath, "NewAltTest.cs");
#if UNITY_2019_1_OR_NEWER
            UnityEditor.ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, newFilePath);
#else
            System.Reflection.MethodInfo method = typeof(UnityEditor.ProjectWindowUtil).GetMethod("CreateScriptAsset", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (method == null)
                throw new Altom.AltDriver.NotFoundException("Method to create Script file was not found");
            method.Invoke((object)null, new object[2]
            {
                (object) templatePath,
                (object) newFilePath
            });
#endif
        }

        [UnityEditor.MenuItem("AltTester/Create AltTester Package", false, 800)]
        public static void CreateAltTesterPackage()
        {
            UnityEngine.Debug.Log("AltTester - Unity Package creation started...");
            var version = AltRunner.VERSION.Replace('.', '_');
            string packageName = "AltTester_" + version + ".unitypackage";
            string assetPathName = "Assets/AltTester";

            // add all paths inside AltTester except examples
            var assetPathNames = Directory.GetDirectories(assetPathName).Where(dir => !dir.EndsWith("Examples")).ToList();
            assetPathNames.AddRange(Directory.GetFiles(assetPathName));

            UnityEditor.AssetDatabase.ExportPackage(assetPathNames.ToArray(), packageName, UnityEditor.ExportPackageOptions.Recurse);
            UnityEngine.Debug.Log("AltTester - Unity Package done.");
        }

        public static void CreateSampleScenesPackage()
        {
            UnityEngine.Debug.Log("SampleScenes - Unity Package creation started...");
            string packageName = "SampleScenes.unitypackage";
            string assetPathNames = "Assets/AltTester/Examples";
            UnityEditor.AssetDatabase.ExportPackage(assetPathNames, packageName, UnityEditor.ExportPackageOptions.Recurse);
            UnityEngine.Debug.Log("SampleScenes - Unity Package done.");
        }

        public static void CreatePackages()
        {
            CreateAltTesterPackage();
            CreateSampleScenesPackage();
        }

        private static void SendDesktopVersionRequest()
        {
            www = UnityEngine.Networking.UnityWebRequest.Get("https://altom.com/alttester-desktop-versions/?id=unityeditor&alttesterversion=" + AltRunner.VERSION);
            var wwwOp = www.SendWebRequest();
            UnityEditor.EditorApplication.update += CheckDesktopVersionRequest;

        }

        public static void CheckDesktopVersionRequest()
        {
            if (!www.isDone)
                return;

#if UNITY_2020_1_OR_NEWER
            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.ConnectionError)
#else
            if (www.isNetworkError)
#endif
            {
                UnityEngine.Debug.Log(www.error);
            }
            else
            {
                if (www.responseCode == 200)
                {
                    string textReceived = www.downloadHandler.text;
                    System.Text.RegularExpressions.Regex regex = null;
                    if (UnityEngine.SystemInfo.operatingSystemFamily == UnityEngine.OperatingSystemFamily.Windows)
                    {
                        regex = new System.Text.RegularExpressions.Regex(@"https://altom.com/app/uploads/AltTester/desktop/AltTesterDesktop.exe");

                    }
                    else if (UnityEngine.SystemInfo.operatingSystemFamily == UnityEngine.OperatingSystemFamily.MacOSX)
                    {
                        regex = new System.Text.RegularExpressions.Regex(@"https://altom.com/app/uploads/AltTester/desktop/AltTesterDesktop.dmg");
                    }

                    System.Text.RegularExpressions.Match match = regex.Match(textReceived);
                    if (match.Success)
                    {

                        var splitedText = match.Value.Split('_');
                        var releasedVersion = splitedText[2].Substring(1);
                        if (String.IsNullOrEmpty(EditorConfiguration.LatestDesktopVersion) || !isCurrentVersionEqualOrNewer(releasedVersion, EditorConfiguration.LatestDesktopVersion))
                        {
                            EditorConfiguration.LatestDesktopVersion = releasedVersion;
                            downloadURl = match.Value;
                            version = releasedVersion;
                            EditorConfiguration.ShowDesktopPopUpInEditor = true;
                        }
                    }
                }

                UnityEditor.EditorApplication.update -= CheckDesktopVersionRequest;
            }
        }
        private static bool isCurrentVersionEqualOrNewer(string releasedVersion, string version)
        {
            var releasedVersionSplited = releasedVersion.Split('.');
            var currentVersionSplited = version.Split('.');
            if (Int16.Parse(currentVersionSplited[0]) != Int16.Parse(releasedVersionSplited[0]))//check major number
            {
                return Int16.Parse(currentVersionSplited[0]) >= Int16.Parse(releasedVersionSplited[0]);
            }
            if (Int16.Parse(currentVersionSplited[1]) != Int16.Parse(releasedVersionSplited[1]))//check minor number
            {
                return Int16.Parse(currentVersionSplited[1]) >= Int16.Parse(releasedVersionSplited[1]);
            }
            return Int16.Parse(currentVersionSplited[2]) >= Int16.Parse(releasedVersionSplited[2]);
        }

        private void Awake()
        {
            if (EditorConfiguration == null)
            {
                InitEditorConfiguration();
            }

            EditorConfiguration.MyTests = null;
            loadTestCompleted = false;
            this.StartCoroutine(AltTestRunner.SetUpListTestCoroutine());
            SendDesktopVersionRequest();
        }
        private void OnEnable()
        {
            Window = this;
        }

        [UnityEditor.MenuItem("AltTester/AltId/Add AltId to every object", false, 800)]
        public static void AddIdComponentToEveryObjectInTheProject()
        {
            var scenes = altGetAllScenes();
            foreach (var scene in scenes)
            {
                EditorSceneManager.OpenScene(scene);
                AddIdComponentToEveryObjectInActiveScene();
            }
        }

        [UnityEditor.MenuItem("AltTester/AltId/Add AltId to every object in active scene", false, 800)]
        public static void AddIdComponentToEveryObjectInActiveScene()
        {
            var rootObjects = new List<UnityEngine.GameObject>();
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.GetActiveScene();
            scene.GetRootGameObjects(rootObjects);

            for (int i = 0; i < rootObjects.Count; i++)
            {
                addComponentToObjectAndHisChildren(rootObjects[i]);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }


        [UnityEditor.MenuItem("AltTester/AltId/Remove AltId from every object", false, 800)]
        public static void RemoveIdComponentFromEveryObjectInTheProject()
        {
            var scenes = altGetAllScenes();
            foreach (var scene in scenes)
            {
                EditorSceneManager.OpenScene(scene);
                RemoveComponentFromEveryObjectInTheScene();
            }
        }

        [UnityEditor.MenuItem("AltTester/AltId/Remove AltId from every object in active scene", false, 800)]
        public static void RemoveComponentFromEveryObjectInTheScene()
        {
            var rootObjects = new List<UnityEngine.GameObject>();
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.GetActiveScene();
            scene.GetRootGameObjects(rootObjects);

            for (int i = 0; i < rootObjects.Count; i++)
            {
                removeComponentFromObjectAndHisChildren(rootObjects[i]);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        [UnityEditor.MenuItem("AltTester/Support/Documentation", false, 800)]
        public static void GoToDocumentation()
        {
            Application.OpenURL("https://altom.com/alttester/docs/sdk/");
        }

        [UnityEditor.MenuItem("AltTester/Support/Discord", false, 800)]
        public static void GoToDiscord()
        {
            Application.OpenURL("https://discord.com/channels/744769398023127102/748159679426985984");
        }

        #endregion

        #region Unity Events
        protected void OnFocus()
        {
            Repaint();
            if (EditorConfiguration == null)
            {
                InitEditorConfiguration();
            }

            if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources/AltTester"))
            {
                AltBuilder.CreateJsonFileForInputMappingOfAxis();
            }
            if (failIcon == null)
            {
                var findIcon = UnityEditor.AssetDatabase.FindAssets("16px-indicator-fail");
                failIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(UnityEditor.AssetDatabase.GUIDToAssetPath(findIcon[0]));
            }
            if (passIcon == null)
            {
                var findIcon = UnityEditor.AssetDatabase.FindAssets("16px-indicator-pass");
                passIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(UnityEditor.AssetDatabase.GUIDToAssetPath(findIcon[0]));
            }
            if (downArrowIcon == null)
            {
                var findIcon = UnityEditor.EditorGUIUtility.isProSkin ? UnityEditor.AssetDatabase.FindAssets("downArrowIcon") : UnityEditor.AssetDatabase.FindAssets("downArrowIconDark");
                downArrowIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(UnityEditor.AssetDatabase.GUIDToAssetPath(findIcon[0]));
            }
            if (upArrowIcon == null)
            {
                var findIcon = UnityEditor.EditorGUIUtility.isProSkin ? UnityEditor.AssetDatabase.FindAssets("upArrowIcon") : UnityEditor.AssetDatabase.FindAssets("upArrowIconDark");
                upArrowIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(UnityEditor.AssetDatabase.GUIDToAssetPath(findIcon[0]));
            }

            if (xIcon == null)
            {
                var findIcon = UnityEditor.EditorGUIUtility.isProSkin ? UnityEditor.AssetDatabase.FindAssets("xIcon") : UnityEditor.AssetDatabase.FindAssets("xIconDark");
                xIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(UnityEditor.AssetDatabase.GUIDToAssetPath(findIcon[0]));
            }
            if (reloadIcon == null)
            {
                var findIcon = UnityEditor.EditorGUIUtility.isProSkin ? UnityEditor.AssetDatabase.FindAssets("reloadIcon") : UnityEditor.AssetDatabase.FindAssets("reloadIconDark");
                reloadIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(UnityEditor.AssetDatabase.GUIDToAssetPath(findIcon[0]));
            }

            if (selectedTestTexture == null)
            {
                selectedTestTexture = MakeTexture(20, 20, UnityEditor.EditorGUIUtility.isProSkin ? selectedTestColorDark : selectedTestColor);
            }

            if (selectedTestsCountTexture == null)
            {
                selectedTestsCountTexture = MakeTexture(20, 20, grayColor);
            }

            getListOfSceneFromEditor();
        }

        protected void OnGUI()
        {

            if (NeedsRepainting)
            {
                NeedsRepainting = false;
                Repaint();
            }

            if (UnityEngine.Application.isPlaying && !EditorConfiguration.RanInEditor)
            {
                EditorConfiguration.RanInEditor = true;
            }

            if (!UnityEngine.Application.isPlaying && EditorConfiguration.RanInEditor)
            {
                afterExitPlayMode();

            }
            if (PlayInEditorPressed && !UnityEditor.EditorApplication.isCompiling && AltBuilder.CheckAltTesterIsDefineAsAScriptingSymbol(UnityEditor.BuildPipeline.GetBuildTargetGroup(UnityEditor.EditorUserBuildSettings.activeBuildTarget)))
            {
                PlayInEditorPressed = false;
                UnityEditor.EditorApplication.isPlaying = true;
            }
            DrawGUI();
        }

        public void BeginHorizontalSplitView()
        {
            UnityEngine.Rect tempRect;

            tempRect = UnityEditor.EditorGUILayout.BeginHorizontal(UnityEngine.GUILayout.ExpandWidth(false));
            if (tempRect.width > 0.0f)
            {
                availableRectHorizontal = tempRect;
            }
        }
        public void BeginVerticalSplitView()
        {
            UnityEngine.Rect tempRect;

            tempRect = UnityEditor.EditorGUILayout.BeginVertical(UnityEngine.GUILayout.ExpandHeight(false));
            if (tempRect.width > 0.0f)
            {
                availableRect = tempRect;
            }
        }
        private void ResizeHorizontalSplitView()
        {
            resizeHandleRectHorizontal = new UnityEngine.Rect(availableRectHorizontal.width * splitNormalizedPositionHorizontal * 2, availableRectHorizontal.y, 2f, availableRectHorizontal.height);
            if (horizontalSplitTexture == null)
            {
                horizontalSplitTexture = MakeTexture(20, 100, UnityEditor.EditorGUIUtility.isProSkin ? oddNumberTestColorDark : oddNumberTestColor);
            }
            UnityEngine.GUI.DrawTexture(resizeHandleRectHorizontal, horizontalSplitTexture);
            UnityEditor.EditorGUIUtility.AddCursorRect(resizeHandleRectHorizontal, UnityEditor.MouseCursor.ResizeHorizontal);
            if (UnityEngine.Event.current.type == UnityEngine.EventType.MouseDown && resizeHandleRectHorizontal.Contains(UnityEngine.Event.current.mousePosition))
            {
                resizeHorizontal = true;
                resize = false;
            }
            if (resizeHorizontal)
            {
                var width = availableRectHorizontal.width * 2;
                splitNormalizedPositionHorizontal = UnityEngine.Event.current.mousePosition.x / width;
            }
            if (UnityEngine.Event.current.type == UnityEngine.EventType.MouseUp)
                resizeHorizontal = false;
        }
        private void ResizeVerticalSplitView()
        {

            resizeHandleRect = new UnityEngine.Rect(availableRect.x, availableRect.height * splitNormalizedPosition + 25f, availableRect.width, 2f);
            if (verticalSplitTexture == null)
            {
                verticalSplitTexture = MakeTexture(20, 100, UnityEditor.EditorGUIUtility.isProSkin ? oddNumberTestColorDark : oddNumberTestColor);
            }
            UnityEngine.GUI.DrawTexture(resizeHandleRect, verticalSplitTexture);
            UnityEditor.EditorGUIUtility.AddCursorRect(resizeHandleRect, UnityEditor.MouseCursor.ResizeVertical);
            if (UnityEngine.Event.current.type == UnityEngine.EventType.MouseDown && resizeHandleRect.Contains(UnityEngine.Event.current.mousePosition))
            {
                resize = true;
            }
            if (resize)
            {
                splitNormalizedPosition = UnityEngine.Event.current.mousePosition.y / availableRect.height;
            }
            if (UnityEngine.Event.current.type == UnityEngine.EventType.MouseUp)
                resize = false;
        }

        protected void OnInspectorUpdate()
        {
            if (IsTestRunResultAvailable)
            {
                Repaint();
                IsTestRunResultAvailable = UnityEditor.EditorUtility.DisplayDialog("Test Report",
                      " Total tests:" + (ReportTestFailed + ReportTestPassed) + System.Environment.NewLine + " Tests passed:" +
                      ReportTestPassed + System.Environment.NewLine + " Tests failed:" + ReportTestFailed + System.Environment.NewLine +
                      " Duration:" + TimeTestRan + " seconds", "Ok");
                if (IsTestRunResultAvailable)
                {
                    IsTestRunResultAvailable = !IsTestRunResultAvailable;
                }
                ReportTestFailed = 0;
                ReportTestPassed = 0;
                TimeTestRan = 0;
            }
        }

        protected void DrawGUI()
        {
            var screenWidth = UnityEditor.EditorGUIUtility.currentViewWidth;

            if (EditorConfiguration.ShowDesktopPopUpInEditor)
            {
                popUpPosition = new UnityEngine.Rect(screenWidth / 2 - 300, 0, 600, 100);
                popUpContentPosition = new UnityEngine.Rect(screenWidth / 2 - 296, 4, 592, 92);
                closeButtonPosition = new UnityEngine.Rect(popUpPosition.xMax - 20, popUpPosition.yMin + 5, 15, 15);
                downloadButtonPosition = new UnityEngine.Rect(popUpPosition.xMax - 200, popUpPosition.yMin + 30, 180, 30);
                checkVersionChangesButtonPosition = new UnityEngine.Rect(popUpPosition.xMax - 200, popUpPosition.yMin + 60, 180, 30);
                if (UnityEngine.Event.current.type == UnityEngine.EventType.MouseDown)
                {
                    if (checkVersionChangesButtonPosition.Contains(UnityEngine.Event.current.mousePosition))
                    {
                        UnityEngine.Application.OpenURL(RELEASENOTESURL);
                        UnityEngine.GUIUtility.ExitGUI();
                    }
                    if (downloadButtonPosition.Contains(UnityEngine.Event.current.mousePosition))
                    {
                        UnityEngine.Application.OpenURL(downloadURl);
                        UnityEngine.GUIUtility.ExitGUI();
                    }
                    if (closeButtonPosition.Contains(UnityEngine.Event.current.mousePosition))
                    {
                        EditorConfiguration.ShowDesktopPopUpInEditor = false;
                        UnityEngine.GUIUtility.ExitGUI();
                    }
                }
            }

            //----------------------Left Panel------------

            BeginHorizontalSplitView();
            var leftSide = (screenWidth / 3) * 2;

            scrollPositionHorizontal = UnityEngine.GUILayout.BeginScrollView(scrollPositionHorizontal, UnityEngine.GUILayout.Width(availableRectHorizontal.width * splitNormalizedPositionHorizontal * 2));

            BeginVerticalSplitView();

            displayTestGui(EditorConfiguration.MyTests);
            ResizeVerticalSplitView();
            scrollPositionVerticalSecond = UnityEngine.GUILayout.BeginScrollView(scrollPositionVerticalSecond, UnityEngine.GUILayout.ExpandHeight(true));
            UnityEditor.EditorGUILayout.Separator();

            displayBuildSettings();
            UnityEditor.EditorGUILayout.Separator();
            displayTestRunSettings();
            UnityEditor.EditorGUILayout.Separator();
            displayScenes();
            UnityEditor.EditorGUILayout.Separator();

            UnityEditor.EditorGUILayout.EndVertical();
            UnityEditor.EditorGUILayout.EndScrollView();
            UnityEditor.EditorGUILayout.EndScrollView();

            ResizeHorizontalSplitView();

            //-------------------Right Panel--------------

            scrollPositionVerticalRightSide = UnityEditor.EditorGUILayout.BeginScrollView(scrollPositionVerticalRightSide, UnityEngine.GUI.skin.textArea, UnityEngine.GUILayout.ExpandHeight(true));

            var rightSide = (screenWidth / 3);
            UnityEditor.EditorGUILayout.BeginVertical();

            DisplayPlatformAndPlatformSettings(rightSide);

            UnityEditor.EditorGUILayout.Separator();
            UnityEditor.EditorGUILayout.Separator();
            UnityEditor.EditorGUILayout.Separator();


            if (AltBuilder.Built)
            {
                var found = false;

                UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(AltBuilder.GetFirstSceneWhichWillBeBuilt());
                if (scene.path.Equals(AltBuilder.GetFirstSceneWhichWillBeBuilt()))
                {
                    if (scene.GetRootGameObjects()
                        .Any(gameObject => gameObject.name.Equals(PREFABNAME)))
                    {
                        UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
                        var altRunner = scene.GetRootGameObjects()
                            .First(a => a.name.Equals(PREFABNAME));
                        destroyAltRunner(altRunner);
                        found = true;
                    }

                    if (found == false)
                        AltBuilder.Built = false;
                }

            }

            UnityEditor.EditorGUILayout.LabelField("Build", UnityEditor.EditorStyles.boldLabel);
            if (EditorConfiguration.platform != AltPlatform.Editor)
            {
                if (UnityEngine.GUILayout.Button("Build Only"))
                {
                    if (EditorConfiguration.platform == AltPlatform.Android)
                    {
                        AltBuilder.BuildAndroidFromUI(autoRun: false);
                    }
#if UNITY_EDITOR_OSX
                    else if (EditorConfiguration.platform == AltPlatform.iOS)
                    {
                        AltBuilder.BuildiOSFromUI(autoRun: false);
                    }
#endif
                    else if (EditorConfiguration.platform == AltPlatform.Standalone)
                    {
                        AltBuilder.BuildStandaloneFromUI(EditorConfiguration.StandaloneTarget, autoRun: false);
                    }
                    else if (EditorConfiguration.platform == AltPlatform.WebGL)
                    {
                        AltBuilder.BuildWebGLFromUI(autoRun: false);
                    }
                    else
                    {
                        runInEditor();
                    }
                    UnityEngine.GUIUtility.ExitGUI();

                }
            }
            else
            {
                UnityEditor.EditorGUI.BeginDisabledGroup(true);
                UnityEngine.GUILayout.Button("Build Only");
                UnityEditor.EditorGUI.EndDisabledGroup();
            }

            UnityEditor.EditorGUILayout.Separator();
            UnityEditor.EditorGUILayout.Separator();
            UnityEditor.EditorGUILayout.Separator();

            UnityEditor.EditorGUILayout.LabelField("Run", UnityEditor.EditorStyles.boldLabel);
            if (EditorConfiguration.platform == AltPlatform.Editor && !UnityEditor.EditorApplication.isCompiling && !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (UnityEngine.GUILayout.Button("Play in Editor"))
                {
                    runInEditor();
                }
            }
            else
            {
                UnityEditor.EditorGUI.BeginDisabledGroup(true);
                UnityEngine.GUILayout.Button("Play in Editor");
                UnityEditor.EditorGUI.EndDisabledGroup();
            }

            if (EditorConfiguration.platform != AltPlatform.Editor)
            {
                if (UnityEngine.GUILayout.Button("Build & Run"))
                {
                    if (EditorConfiguration.platform == AltPlatform.Android)
                    {
                        AltBuilder.BuildAndroidFromUI(autoRun: true);
                    }
#if UNITY_EDITOR_OSX
                    else if (EditorConfiguration.platform == AltPlatform.iOS)
                    {
                        AltBuilder.BuildiOSFromUI(autoRun: true);
                    }
#endif
                    else if (EditorConfiguration.platform == AltPlatform.Standalone)
                    {
                        AltBuilder.BuildStandaloneFromUI(EditorConfiguration.StandaloneTarget, autoRun: true);
                    }
                    else if (EditorConfiguration.platform == AltPlatform.WebGL)
                    {
                        AltBuilder.BuildWebGLFromUI(autoRun: true);
                    }
                    UnityEngine.GUIUtility.ExitGUI();
                }

            }
            else
            {
                UnityEditor.EditorGUI.BeginDisabledGroup(true);
                UnityEngine.GUILayout.Button("Build & Run", UnityEngine.GUILayout.MinWidth(50));
                UnityEditor.EditorGUI.EndDisabledGroup();
            }
            UnityEditor.EditorGUILayout.Separator();
            UnityEditor.EditorGUILayout.Separator();
            UnityEditor.EditorGUILayout.Separator();
            var selectedTests = 0;
            var totalTests = 0;
            var failedTests = 0;
            if (AltTesterEditorWindow.EditorConfiguration.MyTests != null)
                foreach (var test in AltTesterEditorWindow.EditorConfiguration.MyTests)
                {
                    if (test.IsSuite)
                        continue;
                    totalTests++;
                    if (test.Selected)
                        selectedTests += 1;
                    if (test.Status != 1 && test.Status != 0)
                        failedTests++;
                }
            UnityEditor.EditorGUILayout.LabelField("Run tests", UnityEditor.EditorStyles.boldLabel);

            if (UnityEngine.GUILayout.Button("Run All Tests (" + totalTests.ToString() + ")"))
            {
                if (EditorConfiguration.platform == AltPlatform.Editor)
                {
                    var testThread = new Thread(() => AltTestRunner.RunTests(AltTestRunner.TestRunMode.RunAllTest));
                    testThread.Start();
                }
                else
                {

                    AltTestRunner.RunTests(AltTestRunner.TestRunMode.RunAllTest);
                }
            }
            if (UnityEngine.GUILayout.Button("Run Selected Tests (" + selectedTests.ToString() + ")"))
            {
                if (EditorConfiguration.platform == AltPlatform.Editor)
                {
                    var testThread = new Thread(() => AltTestRunner.RunTests(AltTestRunner.TestRunMode.RunSelectedTest));
                    testThread.Start();
                }
                else
                {

                    AltTestRunner.RunTests(AltTestRunner.TestRunMode.RunSelectedTest);
                }
            }
            if (UnityEngine.GUILayout.Button("Run Failed Tests (" + failedTests.ToString() + ")"))
            {
                if (EditorConfiguration.platform == AltPlatform.Editor)
                {
                    var testThread = new Thread(() => AltTestRunner.RunTests(AltTestRunner.TestRunMode.RunFailedTest));
                    testThread.Start();
                }
                else
                {

                    AltTestRunner.RunTests(AltTestRunner.TestRunMode.RunFailedTest);
                }
            }
            //Status test

            scrollPositonTestResult = UnityEditor.EditorGUILayout.BeginScrollView(scrollPositonTestResult, UnityEngine.GUI.skin.textArea, UnityEngine.GUILayout.ExpandHeight(true));
            if (SelectedTest != -1)
            {
                var gUIStyle = new UnityEngine.GUIStyle(UnityEngine.GUI.skin.label)
                {
                    wordWrap = true,
                    richText = true,
                    alignment = UnityEngine.TextAnchor.MiddleCenter
                };
                var gUIStyle2 = new UnityEngine.GUIStyle();
                UnityEditor.EditorGUILayout.LabelField("<b>" + EditorConfiguration.MyTests[SelectedTest].TestName + "</b>", gUIStyle);


                UnityEditor.EditorGUILayout.Separator();
                string textToDisplayForMessage;
                if (EditorConfiguration.MyTests[SelectedTest].Status == 0)
                {
                    textToDisplayForMessage = "No informartion about this test available.\nPlease rerun the test.";
                    UnityEditor.EditorGUILayout.LabelField(textToDisplayForMessage, gUIStyle, UnityEngine.GUILayout.MinWidth(30));
                }
                else
                {
                    gUIStyle = new UnityEngine.GUIStyle(UnityEngine.GUI.skin.label)
                    {
                        wordWrap = true,
                        richText = true
                    };

                    string status = "";
                    switch (EditorConfiguration.MyTests[SelectedTest].Status)
                    {
                        case 1:
                            status = "<color=green>Passed</color>";
                            break;
                        case -1:
                            status = "<color=red>Failed</color>";
                            break;

                    }

                    UnityEngine.GUILayout.BeginHorizontal();
                    UnityEditor.EditorGUILayout.LabelField("<b>Time</b>", gUIStyle, UnityEngine.GUILayout.MinWidth(30));
                    UnityEditor.EditorGUILayout.LabelField(EditorConfiguration.MyTests[SelectedTest].TestDuration.ToString(), gUIStyle, UnityEngine.GUILayout.MinWidth(100));
                    UnityEngine.GUILayout.EndHorizontal();

                    UnityEngine.GUILayout.BeginHorizontal();
                    UnityEditor.EditorGUILayout.LabelField("<b>Status</b>", gUIStyle, UnityEngine.GUILayout.MinWidth(30));
                    UnityEditor.EditorGUILayout.LabelField(status, gUIStyle, UnityEngine.GUILayout.MinWidth(100));
                    UnityEngine.GUILayout.EndHorizontal();
                    if (EditorConfiguration.MyTests[SelectedTest].Status == -1)
                    {
                        UnityEngine.GUILayout.BeginHorizontal();
                        UnityEditor.EditorGUILayout.LabelField("<b>Message</b>", gUIStyle, UnityEngine.GUILayout.MinWidth(30));
                        UnityEditor.EditorGUILayout.LabelField(EditorConfiguration.MyTests[SelectedTest].TestResultMessage, gUIStyle, UnityEngine.GUILayout.MinWidth(100));
                        UnityEngine.GUILayout.EndHorizontal();

                        UnityEngine.GUILayout.BeginHorizontal();
                        UnityEditor.EditorGUILayout.LabelField("<b>StackTrace</b>", gUIStyle, UnityEngine.GUILayout.MinWidth(30));
                        UnityEditor.EditorGUILayout.LabelField(EditorConfiguration.MyTests[SelectedTest].TestStackTrace, gUIStyle, UnityEngine.GUILayout.MinWidth(100));
                        UnityEngine.GUILayout.EndHorizontal();
                    }
                }
            }
            else
            {
                UnityEditor.EditorGUILayout.LabelField("No test selected");
            }
            UnityEditor.EditorGUILayout.EndScrollView();
            UnityEditor.EditorGUILayout.EndVertical();
            UnityEditor.EditorGUILayout.EndHorizontal();

            UnityEditor.EditorGUILayout.EndScrollView();
            if (EditorConfiguration.ShowDesktopPopUpInEditor)
            {
                showAltPopup();
            }
        }

        private void DisplayPlatformAndPlatformSettings(float size)
        {
            UnityEditor.EditorGUILayout.LabelField("Platform", UnityEditor.EditorStyles.boldLabel);
            var guiStyleRadioButton = new UnityEngine.GUIStyle(UnityEditor.EditorStyles.radioButton) { };
            guiStyleRadioButton.padding = new UnityEngine.RectOffset(20, 0, 1, 0);

            EditorGUI.BeginDisabledGroup(UnityEngine.Application.isPlaying || UnityEditor.EditorApplication.isCompiling);
            UnityEditor.EditorGUILayout.BeginHorizontal();
            EditorConfiguration.platform = size <= 300
                ? (AltPlatform)GUILayout.SelectionGrid((int)EditorConfiguration.platform, Enum.GetNames(typeof(AltPlatform)), 1, guiStyleRadioButton)
                : (AltPlatform)GUILayout.SelectionGrid((int)EditorConfiguration.platform, Enum.GetNames(typeof(AltPlatform)), Enum.GetNames(typeof(AltPlatform)).Length, guiStyleRadioButton);
            UnityEditor.EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            switch (EditorConfiguration.platform)
            {
                case AltPlatform.Android:
                    showSettings(UnityEditor.BuildTargetGroup.Android, AltPlatform.Android);

                    break;
                case AltPlatform.Standalone:
                    BuildTarget[] options = new BuildTarget[]
                   {
                BuildTarget.StandaloneWindows, BuildTarget.StandaloneWindows64,BuildTarget.StandaloneOSX,BuildTarget.StandaloneLinux64
                   };

                    int selected = Array.IndexOf(options, EditorConfiguration.StandaloneTarget);
                    selected = EditorGUILayout.Popup("Build Target", selected, options.ToList().ConvertAll(x => x.ToString()).ToArray());
                    EditorConfiguration.StandaloneTarget = options[selected];
                    browseBuildLocation();
                    break;
#if UNITY_EDITOR_OSX
                case AltPlatform.iOS:
                    showSettings(UnityEditor.BuildTargetGroup.iOS, AltPlatform.iOS);
                    break;
#endif
                case AltPlatform.WebGL:
                    showSettings(UnityEditor.BuildTargetGroup.WebGL, AltPlatform.WebGL);
                    break;
            }
        }

        private void showSettings(BuildTargetGroup targetGroup, AltPlatform platform)
        {
            browseBuildLocation();
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Settings", UnityEditor.EditorStyles.boldLabel);

            if (UnityEngine.GUILayout.Button(platform.ToString() + " player settings"))
            {
#if UNITY_2018_3_OR_NEWER
                UnityEditor.SettingsService.OpenProjectSettings("Project/Player");
#else
                UnityEditor.EditorApplication.ExecuteMenuItem("Edit/Project Settings/Player");
#endif
                UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup = targetGroup;
            }
        }

        private void showAltPopup()
        {
            if (font == null)
            {
                font = UnityEngine.Font.CreateDynamicFontFromOSFont("Arial", 16);
            }
            if (gUIStyleText == null)
            {
                gUIStyleText = new UnityEngine.GUIStyle(UnityEngine.GUI.skin.label)
                {
                    wordWrap = true,
                    richText = true,
                    alignment = UnityEngine.TextAnchor.MiddleLeft,
                    font = font
                };
            }
            if (gUIStyleButton == null)
            {
                gUIStyleButton = new UnityEngine.GUIStyle(UnityEngine.GUI.skin.button)
                {
                    wordWrap = true,
                    richText = true,
                    alignment = UnityEngine.TextAnchor.MiddleCenter,
                    font = font
                };
                gUIStyleButton.normal.background = AltTesterEditorWindow.PortForwardingTexture;
                gUIStyleButton.normal.textColor = UnityEngine.Color.white;
            }
            if (gUIStyleHistoryChanges == null)
            {
                gUIStyleHistoryChanges = new UnityEngine.GUIStyle(UnityEngine.GUI.skin.label)
                {
                    wordWrap = true,
                    richText = true,
                    alignment = UnityEngine.TextAnchor.MiddleCenter,
                    font = font
                };
            }
            if (borderTexture == null)
            {
                borderTexture = MakeTexture(20, 20, UnityEditor.EditorGUIUtility.isProSkin ? borderColorDark : borderColor);
            }
            UnityEngine.GUI.DrawTexture(popUpPosition, borderTexture);
            UnityEngine.GUI.DrawTexture(popUpContentPosition, oddNumberTestTexture);

            UnityEngine.GUI.Button(closeButtonPosition, "<size=10>X</size>", gUIStyleHistoryChanges);
            UnityEngine.GUI.Button(downloadButtonPosition, "<b><size=16>Download now!</size></b>", gUIStyleButton);
            UnityEngine.GUI.Button(checkVersionChangesButtonPosition, "<size=13>Check version history</size>", gUIStyleHistoryChanges);
            UnityEditor.EditorGUI.LabelField(checkVersionChangesButtonPosition, "<size=13>__________________</size>", gUIStyleHistoryChanges);

            UnityEngine.Rect textPosition = new UnityEngine.Rect(popUpPosition.xMin + 20, popUpPosition.yMin + 30, 370, 30);
            UnityEditor.EditorGUI.LabelField(textPosition, System.String.Format("<b><size=16>AltTester Desktop {0} has been released!</size></b>", version), gUIStyleText);
        }

        #endregion

        #region public methods
        public static void InitEditorConfiguration()
        {
            if (UnityEditor.AssetDatabase.FindAssets("AltTesterEditorSettings").Length == 0)
            {
                var altTesterEditorFolderPath = UnityEditor.AssetDatabase.GUIDToAssetPath(UnityEditor.AssetDatabase.FindAssets("AltTesterEditorWindow")[0]);
                altTesterEditorFolderPath = Path.GetDirectoryName(altTesterEditorFolderPath);
                EditorConfiguration = CreateInstance<AltEditorConfiguration>();
                EditorConfiguration.MyTests = null;
                UnityEditor.AssetDatabase.CreateAsset(EditorConfiguration, altTesterEditorFolderPath + "/AltTesterEditorSettings.asset");
                UnityEditor.AssetDatabase.SaveAssets();
            }
            else
            {
                EditorConfiguration = UnityEditor.AssetDatabase.LoadAssetAtPath<AltEditorConfiguration>(
                    UnityEditor.AssetDatabase.GUIDToAssetPath(UnityEditor.AssetDatabase.FindAssets("AltTesterEditorSettings")[0]));
            }
            UnityEditor.EditorUtility.SetDirty(EditorConfiguration);
        }

        public static UnityEngine.Texture2D MakeTexture(int width, int height, UnityEngine.Color col)
        {
            UnityEngine.Color[] pix = new UnityEngine.Color[width * height];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            var result = new UnityEngine.Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }


        public static UnityEngine.Texture2D SetColor(UnityEngine.Texture2D tex2, UnityEngine.Color32 color)
        {

            var fillColorArray = tex2.GetPixels32();

            for (var i = 0; i < fillColorArray.Length; ++i)
            {
                fillColorArray[i] = color;
            }

            tex2.SetPixels32(fillColorArray);

            tex2.Apply();
            return tex2;
        }


        public static void AddAllScenes()
        {
            var scenesToBeAddedGuid = UnityEditor.AssetDatabase.FindAssets("t:SceneAsset");
            EditorConfiguration.Scenes = new System.Collections.Generic.List<AltMyScenes>();
            foreach (var sceneGuid in scenesToBeAddedGuid)
            {
                var scenePath = UnityEditor.AssetDatabase.GUIDToAssetPath(sceneGuid);
                EditorConfiguration.Scenes.Add(new AltMyScenes(false, scenePath, 0));
            }
            UnityEditor.EditorBuildSettings.scenes = pathFromTheSceneInCurrentList();
        }

        public static void SelectAllScenes()
        {
            foreach (var scene in EditorConfiguration.Scenes)
            {
                scene.ToBeBuilt = true;
            }
            UnityEditor.EditorBuildSettings.scenes = pathFromTheSceneInCurrentList();
        }

        #endregion
        private static void swap(int index1, int index2)
        {
            AltMyScenes backUp = EditorConfiguration.Scenes[index1];
            EditorConfiguration.Scenes[index1] = EditorConfiguration.Scenes[index2];
            EditorConfiguration.Scenes[index2] = backUp;
        }
        private static void browseBuildLocation()
        {
            UnityEngine.GUILayout.BeginHorizontal();
            var guiStyleTextField = new UnityEngine.GUIStyle(UnityEngine.GUI.skin.textField)
            {
                fixedHeight = 15
            };
            guiStyleTextField.margin = new UnityEngine.RectOffset(3, 2, 4, 2);
            guiStyleTextField.padding = new UnityEngine.RectOffset(2, 0, 0, 0);
            EditorConfiguration.BuildLocationPath = UnityEditor.EditorGUILayout.TextField("Build Location", EditorConfiguration.BuildLocationPath, guiStyleTextField);
            UnityEngine.GUI.SetNextControlName("Browse");
            var guiStyleButon = new UnityEngine.GUIStyle(UnityEngine.GUI.skin.button)
            {
                fixedHeight = 15
            };
            guiStyleButon.margin = new UnityEngine.RectOffset(2, 2, 4, 2);
            var buildLocationPath = EditorConfiguration.BuildLocationPath;
            if (UnityEngine.GUILayout.Button("Browse", guiStyleButon))
            {
                EditorConfiguration.BuildLocationPath = UnityEditor.EditorUtility.OpenFolderPanel("Select Build Location", "", "");
                if (EditorConfiguration.BuildLocationPath.Length == 0)
                    EditorConfiguration.BuildLocationPath = buildLocationPath;
                UnityEngine.GUI.FocusControl("Browse");
            }
            UnityEngine.GUILayout.EndHorizontal();
        }

        private void getListOfSceneFromEditor()
        {
            var newScenes = new List<AltMyScenes>();
            foreach (var scene in UnityEditor.EditorBuildSettings.scenes)
            {
                newScenes.Add(new AltMyScenes(scene.enabled, scene.path, 0));
            }
            EditorConfiguration.Scenes = newScenes;
        }

        private void afterExitPlayMode()
        {
            removeAltTesterPrefab();
            AltBuilder.RemoveAltTesterFromScriptingDefineSymbols(UnityEditor.BuildPipeline.GetBuildTargetGroup(UnityEditor.EditorUserBuildSettings.activeBuildTarget));
            EditorConfiguration.RanInEditor = false;
        }

        private static void removeAltTesterPrefab()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            var altRunners = activeScene.GetRootGameObjects()
                .Where(gameObject => gameObject.name.Equals(PREFABNAME)).ToList();
            if (altRunners.Count != 0)
            {
                foreach (var altRunner in altRunners)
                {
                    DestroyImmediate(altRunner);

                }
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveOpenScenes();
            }
        }

        private void runInEditor()
        {
            AltBuilder.InsertAltTesterInTheActiveScene(AltTesterEditorWindow.EditorConfiguration.GetInstrumentationSettings());
            AltBuilder.CreateJsonFileForInputMappingOfAxis();
            AltBuilder.AddAltTesterInScriptingDefineSymbolsGroup(UnityEditor.BuildPipeline.GetBuildTargetGroup(UnityEditor.EditorUserBuildSettings.activeBuildTarget));
            PlayInEditorPressed = true;
        }
        private void displayTestRunSettings()
        {
            foldOutTestRunSettings = UnityEditor.EditorGUILayout.Foldout(foldOutTestRunSettings, "Test run Settings");
            if (foldOutTestRunSettings)
            {
                labelAndCheckboxHorizontalLayout("Create XML Report", ref EditorConfiguration.createXMLReport);
                if (EditorConfiguration.createXMLReport)
                    labelAndInputFieldHorizontalLayout("XML file path", ref EditorConfiguration.xMLFilePath);
            }
        }
        private void displayBuildSettings()
        {
            foldOutBuildSettings = UnityEditor.EditorGUILayout.Foldout(foldOutBuildSettings, "Build Settings");
            if (foldOutBuildSettings)
            {
                var companyName = UnityEditor.PlayerSettings.companyName;
                labelAndInputFieldHorizontalLayout("Company Name", ref companyName);
                UnityEditor.PlayerSettings.companyName = companyName;

                var productName = UnityEditor.PlayerSettings.productName;
                labelAndInputFieldHorizontalLayout("Product Name", ref productName);
                UnityEditor.PlayerSettings.productName = productName;

                labelAndCheckboxHorizontalLayout("Input visualizer", ref EditorConfiguration.InputVisualizer);
                labelAndCheckboxHorizontalLayout("Show popup", ref EditorConfiguration.ShowPopUp);
                labelAndCheckboxHorizontalLayout("Append \"Test\" to product name for AltTester builds:", ref EditorConfiguration.appendToName);
                var keepAUTSymbolChanged = labelAndCheckboxHorizontalLayout("Keep ALTTESTER symbol defined:", ref EditorConfiguration.KeepAUTSymbolDefined);
                if (keepAUTSymbolChanged && EditorConfiguration.KeepAUTSymbolDefined && !AltBuilder.CheckAltTesterIsDefineAsAScriptingSymbol(UnityEditor.BuildPipeline.GetBuildTargetGroup(UnityEditor.EditorUserBuildSettings.activeBuildTarget)))
                {
                    AltBuilder.AddAltTesterInScriptingDefineSymbolsGroup(UnityEditor.BuildPipeline.GetBuildTargetGroup(UnityEditor.EditorUserBuildSettings.activeBuildTarget));
                }
                if (keepAUTSymbolChanged && !EditorConfiguration.KeepAUTSymbolDefined && AltBuilder.CheckAltTesterIsDefineAsAScriptingSymbol(UnityEditor.BuildPipeline.GetBuildTargetGroup(UnityEditor.EditorUserBuildSettings.activeBuildTarget)))
                {
                    AltBuilder.RemoveAltTesterFromScriptingDefineSymbols(UnityEditor.BuildPipeline.GetBuildTargetGroup(UnityEditor.EditorUserBuildSettings.activeBuildTarget));
                }
                labelAndInputFieldHorizontalLayout("Proxy Host", ref EditorConfiguration.ProxyHost);

                labelAndInputFieldHorizontalLayout("Proxy Port", ref EditorConfiguration.ProxyPort);

                labelAndInputFieldHorizontalLayout("Game Name", ref EditorConfiguration.GameName);
            }

            switch (EditorConfiguration.platform)
            {
                case AltPlatform.Android:
                    UnityEditor.EditorGUILayout.Separator();

                    foldOutIosSettings = UnityEditor.EditorGUILayout.Foldout(foldOutIosSettings, "Android Settings");
                    if (foldOutIosSettings)
                    {
                        string androidBundleIdentifier = UnityEditor.PlayerSettings.GetApplicationIdentifier(UnityEditor.BuildTargetGroup.Android);
                        labelAndInputFieldHorizontalLayout("Android Bundle Identifier", ref androidBundleIdentifier);
                        if (androidBundleIdentifier != UnityEditor.PlayerSettings.GetApplicationIdentifier(UnityEditor.BuildTargetGroup.Android))
                        {
                            UnityEditor.PlayerSettings.SetApplicationIdentifier(UnityEditor.BuildTargetGroup.Android, androidBundleIdentifier);
                        }
                        labelAndInputFieldHorizontalLayout("Adb Path:", ref EditorConfiguration.AdbPath);
                    }
                    break;
                case AltPlatform.Editor:
                    break;
                case AltPlatform.Standalone:
                    break;
#if UNITY_EDITOR_OSX
                case AltPlatform.iOS:
                    foldOutIosSettings = UnityEditor.EditorGUILayout.Foldout(foldOutIosSettings, "iOS Settings");
                    if (foldOutIosSettings)
                    {
                        string iOSBundleIdentifier = UnityEditor.PlayerSettings.GetApplicationIdentifier(UnityEditor.BuildTargetGroup.iOS);
                        labelAndInputFieldHorizontalLayout("iOS Bundle Identifier", ref iOSBundleIdentifier);
                        if (iOSBundleIdentifier != UnityEditor.PlayerSettings.GetApplicationIdentifier(UnityEditor.BuildTargetGroup.iOS))
                        {
                            UnityEditor.PlayerSettings.SetApplicationIdentifier(UnityEditor.BuildTargetGroup.iOS, iOSBundleIdentifier);
                        }

                        var appleDeveloperTeamID = UnityEditor.PlayerSettings.iOS.appleDeveloperTeamID;
                        labelAndInputFieldHorizontalLayout("Signing Team Id: ", ref appleDeveloperTeamID);
                        UnityEditor.PlayerSettings.iOS.appleDeveloperTeamID = appleDeveloperTeamID;

                        var appleEnableAutomaticsSigning = UnityEditor.PlayerSettings.iOS.appleEnableAutomaticSigning;
                        labelAndCheckboxHorizontalLayout("Automatically Sign: ", ref appleEnableAutomaticsSigning);
                        UnityEditor.PlayerSettings.iOS.appleEnableAutomaticSigning = appleEnableAutomaticsSigning;

                        labelAndInputFieldHorizontalLayout("Iproxy Path: ", ref EditorConfiguration.IProxyPath);
                        labelAndInputFieldHorizontalLayout("Xcrun Path: ", ref EditorConfiguration.XcrunPath);
                    }
                    break;
#endif
            }
        }

        private void displayPortForwarding(float widthColumn)
        {
            foldOutPortForwarding = UnityEditor.EditorGUILayout.Foldout(foldOutPortForwarding, "Port Forwarding");
            var guiStyleBolded = setTextGuiStyle();
            guiStyleBolded.fontStyle = UnityEngine.FontStyle.Bold;

            var guiStyleNormal = setTextGuiStyle();

            UnityEditor.EditorGUILayout.BeginHorizontal();
            UnityEditor.EditorGUILayout.LabelField("", UnityEngine.GUILayout.MaxWidth(30));
            UnityEditor.EditorGUILayout.BeginVertical();
            widthColumn -= 30;
            if (foldOutPortForwarding)
            {
                UnityEngine.GUILayout.BeginVertical(UnityEngine.GUI.skin.textField, UnityEngine.GUILayout.MaxHeight(30));
                UnityEngine.GUILayout.BeginHorizontal();
                UnityEngine.GUILayout.Label("DeviceId", guiStyleBolded, UnityEngine.GUILayout.Width(widthColumn / 2), UnityEngine.GUILayout.ExpandWidth(true));
                UnityEngine.GUILayout.FlexibleSpace();
                UnityEngine.GUILayout.Label("Local Port", guiStyleBolded, UnityEngine.GUILayout.Width(widthColumn / 7));
                UnityEngine.GUILayout.Label("Remote Port", guiStyleBolded, UnityEngine.GUILayout.Width(widthColumn / 7));

                UnityEngine.GUILayout.BeginHorizontal();
                if (UnityEngine.GUILayout.Button(reloadIcon, UnityEngine.GUILayout.Width(widthColumn / 10)))
                {
                    refreshDeviceList();
                }
                UnityEngine.GUILayout.EndHorizontal();
                UnityEngine.GUILayout.EndHorizontal();

                if (Devices.Count != 0)
                {
                    foreach (var device in Devices)
                    {
                        if (device.Active)
                        {
                            var styleActive = new UnityEngine.GUIStyle();
                            styleActive.normal.background = PortForwardingTexture;

                            UnityEngine.GUILayout.BeginHorizontal(styleActive);
                            UnityEngine.GUILayout.Label(device.DeviceId, guiStyleNormal, UnityEngine.GUILayout.Width(widthColumn / 2), UnityEngine.GUILayout.ExpandWidth(true));
                            UnityEngine.GUILayout.Label(device.LocalPort.ToString(), guiStyleNormal, UnityEngine.GUILayout.Width(widthColumn / 7));
                            UnityEngine.GUILayout.Label(device.RemotePort.ToString(), guiStyleNormal, UnityEngine.GUILayout.Width(widthColumn / 7));
                            if (UnityEngine.GUILayout.Button("Stop", UnityEngine.GUILayout.Width(widthColumn / 10), UnityEngine.GUILayout.Height(15)))
                            {
                                if (device.Platform == "Android")
                                {
                                    AltPortForwarding.RemoveForwardAndroid(device.LocalPort, device.DeviceId, EditorConfiguration.AdbPath);
                                }
#if UNITY_EDITOR_OSX
                                else
                                {
                                    AltPortForwarding.KillIProxy(device.Pid);
                                }
#endif
                                device.Active = false;
                                refreshDeviceList();
                            }
                        }
                        else
                        {
                            UnityEngine.GUILayout.BeginHorizontal();
                            UnityEngine.GUILayout.Label(device.DeviceId, guiStyleNormal, UnityEngine.GUILayout.Width(widthColumn / 2), UnityEngine.GUILayout.ExpandWidth(true));
                            device.LocalPort = UnityEditor.EditorGUILayout.IntField(device.LocalPort, UnityEngine.GUILayout.Width(widthColumn / 7));
                            device.RemotePort = UnityEditor.EditorGUILayout.IntField(device.RemotePort, UnityEngine.GUILayout.Width(widthColumn / 7));
                            if (UnityEngine.GUILayout.Button("Start", UnityEngine.GUILayout.Width(widthColumn / 10), UnityEngine.GUILayout.MaxHeight(15)))
                            {
                                if (device.Platform == "Android")
                                {
                                    var response = AltPortForwarding.ForwardAndroid(device.LocalPort, device.RemotePort, device.DeviceId, EditorConfiguration.AdbPath);
                                    if (!response.Equals("Ok"))
                                    {
                                        logger.Error(response);
                                    }
                                }
#if UNITY_EDITOR_OSX
                                else
                                {
                                    try
                                    {

                                        device.Pid = AltPortForwarding.ForwardIos(device.LocalPort, device.RemotePort, device.DeviceId, EditorConfiguration.IProxyPath); ;
                                        device.Active = true;
                                    }
                                    catch (Exception ex)
                                    {
                                        logger.Error(ex);
                                    }

                                }
#endif
                                refreshDeviceList();
                            }
                        }

                        UnityEngine.GUILayout.EndHorizontal();
                    }
                }
                else
                {
                    UnityEditor.EditorGUILayout.LabelField("No devices connected. Click \"refresh\" button to search for devices", guiStyleNormal);
                }
                UnityEngine.GUILayout.EndVertical();
            }

            UnityEditor.EditorGUILayout.EndVertical();
            UnityEditor.EditorGUILayout.EndHorizontal();
        }

        private void refreshDeviceList()
        {
            List<AltDevice> adbDevices = AltPortForwarding.GetDevicesAndroid(EditorConfiguration.AdbPath);
            List<AltDevice> androidForwardedDevices = AltPortForwarding.GetForwardedDevicesAndroid(EditorConfiguration.AdbPath);
            foreach (var adbDevice in adbDevices)
            {
                var deviceForwarded = androidForwardedDevices.FirstOrDefault(device => device.DeviceId.Equals(adbDevice.DeviceId));
                if (deviceForwarded != null)
                {
                    adbDevice.LocalPort = deviceForwarded.LocalPort;
                    adbDevice.RemotePort = deviceForwarded.RemotePort;
                    adbDevice.Active = deviceForwarded.Active;
                }
            }
#if UNITY_EDITOR_OSX
            var iOSDEvices = AltPortForwarding.GetConnectediOSDevices(EditorConfiguration.XcrunPath);
            var iOSForwardedDevices = AltPortForwarding.GetForwardediOSDevices();
            foreach (var iOSDEvice in iOSDEvices)
            {
                var deviceForwarded = iOSForwardedDevices.FirstOrDefault(device => device.DeviceId.Equals(iOSDEvice.DeviceId));
                if (deviceForwarded != null)
                {
                    iOSDEvice.LocalPort = deviceForwarded.LocalPort;
                    iOSDEvice.RemotePort = deviceForwarded.RemotePort;
                    iOSDEvice.Active = deviceForwarded.Active;
                    iOSDEvice.Pid = deviceForwarded.Pid;
                }
            }
#endif

            Devices = adbDevices;
#if UNITY_EDITOR_OSX
            Devices.AddRange(iOSDEvices);
#endif
        }


        private static bool labelAndCheckboxHorizontalLayout(string labelText, ref bool editorConfigVariable)
        {
            bool initialValue = editorConfigVariable;
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(EditorStyles.label) { wordWrap = true };
            }
            UnityEditor.EditorGUILayout.BeginHorizontal();
            UnityEditor.EditorGUI.BeginDisabledGroup(UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode || UnityEditor.EditorApplication.isCompiling);
            UnityEditor.EditorGUILayout.LabelField("", UnityEngine.GUILayout.MaxWidth(30));
            UnityEditor.EditorGUILayout.LabelField(labelText, labelStyle, UnityEngine.GUILayout.Width(150));
            editorConfigVariable =
                UnityEditor.EditorGUILayout.Toggle(editorConfigVariable, UnityEngine.GUILayout.MaxWidth(30));
            UnityEngine.GUILayout.FlexibleSpace();
            UnityEditor.EditorGUI.EndDisabledGroup();
            UnityEditor.EditorGUILayout.EndHorizontal();
            return initialValue != editorConfigVariable;
        }

        private static void labelAndInputFieldHorizontalLayout(string labelText, ref string editorConfigVariable, bool isValid = true)
        {
            UnityEditor.EditorGUILayout.BeginHorizontal();
            UnityEditor.EditorGUI.BeginDisabledGroup(UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode || UnityEditor.EditorApplication.isCompiling);
            UnityEditor.EditorGUILayout.LabelField("", UnityEngine.GUILayout.MaxWidth(30));
            if (isValid)
                editorConfigVariable = UnityEditor.EditorGUILayout.TextField(labelText, editorConfigVariable.Trim());
            else
            {
                GUIStyle style = new GUIStyle(GUI.skin.textField);
                style.normal.textColor = Color.red;
                style.focused.textColor = Color.red;
                editorConfigVariable = UnityEditor.EditorGUILayout.TextField(labelText, editorConfigVariable.Trim(), style);
            }
            UnityEditor.EditorGUI.EndDisabledGroup();
            UnityEditor.EditorGUILayout.EndHorizontal();
        }

        private static void labelAndInputFieldHorizontalLayout(string labelText, ref int editorConfigVariable)
        {
            UnityEditor.EditorGUILayout.BeginHorizontal();
            UnityEditor.EditorGUI.BeginDisabledGroup(UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode || UnityEditor.EditorApplication.isCompiling);
            UnityEditor.EditorGUILayout.LabelField("", UnityEngine.GUILayout.MaxWidth(30));
            editorConfigVariable = UnityEditor.EditorGUILayout.IntField(labelText, editorConfigVariable);
            UnityEditor.EditorGUI.EndDisabledGroup();
            UnityEditor.EditorGUILayout.EndHorizontal();
        }

        private static void labelAndDropdownFieldHorizontalLayout(string labelText, string[] options, ref int selected)
        {
            UnityEditor.EditorGUILayout.BeginHorizontal();
            UnityEditor.EditorGUILayout.LabelField("", UnityEngine.GUILayout.MaxWidth(30));

            selected = UnityEditor.EditorGUILayout.Popup(labelText, selected, options);
            UnityEditor.EditorGUILayout.EndHorizontal();
        }

        private void displayScenes()
        {
            foldOutScenes = UnityEditor.EditorGUILayout.Foldout(foldOutScenes, "Scene Manager");
            UnityEditor.EditorGUILayout.BeginHorizontal();
            if (foldOutScenes)
                UnityEditor.EditorGUILayout.LabelField("", UnityEngine.GUILayout.MaxWidth(30));
            UnityEditor.EditorGUILayout.BeginVertical();
            UnityEngine.GUIStyle guiStyle = setTextGuiStyle();
            if (foldOutScenes)
            {
                if (EditorConfiguration.Scenes.Count != 0)
                {
                    UnityEditor.EditorGUILayout.BeginHorizontal();
                    UnityEditor.EditorGUILayout.LabelField("Display scene full path:", UnityEngine.GUILayout.Width(150), UnityEngine.GUILayout.ExpandWidth(false));
                    EditorConfiguration.ScenePathDisplayed = UnityEditor.EditorGUILayout.Toggle(EditorConfiguration.ScenePathDisplayed, UnityEngine.GUILayout.ExpandWidth(false), UnityEngine.GUILayout.Width(30));
                    UnityEngine.GUILayout.FlexibleSpace();


                    UnityEditor.EditorGUILayout.EndHorizontal();
                    UnityEngine.GUILayout.BeginVertical(UnityEngine.GUI.skin.textField);
                    AltMyScenes sceneToBeRemoved = null;
                    int counter = 0;
                    foreach (var scene in EditorConfiguration.Scenes)
                    {
                        UnityEngine.GUILayout.BeginHorizontal(UnityEngine.GUI.skin.textArea);

                        var valToggle = UnityEditor.EditorGUILayout.Toggle(scene.ToBeBuilt, UnityEngine.GUILayout.MaxWidth(15));
                        if (valToggle != scene.ToBeBuilt)
                        {
                            scene.ToBeBuilt = valToggle;
                            UnityEditor.EditorBuildSettings.scenes = pathFromTheSceneInCurrentList();
                        }
                        var sceneName = scene.Path;
                        if (!EditorConfiguration.ScenePathDisplayed)
                        {
                            var splittedPath = sceneName.Split('/');
                            sceneName = splittedPath[splittedPath.Length - 1];
                        }

                        UnityEditor.EditorGUILayout.LabelField(sceneName, guiStyle);
                        UnityEngine.GUILayout.FlexibleSpace();
                        string value;
                        if (scene.ToBeBuilt)
                        {
                            scene.BuildScene = counter;
                            counter++;
                            value = scene.BuildScene.ToString();
                        }
                        else
                        {
                            value = "";
                        }
                        var buttonWidth = 20;
                        UnityEditor.EditorGUILayout.LabelField(value, guiStyle, UnityEngine.GUILayout.MaxWidth(buttonWidth));


                        if (EditorConfiguration.Scenes.IndexOf(scene) != 0 && EditorConfiguration.Scenes.Count > 1)
                        {

                            if (UnityEngine.GUILayout.Button(upArrowIcon, UnityEngine.GUILayout.MaxWidth(buttonWidth)))
                            {
                                sceneMove(scene, true);
                                UnityEditor.EditorBuildSettings.scenes = pathFromTheSceneInCurrentList();
                            }
                        }
                        else
                        {
                            UnityEditor.EditorGUILayout.LabelField("", UnityEngine.GUILayout.MaxWidth(buttonWidth));
                        }

                        if (EditorConfiguration.Scenes.IndexOf(scene) != EditorConfiguration.Scenes.Count - 1 && EditorConfiguration.Scenes.Count > 1)
                        {
                            if (UnityEngine.GUILayout.Button(downArrowIcon, UnityEngine.GUILayout.MaxWidth(buttonWidth)))
                            {
                                sceneMove(scene, false);
                                UnityEditor.EditorBuildSettings.scenes = pathFromTheSceneInCurrentList();
                            }
                        }
                        else
                        {
                            UnityEditor.EditorGUILayout.LabelField("", UnityEngine.GUILayout.MaxWidth(buttonWidth));
                        }


                        if (UnityEngine.GUILayout.Button(xIcon, UnityEngine.GUILayout.MaxWidth(buttonWidth)))
                        {
                            sceneToBeRemoved = scene;
                        }

                        UnityEngine.GUILayout.EndHorizontal();

                    }


                    if (sceneToBeRemoved != null)
                    {
                        removeScene(sceneToBeRemoved);
                    }

                    UnityEngine.GUILayout.EndVertical();
                }

                UnityEngine.GUILayout.BeginVertical();
                UnityEditor.EditorGUILayout.BeginHorizontal();
                UnityEditor.EditorGUILayout.LabelField("Add scene: ", UnityEngine.GUILayout.MaxWidth(80));
                obj = UnityEditor.EditorGUILayout.ObjectField(obj, typeof(UnityEditor.SceneAsset), true);

                if (obj != null)
                {
                    var path = UnityEditor.AssetDatabase.GetAssetPath(obj);
                    if (EditorConfiguration.Scenes.All(n => n.Path != path))
                    {
                        EditorConfiguration.Scenes.Add(new AltMyScenes(false, path, -1));
                        UnityEditor.EditorBuildSettings.scenes = pathFromTheSceneInCurrentList();
                    }

                    obj = null;
                }
                UnityEditor.EditorGUILayout.EndHorizontal();

                if (UnityEditor.EditorGUIUtility.currentViewWidth / 3 * 2 > 700)//All scene button in one line
                {
                    UnityEditor.EditorGUILayout.BeginHorizontal();
                    if (UnityEngine.GUILayout.Button("Add all scenes", UnityEditor.EditorStyles.miniButtonLeft, UnityEngine.GUILayout.MinWidth(30)))
                    {
                        AddAllScenes();
                    }

                    if (UnityEngine.GUILayout.Button("Select all scenes", UnityEditor.EditorStyles.miniButtonMid, UnityEngine.GUILayout.MinWidth(30)))
                    {
                        SelectAllScenes();
                    }
                    if (UnityEngine.GUILayout.Button("Deselect all scenes", UnityEditor.EditorStyles.miniButtonMid, UnityEngine.GUILayout.MinWidth(30)))
                    {
                        deselectAllScenes();
                    }
                    if (UnityEngine.GUILayout.Button("Remove unselected scenes", UnityEditor.EditorStyles.miniButtonMid, UnityEngine.GUILayout.MinWidth(30)))
                    {
                        removeNotSelectedScenes();
                    }
                    if (UnityEngine.GUILayout.Button("Remove all scenes", UnityEditor.EditorStyles.miniButtonRight, UnityEngine.GUILayout.MinWidth(30)))
                    {
                        EditorConfiguration.Scenes = new System.Collections.Generic.List<AltMyScenes>();
                        UnityEditor.EditorBuildSettings.scenes = pathFromTheSceneInCurrentList();
                    }
                    UnityEditor.EditorGUILayout.EndHorizontal();
                }
                else
                {
                    UnityEditor.EditorGUILayout.BeginHorizontal();
                    if (UnityEngine.GUILayout.Button("Add all scenes", UnityEditor.EditorStyles.miniButtonLeft, UnityEngine.GUILayout.MinWidth(30)))
                    {
                        AddAllScenes();
                    }

                    if (UnityEngine.GUILayout.Button("Select all scenes", UnityEditor.EditorStyles.miniButtonRight, UnityEngine.GUILayout.MinWidth(30)))
                    {
                        SelectAllScenes();
                    }
                    UnityEditor.EditorGUILayout.EndHorizontal();
                    UnityEditor.EditorGUILayout.BeginHorizontal();

                    if (UnityEngine.GUILayout.Button("Deselect all scenes", UnityEditor.EditorStyles.miniButtonLeft, UnityEngine.GUILayout.MinWidth(30)))
                    {
                        deselectAllScenes();
                    }
                    if (UnityEngine.GUILayout.Button("Remove unselected scenes", UnityEditor.EditorStyles.miniButtonMid, UnityEngine.GUILayout.MinWidth(30)))
                    {
                        removeNotSelectedScenes();
                    }
                    if (UnityEngine.GUILayout.Button("Remove all scenes", UnityEditor.EditorStyles.miniButtonRight, UnityEngine.GUILayout.MinWidth(30)))
                    {
                        EditorConfiguration.Scenes = new System.Collections.Generic.List<AltMyScenes>();
                        UnityEditor.EditorBuildSettings.scenes = pathFromTheSceneInCurrentList();
                    }
                    UnityEditor.EditorGUILayout.EndHorizontal();
                }

                UnityEditor.EditorGUILayout.EndVertical();
            }

            UnityEditor.EditorGUILayout.EndVertical();
            UnityEditor.EditorGUILayout.EndHorizontal();
        }

        private static UnityEngine.GUIStyle setTextGuiStyle()
        {
            var guiStyle = new UnityEngine.GUIStyle
            {
                alignment = UnityEngine.TextAnchor.MiddleLeft,
                stretchHeight = true
            };
            guiStyle.normal.textColor = UnityEditor.EditorGUIUtility.isProSkin ? UnityEngine.Color.white : UnityEngine.Color.black;
            guiStyle.wordWrap = true;
            return guiStyle;
        }

        private void removeNotSelectedScenes()
        {
            var copyMyScenes = new List<AltMyScenes>();
            foreach (var scene in EditorConfiguration.Scenes)
            {
                if (scene.ToBeBuilt)
                {
                    copyMyScenes.Add(scene);
                }
            }

            EditorConfiguration.Scenes = copyMyScenes;
            UnityEditor.EditorBuildSettings.scenes = pathFromTheSceneInCurrentList();
        }

        private void deselectAllScenes()
        {
            foreach (var scene in EditorConfiguration.Scenes)
            {
                scene.ToBeBuilt = false;
            }
            UnityEditor.EditorBuildSettings.scenes = pathFromTheSceneInCurrentList();

        }

        private void displayTestGui(System.Collections.Generic.List<AltMyTest> tests)
        {
            UnityEditor.EditorGUILayout.BeginHorizontal();
            UnityEditor.EditorGUILayout.LabelField("Tests list", UnityEditor.EditorStyles.boldLabel);
            if (UnityEngine.GUILayout.Button("Refresh"))
            {
                this.StartCoroutine(AltTestRunner.SetUpListTestCoroutine());
                loadTestCompleted = false;
            }
            UnityEditor.EditorGUILayout.EndHorizontal();
            UnityEditor.EditorGUILayout.BeginVertical();
            scrollPositionVertical = UnityEngine.GUILayout.BeginScrollView(scrollPositionVertical, UnityEngine.GUILayout.Height(availableRect.height * splitNormalizedPosition));


            if (!loadTestCompleted)
            {
                UnityEditor.EditorGUILayout.LabelField("Loading", UnityEditor.EditorStyles.boldLabel);
            }
            else
            {


                int foldOutCounter = 0;
                int testCounter = 0;
                var parentNames = new List<string>();
                var selectedTests = new List<int>();
                foreach (var test in tests)
                {
                    if (test.TestCaseCount == 0)
                    {
                        continue;
                    }
                    if (foldOutCounter > 0)
                    {
                        if (test.Type.Equals("NUnit.Framework.Internal.TestMethod"))
                        {
                            foldOutCounter--;
                        }
                        continue;
                    }
                    testCounter++;
                    var idx = parentNames.IndexOf(test.ParentName) + 1;
                    parentNames.RemoveRange(idx, parentNames.Count - idx);

                    var horizontalStyle = new UnityEngine.GUIStyle();
                    if (tests.IndexOf(test) == SelectedTest)
                    {
                        horizontalStyle.normal.background = selectedTestTexture;
                    }
                    else
                    {
                        if (testCounter % 2 == 0)
                        {
                            if (evenNumberTestTexture == null)
                            {
                                evenNumberTestTexture = MakeTexture(20, 20, UnityEditor.EditorGUIUtility.isProSkin ? evenNumberTestColorDark : evenNumberTestColor);
                            }
                            horizontalStyle.normal.background = evenNumberTestTexture;
                        }
                        else
                        {

                            if (oddNumberTestTexture == null)
                            {
                                oddNumberTestTexture = MakeTexture(20, 20, UnityEditor.EditorGUIUtility.isProSkin ? oddNumberTestColorDark : oddNumberTestColor);
                            }
                            horizontalStyle.normal.background = oddNumberTestTexture;
                        }
                    }
                    UnityEditor.EditorGUILayout.BeginHorizontal(horizontalStyle);


                    if (test.Type.Equals("NUnit.Framework.Internal.TestMethod"))
                    {
                        test.FoldOut = true;
                    }
                    else
                    {
                        if (!test.FoldOut)
                        {
                            foldOutCounter = test.TestCaseCount;
                        }
                        parentNames.Add(test.TestName);
                        selectedTests.Add(test.TestSelectedCount);
                    }
                    EditorGUILayout.LabelField(" ", GUILayout.Width((!test.IsSuite ? 13 : 10) * parentNames.Count));


                    UnityEngine.GUILayout.BeginVertical();
                    GUILayout.Space(0);
                    var valueChanged = UnityEditor.EditorGUILayout.Toggle(test.Selected, UnityEngine.GUILayout.Width(15));
                    GUILayout.EndVertical();
                    if (valueChanged == test.Selected)
                    {
                        updateNumberOfSelectedTests(test);
                    }
                    else
                    {
                        test.Selected = valueChanged;
                        changeSelectionChildsAndParent(test);
                    }

                    var testName = test.TestName;

                    if (test.ParentName == "")
                    {
                        var splitedPath = testName.Split('/');
                        testName = splitedPath[splitedPath.Length - 1];
                    }
                    else
                    {
                        if (testName.Contains('('))
                        {
                            var splitedPath = testName.Split(new[] { '.' }, 2);
                            testName = splitedPath[1];
                        }
                        else
                        {
                            var splitedPath = testName.Split('.');
                            testName = splitedPath[splitedPath.Length - 1];
                        }
                    }

                    var guiStyle = new UnityEngine.GUIStyle { normal = { textColor = UnityEditor.EditorGUIUtility.isProSkin ? UnityEngine.Color.white : UnityEngine.Color.black } };
                    if (test.Status != 0)
                    {
                        UnityEngine.Color color = redColor;
                        UnityEngine.Texture2D icon = failIcon;
                        if (test.Status == 1)
                        {
                            color = greenColor;
                            icon = passIcon;
                        }
                        var gUIStyleLabel = new UnityEngine.GUIStyle
                        {
                            alignment = UnityEngine.TextAnchor.MiddleLeft
                        };
                        UnityEngine.GUILayout.Label(icon, gUIStyleLabel, UnityEngine.GUILayout.Width(20));
                        guiStyle = new UnityEngine.GUIStyle { normal = { textColor = color } };
                    }
                    selectTest(tests, test, testName, guiStyle);
                    UnityEngine.GUILayout.FlexibleSpace();
                    UnityEditor.EditorGUILayout.EndHorizontal();
                }
            }
            UnityEditor.EditorGUILayout.EndScrollView();
            UnityEditor.EditorGUILayout.EndVertical();

        }

        private static void selectTest(System.Collections.Generic.List<AltMyTest> tests, AltMyTest test, string testName, UnityEngine.GUIStyle guiStyle)
        {
            if (!test.IsSuite)
            {
                if (UnityEngine.GUILayout.Button(testName, guiStyle))
                {

                    SelectedTest = tests.IndexOf(test);
                    var parentTest = EditorConfiguration.MyTests.FirstOrDefault(a => a.TestName.Equals(test.ParentName));
                    if (SelectedTest == tests.IndexOf(test))
                    {
                        var actualTime = System.DateTime.Now.Ticks;
                        if (actualTime - timeSinceLastClick < 5000000)
                        {
                            if (test.Path == null)
                                throw new AltPathNotFoundException("The path to your test is invalid. Please make sure you have matching class and file names.");
#if UNITY_2019_1_OR_NEWER
                            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(test.Path, findLine(test.Path, testName), 0);
#else
                            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(test.Path, findLine(test.Path, testName));
#endif
                        }
                    }
                    else
                    {
                        SelectedTest = tests.IndexOf(test);
                        var parentTestTwo = EditorConfiguration.MyTests.FirstOrDefault(a => a.TestName.Equals(test.ParentName));
                    }
                    timeSinceLastClick = System.DateTime.Now.Ticks;
                }
            }
            else
            {
                var selectedTests = test.TestSelectedCount;
                if (selectedTests == 0)
                {
                    test.FoldOut = UnityEditor.EditorGUILayout.Foldout(test.FoldOut, testName);
                }
                else
                {
                    test.FoldOut = UnityEditor.EditorGUILayout.Foldout(test.FoldOut, testName + "(" + test.TestSelectedCount.ToString() + ")");
                }
            }
        }

        private static int findLine(String path, String nameOfTest)
        {
            String[] lines = File.ReadAllLines(path);
            int index = nameOfTest.IndexOf("(");
            if (index > -1)
                nameOfTest = nameOfTest.Substring(0, index);
            for (int i = 0; i < lines.Length; i++)
            {
                if (isComment(lines[i]))
                    continue;
                if (System.Text.RegularExpressions.Regex.Match(lines[i], @"(\s+)" + nameOfTest + @"(\s*\()").Success)
                    return i + 1;
            }
            return 1;
        }

        private static bool isComment(String line)
        {
            if (System.Text.RegularExpressions.Regex.Match(line, @"^(\s*//)").Success)
                return true;
            if (System.Text.RegularExpressions.Regex.Match(line, @"^(\s*/\*)").Success)
                insideMultilineComment = true;
            if (System.Text.RegularExpressions.Regex.Match(line, @"^(\s*\*/)").Success)
                insideMultilineComment = false;
            return insideMultilineComment;
        }

        private void changeSelectionChildsAndParent(AltMyTest test)
        {
            if (test.Type.ToString().Equals("NUnit.Framework.Internal.TestAssembly"))
            {
                var index = EditorConfiguration.MyTests.IndexOf(test);
                for (int i = index + 1; i < EditorConfiguration.MyTests.Count; i++)
                {
                    if (EditorConfiguration.MyTests[i].Type.ToString().Equals("NUnit.Framework.Internal.TestAssembly"))
                    {
                        break;
                    }
                    else
                    {
                        var totalTests = 0;
                        foreach (var selectedTest in AltTesterEditorWindow.EditorConfiguration.MyTests)
                        {
                            if (!selectedTest.IsSuite)
                                totalTests++;
                        }
                        if (test.Selected)
                        {
                            test.TestSelectedCount = totalTests;
                            foreach (var selectedTest in AltTesterEditorWindow.EditorConfiguration.MyTests)
                            {
                                if (selectedTest.IsSuite)
                                    selectedTest.TestSelectedCount = selectedTest.TestCaseCount;
                            }
                        }
                        else
                        {
                            test.TestSelectedCount = 0;
                            foreach (var selectedTest in AltTesterEditorWindow.EditorConfiguration.MyTests)
                            {
                                if (selectedTest.IsSuite)
                                    selectedTest.TestSelectedCount = 0;
                            }
                        }
                        if (EditorConfiguration.MyTests[i].Selected != test.Selected)
                        {
                            EditorConfiguration.MyTests[i].Selected = test.Selected;
                        }

                    }
                }
            }
            else
            {
                if (test.IsSuite)
                {
                    var index = EditorConfiguration.MyTests.IndexOf(test);
                    var testCount = test.TestCaseCount;
                    var testName = test.TestName;
                    var parentTest = EditorConfiguration.MyTests.FirstOrDefault(a => a.TestName.Equals(test.ParentName));
                    setSelectedTestsNumberForSuite(test, test.Selected);
                    for (int i = index + 1; i <= index + testCount; i++)
                    {
                        var selectedTest = EditorConfiguration.MyTests[i];
                        if (selectedTest.Selected != test.Selected)
                        {
                            selectedTest.Selected = test.Selected;
                            if (test.Selected)
                            {
                                test.TestSelectedCount = test.TestCaseCount;
                                if (selectedTest.IsSuite)
                                    selectedTest.TestSelectedCount = selectedTest.TestCaseCount;
                            }
                            else
                            {
                                test.TestSelectedCount = 0;
                                selectedTest.TestSelectedCount = 0;
                            }
                        }
                        if (selectedTest.IsSuite)
                        {
                            testCount++;
                        }
                    }
                }

                if (!test.IsSuite)
                {
                    setSelectedTestsNumberForParent(test, test.Selected);
                }
            }
        }
        private void updateNumberOfSelectedTests(AltMyTest test)
        {
            if (test != null && test.IsSuite)
            {
                test.TestSelectedCount = 0;
                var index = EditorConfiguration.MyTests.IndexOf(test);
                var testCount = test.TestCaseCount;
                for (int i = index + 1; i <= index + testCount; i++)
                {
                    var selectedTest = EditorConfiguration.MyTests[i];
                    if (selectedTest.Selected)
                    {
                        if (!selectedTest.IsSuite)
                            test.TestSelectedCount++;
                    }
                    if (selectedTest.IsSuite)
                        testCount++;
                }
            }

        }
        private void setSelectedTestsNumberForParent(AltMyTest test, bool isSelected)
        {
            while (test.ParentName != null)
            {
                test = EditorConfiguration.MyTests.FirstOrDefault(a => a.TestName.Equals(test.ParentName));
                if (test != null)
                {
                    if (isSelected)
                        test.TestSelectedCount++;
                    else
                    {
                        if (test.TestSelectedCount > 0)
                            test.TestSelectedCount--;
                        test.Selected = false;
                    }
                }
                else
                    return;
            }
        }
        private void setSelectedTestsNumberForSuite(AltMyTest test, bool isSelected)
        {
            var totalTests = test.TestCaseCount;

            while (test.ParentName != null)
            {
                test = EditorConfiguration.MyTests.FirstOrDefault(a => a.TestName.Equals(test.ParentName));
                if (test != null)
                {
                    if (isSelected)
                    {
                        test.TestSelectedCount += totalTests;
                    }
                    else
                    {
                        test.TestSelectedCount -= totalTests;
                        test.Selected = false;
                    }
                }
                else
                    return;
            }
        }
        private static void sceneMove(AltMyScenes scene, bool up)
        {
            int index = EditorConfiguration.Scenes.IndexOf(scene);
            if (up)
            {
                swap(index, index - 1);
            }
            else
            {
                swap(index, index + 1);
            }
        }

        private static UnityEditor.EditorBuildSettingsScene[] pathFromTheSceneInCurrentList()
        {
            var listOfPath = new List<UnityEditor.EditorBuildSettingsScene>();
            foreach (var scene in EditorConfiguration.Scenes)
            {
                listOfPath.Add(new UnityEditor.EditorBuildSettingsScene(scene.Path, scene.ToBeBuilt));
            }

            return listOfPath.ToArray();
        }

        private void removeScene(AltMyScenes scene)
        {
            EditorConfiguration.Scenes.Remove(scene);
            UnityEditor.EditorBuildSettings.scenes = pathFromTheSceneInCurrentList();
        }

        private static string getPathForSelectedItem()
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.Selection.activeObject);
            if (System.IO.Path.GetExtension(path) != "") //checks if current item is a folder or a file
            {
                path = path.Replace(System.IO.Path.GetFileName(UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.Selection.activeObject)), "");
            }
            return path;
        }

        private static void destroyAltRunner(UnityEngine.Object altRunner)
        {

            DestroyImmediate(altRunner);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            EditorSceneManager.OpenScene(AltBuilder.PreviousScenePath);
        }

        private static void addComponentToObjectAndHisChildren(UnityEngine.GameObject gameObject)
        {
            var component = gameObject.GetComponent<AltId>();
            if (component == null)
            {
                gameObject.AddComponent(typeof(AltId));

            }
            int childCount = gameObject.transform.childCount;
            for (int j = 0; j < childCount; j++)
            {
                addComponentToObjectAndHisChildren(gameObject.transform.GetChild(j).gameObject);
            }
        }

        private static void removeComponentFromObjectAndHisChildren(UnityEngine.GameObject gameObject)
        {
            var component = gameObject.GetComponent<AltId>();
            if (component != null)
            {
                DestroyImmediate(component);
            }
            int childCount = gameObject.transform.childCount;
            for (int j = 0; j < childCount; j++)
            {
                removeComponentFromObjectAndHisChildren(gameObject.transform.GetChild(j).gameObject);
            }
        }

        private static string[] altGetAllScenes()
        {
            string[] temp = UnityEditor.AssetDatabase.GetAllAssetPaths();
            var result = new List<string>();
            foreach (string s in temp)
            {
                if (s.EndsWith(".unity")) result.Add(s);
            }
            return result.ToArray();
        }
    }
}