using System.Collections.Generic;
using Altom.AltDriver;
using Altom.AltTester;
using Altom.AltTesterEditor;
using Altom.AltTesterEditor.Logging;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Altom.AltTesterEditor
{
    public static class PlatformName
    {
        public const string Android = "Android";
        public const string iOS = "iOS";
        public const string Standalone = "Standalone";
    }

    public class AltBuilder
    {
        private const string ALTTESTERDEFINE = "ALTTESTER";
        private const string WEBGLDEFINE = "UNITY_WEBGL";
        private static readonly NLog.Logger logger = EditorLogManager.Instance.GetCurrentClassLogger();
        public enum InputType
        {
            KeyOrMouseButton,
            MouseMovement,
            JoystickAxis,
        };

        public static bool Built = false;
        public static string PreviousScenePath;
        public static UnityEngine.SceneManagement.Scene SceneWithAltTester;
        public static UnityEngine.Object AltTester;

        public static void InitBuildSetup(UnityEditor.BuildTargetGroup buildTargetGroup)
        {
            AltTesterEditorWindow.InitEditorConfiguration();

            if (AltTesterEditorWindow.EditorConfiguration.appendToName)
            {
                UnityEditor.PlayerSettings.productName = UnityEditor.PlayerSettings.productName + "Test";
                string bundleIdentifier = UnityEditor.PlayerSettings.GetApplicationIdentifier(buildTargetGroup) + "Test";
                UnityEditor.PlayerSettings.SetApplicationIdentifier(buildTargetGroup, bundleIdentifier);
            }
            AddAltTesterInScriptingDefineSymbolsGroup(buildTargetGroup);
            if (buildTargetGroup == UnityEditor.BuildTargetGroup.Standalone)
                CreateJsonFileForInputMappingOfAxis();

        }

        public static void BuildAndroidFromUI(bool autoRun = false)
        {
            try
            {
                InitBuildSetup(UnityEditor.BuildTargetGroup.Android);
                logger.Debug("Starting Android build..." + UnityEditor.PlayerSettings.productName + " : " + UnityEditor.PlayerSettings.bundleVersion);

                var buildPlayerOptions = new UnityEditor.BuildPlayerOptions
                {
                    locationPathName = getOutputPath(UnityEditor.BuildTarget.Android),
                    scenes = getScenesForBuild(),
                    target = UnityEditor.BuildTarget.Android
                };

                buildGame(autoRun, buildPlayerOptions);
            }
            catch (System.Exception e)
            {
                logger.Error(e);
            }
            finally
            {
                Built = true;
                resetBuildSetup(UnityEditor.BuildTargetGroup.Android);
            }
        }

        public static void BuildWebGLFromUI(bool autoRun = false)
        {
            try
            {
                InitBuildSetup(UnityEditor.BuildTargetGroup.WebGL);
                logger.Debug("Starting WebGL build..." + UnityEditor.PlayerSettings.productName + " : " + UnityEditor.PlayerSettings.bundleVersion);

                var buildPlayerOptions = new UnityEditor.BuildPlayerOptions
                {
                    locationPathName = getOutputPath(UnityEditor.BuildTarget.WebGL),
                    scenes = getScenesForBuild(),
                    target = UnityEditor.BuildTarget.WebGL,
                    targetGroup = UnityEditor.BuildTargetGroup.WebGL
                };
                AltBuilder.AddScriptingDefineSymbol(WEBGLDEFINE, UnityEditor.BuildTargetGroup.WebGL);

                buildGame(autoRun, buildPlayerOptions);
            }
            catch (System.Exception e)
            {
                logger.Error(e);
            }
            finally
            {
                Built = true;
                resetBuildSetup(UnityEditor.BuildTargetGroup.WebGL);
                AltBuilder.RemoveScriptingDefineSymbol(WEBGLDEFINE, UnityEditor.BuildTargetGroup.WebGL);
            }

        }

        public static void BuildStandaloneFromUI(UnityEditor.BuildTarget buildTarget, bool autoRun = false)
        {
            try
            {
                InitBuildSetup(UnityEditor.BuildTargetGroup.Standalone);
                logger.Debug("Starting Standalone build..." + UnityEditor.PlayerSettings.productName + " : " + UnityEditor.PlayerSettings.bundleVersion);

                UnityEditor.BuildPlayerOptions buildPlayerOptions = new UnityEditor.BuildPlayerOptions
                {
                    locationPathName = getOutputPath(buildTarget),
                    scenes = getScenesForBuild(),
                    target = buildTarget
                };

                buildGame(autoRun, buildPlayerOptions);
            }
            catch (System.Exception e)
            {
                logger.Error(e);
            }
            finally
            {
                Built = true;
                resetBuildSetup(UnityEditor.BuildTargetGroup.Standalone);
            }

        }

        public static void RemoveAltTesterFromScriptingDefineSymbols(UnityEditor.BuildTargetGroup targetGroup)
        {
            RemoveScriptingDefineSymbol(ALTTESTERDEFINE, targetGroup);
        }

        public static void RemoveScriptingDefineSymbol(string symbol, UnityEditor.BuildTargetGroup targetGroup)
        {
            if (AltTesterEditorWindow.EditorConfiguration != null && AltTesterEditorWindow.EditorConfiguration.KeepAUTSymbolDefined)
                return;
            try
            {
                var scriptingDefineSymbolsForGroup =
                    UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
                string newScriptingDefineSymbolsForGroup = "";
                if (scriptingDefineSymbolsForGroup.Contains(symbol))
                {
                    var split = scriptingDefineSymbolsForGroup.Split(';');
                    foreach (var define in split)
                    {
                        if (define != symbol)
                        {
                            newScriptingDefineSymbolsForGroup += define + ";";
                        }
                    }
                    if (newScriptingDefineSymbolsForGroup.Length != 0)
                        newScriptingDefineSymbolsForGroup.Remove(newScriptingDefineSymbolsForGroup.Length - 1);
                    UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup,
                        newScriptingDefineSymbolsForGroup);
                }
            }
            catch (System.Exception e)
            {
                logger.Error("Some Error Happened +" + e.Message);
                logger.Error("Stack trace " + e.StackTrace);
            }
        }

        public static void AddAltTesterInScriptingDefineSymbolsGroup(UnityEditor.BuildTargetGroup targetGroup)
        {
            AddScriptingDefineSymbol(ALTTESTERDEFINE, targetGroup);
        }

        public static void AddScriptingDefineSymbol(string symbol, UnityEditor.BuildTargetGroup targetGroup)
        {
            var scriptingDefineSymbolsForGroup = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            if (!scriptingDefineSymbolsForGroup.Contains(symbol))
            {
                scriptingDefineSymbolsForGroup += ";" + symbol;
            }
            UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, scriptingDefineSymbolsForGroup);
        }

        [System.Obsolete("Use AddAltTesterInScriptingDefineSymbolsGroup instead.")]
        public static void AddAltTesterInScritpingDefineSymbolsGroup(UnityEditor.BuildTargetGroup targetGroup)
        {
            AddAltTesterInScriptingDefineSymbolsGroup(targetGroup);
        }

        public static bool CheckAltTesterIsDefineAsAScriptingSymbol(UnityEditor.BuildTargetGroup targetGroup)
        {
            var scriptingDefineSymbolsForGroup = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            return scriptingDefineSymbolsForGroup.Contains(ALTTESTERDEFINE);
        }

        public static void CreateJsonFileForInputMappingOfAxis()
        {
            string gameDataProjectFilePath = "/Resources/AltTester/AltTesterInputAxisData.json";
            var inputManager = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
            var obj = new UnityEditor.SerializedObject(inputManager);

            UnityEditor.SerializedProperty axisArray = obj.FindProperty("m_Axes");

            if (axisArray.arraySize == 0)
                logger.Info("No Axes");
            var axisList = new List<AltAxis>();
            for (int i = 0; i < axisArray.arraySize; ++i)
            {
                var axis = axisArray.GetArrayElementAtIndex(i);

                var name = axis.FindPropertyRelative("m_Name").stringValue;
                var inputType = (InputType)axis.FindPropertyRelative("type").intValue;
                var negativeButton = axis.FindPropertyRelative("negativeButton").stringValue;
                var positiveButton = axis.FindPropertyRelative("positiveButton").stringValue;
                var altPositiveButton = axis.FindPropertyRelative("altPositiveButton").stringValue;
                var altNegativeButton = axis.FindPropertyRelative("altNegativeButton").stringValue;
                axisList.Add(new AltAxis(name, negativeButton, positiveButton, altPositiveButton, altNegativeButton));
            }

            string dataAsJson = Newtonsoft.Json.JsonConvert.SerializeObject(axisList);
            string filePath = Application.dataPath + gameDataProjectFilePath;
            if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources/AltTester"))
            {
                if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
                }
                UnityEditor.AssetDatabase.CreateFolder("Assets/Resources", "AltTester");
            }
            System.IO.File.WriteAllText(filePath, dataAsJson);
        }

        public static void InsertAltTesterInScene(string scene, AltInstrumentationSettings instrumentationSettings)
        {
            logger.Debug("Adding AltTesterPrefab into the [" + scene + "] scene.");
            var altRunner = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(UnityEditor.AssetDatabase.GUIDToAssetPath(UnityEditor.AssetDatabase.FindAssets("AltTesterPrefab")[0]));

            SceneWithAltTester = EditorSceneManager.OpenScene(scene);
            AltTester = UnityEditor.PrefabUtility.InstantiatePrefab(altRunner);
            var altRunnerComponent = ((GameObject)AltTester).GetComponent<AltRunner>();
            altRunnerComponent.InstrumentationSettings = instrumentationSettings;

            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            logger.Info("AltTesterPrefab successfully modified into the [" + scene + "] scene.");
        }

        public static void InsertAltTesterInTheActiveScene(AltInstrumentationSettings instrumentationSettings)
        {
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            InsertAltTesterInScene(activeScene, instrumentationSettings);
        }

        public static void InsertAltTesterInTheFirstScene(AltInstrumentationSettings instrumentationSettings)
        {
            var altTesterPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(UnityEditor.AssetDatabase.GUIDToAssetPath(UnityEditor.AssetDatabase.FindAssets("AltTesterPrefab")[0]));

            PreviousScenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            SceneWithAltTester = EditorSceneManager.OpenScene(GetFirstSceneWhichWillBeBuilt());

            AltTester = UnityEditor.PrefabUtility.InstantiatePrefab(altTesterPrefab);
            AltRunner altRunnerComponent = ((UnityEngine.GameObject)AltTester).GetComponent<AltRunner>();
            altRunnerComponent.InstrumentationSettings = instrumentationSettings;


            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            try
            {
                EditorSceneManager.OpenScene(PreviousScenePath);
            }
            catch
            {
                logger.Info("No scene was loaded yet.");
            }
        }

        public static string GetFirstSceneWhichWillBeBuilt()
        {
            foreach (var scene in AltTesterEditorWindow.EditorConfiguration.Scenes)
            {
                if (scene.ToBeBuilt)
                {
                    return scene.Path;
                }
            }

            return "";
        }

#if UNITY_EDITOR_OSX


        public static void BuildiOSFromUI(bool autoRun)
        {
            try
            {
                InitBuildSetup(UnityEditor.BuildTargetGroup.iOS);
                logger.Debug("Starting IOS build..." + UnityEditor.PlayerSettings.productName + " : " + UnityEditor.PlayerSettings.bundleVersion);
                UnityEditor.BuildPlayerOptions buildPlayerOptions = new UnityEditor.BuildPlayerOptions();
                buildPlayerOptions.locationPathName = getOutputPath(UnityEditor.BuildTarget.iOS);
                buildPlayerOptions.scenes = getScenesForBuild();

                buildPlayerOptions.target = UnityEditor.BuildTarget.iOS;
                buildGame(autoRun, buildPlayerOptions);

            }
            catch (System.Exception e)
            {
                logger.Error(e);
            }
            finally
            {
                Built = true;
                resetBuildSetup(UnityEditor.BuildTargetGroup.iOS);
            }

        }
#endif

        private static string getOutputPath(UnityEditor.BuildTarget target)
        {
            var outputPath = AltTesterEditorWindow.EditorConfiguration.BuildLocationPath;

            outputPath = string.IsNullOrEmpty(outputPath) ? "build" : outputPath;
            outputPath = System.IO.Path.Combine(outputPath, UnityEditor.PlayerSettings.productName);

            switch (target)
            {
                case UnityEditor.BuildTarget.Android:
                    if (!outputPath.EndsWith(".apk"))
                    {
                        outputPath = outputPath + ".apk";
                    }

                    break;
                case UnityEditor.BuildTarget.StandaloneOSX:
                    if (!outputPath.EndsWith(".app"))
                    {
                        outputPath = outputPath + ".app";
                    }
                    break;
                case UnityEditor.BuildTarget.StandaloneWindows:
                case UnityEditor.BuildTarget.StandaloneWindows64:
                    if (!outputPath.EndsWith(".exe"))
                    {
                        outputPath = outputPath + ".exe";
                    }
                    break;
                case UnityEditor.BuildTarget.StandaloneLinux64:
                    if (!outputPath.EndsWith(".x86_64"))
                    {
                        outputPath = outputPath + ".x86_64";
                    }
                    break;

                case UnityEditor.BuildTarget.iOS:
                default:
                    break;

            }

            return outputPath;
        }

        private static void resetBuildSetup(UnityEditor.BuildTargetGroup buildTargetGroup)
        {
            if (AltTesterEditorWindow.EditorConfiguration.appendToName)
            {
                UnityEditor.PlayerSettings.productName = UnityEditor.PlayerSettings.productName.Remove(UnityEditor.PlayerSettings.productName.Length - 5);
                string bundleIdentifier = UnityEditor.PlayerSettings.GetApplicationIdentifier(buildTargetGroup).Remove(UnityEditor.PlayerSettings.GetApplicationIdentifier(buildTargetGroup).Length - 5);
                UnityEditor.PlayerSettings.SetApplicationIdentifier(buildTargetGroup, bundleIdentifier);
            }

            RemoveAltTesterFromScriptingDefineSymbols(buildTargetGroup);
        }

        private static void buildGame(bool autoRun, UnityEditor.BuildPlayerOptions buildPlayerOptions)
        {
            UnityEditor.PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            UnityEditor.PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
            UnityEditor.PlayerSettings.SetStackTraceLogType(LogType.Assert, StackTraceLogType.None);

            if (autoRun)
            {
                buildPlayerOptions.options = UnityEditor.BuildOptions.Development | UnityEditor.BuildOptions.AutoRunPlayer | UnityEditor.BuildOptions.IncludeTestAssemblies;
            }
            else
            {
                buildPlayerOptions.options = UnityEditor.BuildOptions.Development | UnityEditor.BuildOptions.ShowBuiltPlayer | UnityEditor.BuildOptions.IncludeTestAssemblies;
            }
            var results = UnityEditor.BuildPipeline.BuildPlayer(buildPlayerOptions);


#if UNITY_2017
            if (results.Equals(""))
            {
                logger.Info("Build path: " + buildPlayerOptions.locationPathName);
                logger.Info("Build " + UnityEditor.PlayerSettings.productName + ":" + UnityEditor.PlayerSettings.bundleVersion + " Succeeded");
            }
            else
            {
                logger.Error("Build Error!");
            }

#else
            if (results.summary.totalErrors == 0 || results.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                logger.Info("Build path: " + buildPlayerOptions.locationPathName);
                logger.Info("Build " + UnityEditor.PlayerSettings.productName + ":" + UnityEditor.PlayerSettings.bundleVersion + " Succeeded");
            }
            else
            {
                logger.Error("Build Error! " + results.steps + "\n Result: " + results.summary.result +
                           "\n Stripping info: " + results.strippingInfo);
            }
#endif

        }

        private static string[] getScenesForBuild()
        {
            if (AltTesterEditorWindow.EditorConfiguration.Scenes.Count == 0)
            {
                AltTesterEditorWindow.AddAllScenes();
                AltTesterEditorWindow.SelectAllScenes();
            }
            var sceneList = new List<string>();
            foreach (var scene in AltTesterEditorWindow.EditorConfiguration.Scenes)
            {
                if (scene.ToBeBuilt)
                {
                    sceneList.Add(scene.Path);
                }
            }

            InsertAltTesterInTheFirstScene(AltTesterEditorWindow.EditorConfiguration.GetInstrumentationSettings());

            return sceneList.ToArray();
        }
    }
}