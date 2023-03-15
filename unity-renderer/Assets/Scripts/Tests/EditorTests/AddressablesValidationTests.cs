using UnityEditor.AddressableAssets;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets.Build.AnalyzeRules;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEngine;

namespace Tests.ValidationTests
{
    public class AddressablesValidationTests
    {
        private static readonly string[] EXCLUDED_FILE_TYPES = {  }; // "shader", "png", "jpg"

        [Test]
        public void ValidateResourcesToAddressableDuplicateDependencies()
        {
            CheckResourcesDupeDependencies rule = new CheckResourcesDupeDependencies();
            List<AnalyzeRule.AnalyzeResult> duplicates = rule.RefreshAnalysis(AddressableAssetSettingsDefaultObject.Settings);
       }

        [Test]
        public void ValidateScenesToAddressableDuplicateDependencies()
        {
            CheckSceneDupeDependencies rule = new CheckSceneDupeDependencies();
            List<AnalyzeRule.AnalyzeResult> duplicates = rule.RefreshAnalysis(AddressableAssetSettingsDefaultObject.Settings);
        }

        [Test]
        public void ValidateDuplicateBundleDependencies()
        {
            CheckBundleDupeDependencies rule = new CheckBundleDupeDependencies();
            List<AnalyzeRule.AnalyzeResult> duplicates = rule.RefreshAnalysis(AddressableAssetSettingsDefaultObject.Settings);

            Dictionary<string, List<string>> entriesToGroups = new Dictionary<string, List<string>>();

            foreach (AnalyzeRule.AnalyzeResult duplicate in duplicates)
            {
                string[] dSplit = duplicate.resultName.Split(':');
                string dPath = dSplit[0];
                string dBundle = dSplit[^1];

                if (!entriesToGroups.ContainsKey(dPath))
                    entriesToGroups.Add(dPath, new List<string> { dBundle });
                else if (!entriesToGroups[dPath].Contains(dBundle))
                    entriesToGroups[dPath].Add(dBundle);
            }

            StringBuilder message = new StringBuilder();

            foreach (var keyValuePair in entriesToGroups
                        .Where(keyValuePair => !EXCLUDED_FILE_TYPES.Contains(keyValuePair.Key.Split('.')[^1])))
            {
                message.Append(keyValuePair.Key + " - Groups: ");

                foreach (string group in keyValuePair.Value)
                    message.Append(group + ", ");

                message.Remove(message.Length - 2, 2);
                message.Append(";\n");
            }

            Assert.That(message.ToString(), Is.Empty,
                message: $"Found {message.ToString().Split(';').Length} assets being duplicated between groups: \n{message}");
        }

        public void Duplicates_Custom()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            string[] builtInResourcesPaths = ScenePaths(); // or ResourcesPaths()

            List<AnalyzeRule.AnalyzeResult> results = new List<AnalyzeRule.AnalyzeResult>();

            List<GUID> m_AddressableAssets = (from aaGroup in settings.groups
                where aaGroup != null
                from entry in aaGroup.entries
                select new GUID(entry.guid)).ToList();

            Dictionary<string, List<GUID>> m_ResourcesToDependencies = BuiltInResourcesToDependenciesMap(builtInResourcesPaths);

            if (m_ResourcesToDependencies == null || m_ResourcesToDependencies.Count == 0)
                return;

            CalculateInputDefinitions(settings);
        }

        private readonly List<AddressableAssetEntry> m_AssetEntries = new ();
        private readonly Dictionary<string, string> m_BundleToAssetGroup = new ();
        private readonly List<AssetBundleBuild> m_AllBundleInputDefs = new ();

        /// <summary>
        /// Generate input definitions and entries for AssetBundleBuild
        /// </summary>
        /// <param name="settings">The current Addressables settings object</param>
        private void CalculateInputDefinitions(AddressableAssetSettings settings)
        {
            int updateFrequency = Mathf.Max(settings.groups.Count / 10, 1);
            var progressDisplayed = false;

            for (var groupIndex = 0; groupIndex < settings.groups.Count; ++groupIndex)
            {
                AddressableAssetGroup group = settings.groups[groupIndex];

                if (group == null)
                    continue;

                if (!progressDisplayed || groupIndex % updateFrequency == 0)
                {
                    progressDisplayed = true;

                    if (EditorUtility.DisplayCancelableProgressBar("Calculating Input Definitions", "",
                            (float)groupIndex / settings.groups.Count))
                    {
                        m_AssetEntries.Clear();
                        m_BundleToAssetGroup.Clear();
                        m_AllBundleInputDefs.Clear();
                        break;
                    }
                }

                var schema = group.GetSchema<BundledAssetGroupSchema>();

                if (schema != null && schema.IncludeInBuild)
                {
                    List<AssetBundleBuild> bundleInputDefinitions = new List<AssetBundleBuild>();
                    m_AssetEntries.AddRange(BuildScriptPackedMode.PrepGroupBundlePacking(group, bundleInputDefinitions, schema));

                    for (var i = 0; i < bundleInputDefinitions.Count; i++)
                    {
                        if (m_BundleToAssetGroup.ContainsKey(bundleInputDefinitions[i].assetBundleName))
                            bundleInputDefinitions[i] = CreateUniqueBundle(bundleInputDefinitions[i], m_BundleToAssetGroup);

                        m_BundleToAssetGroup.Add(bundleInputDefinitions[i].assetBundleName, schema.Group.Guid);
                    }

                    m_AllBundleInputDefs.AddRange(bundleInputDefinitions);
                }
            }

            if (progressDisplayed)
                EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// Create new AssetBundleBuild
        /// </summary>
        /// <param name="bid">ID for new AssetBundleBuild</param>
        /// <param name="bundleToAssetGroup"> Map of bundle names to asset group Guids</param>
        /// <returns></returns>
        private static AssetBundleBuild CreateUniqueBundle(AssetBundleBuild bid, Dictionary<string, string> bundleToAssetGroup)
        {
            var count = 1;
            string newName = bid.assetBundleName;

            while (bundleToAssetGroup.ContainsKey(newName) && count < 1000)
                newName = bid.assetBundleName.Replace(".bundle", $"{count++}.bundle");

            return new AssetBundleBuild
            {
                assetBundleName = newName,
                addressableNames = bid.addressableNames,
                assetBundleVariant = bid.assetBundleVariant,
                assetNames = bid.assetNames
            };
        }

        /// <summary>
        /// Build map of resources to corresponding dependencies
        /// </summary>
        /// <param name="resourcePaths"> Array of resource paths</param>
        private static Dictionary<string, List<GUID>> BuiltInResourcesToDependenciesMap(string[] resourcePaths)
        {
            Dictionary<string, List<GUID>> m_ResourcesToDependencies = new ();

            foreach (string path in resourcePaths)
            {
                string[] dependencies = path.EndsWith(".unity")
                    ? GetSceneDependencies(path)
                    : AssetDatabase.GetDependencies(path);

                if (!m_ResourcesToDependencies.ContainsKey(path))
                    m_ResourcesToDependencies.Add(path, new List<GUID>(dependencies.Length));
                else
                    m_ResourcesToDependencies[path].Capacity += dependencies.Length;

                foreach (string dependency in dependencies)
                    if (!dependency.EndsWith(".cs") && !dependency.EndsWith(".dll"))
                        m_ResourcesToDependencies[path].Add(new GUID(AssetDatabase.AssetPathToGUID(dependency)));
            }

            return m_ResourcesToDependencies;
        }

        private static string[] GetSceneDependencies(string path)
        {
            using var buildWrapper = new BuildInterfacesWrapper();

            var usageTags = new BuildUsageTagSet();

            BuildSettings settings = new BuildSettings
            {
                group = EditorUserBuildSettings.selectedBuildTargetGroup,
                target = EditorUserBuildSettings.activeBuildTarget,
                typeDB = null,
                buildFlags = ContentBuildFlags.None
            };

            SceneDependencyInfo sceneInfo =
                ContentBuildInterface.CalculatePlayerDependenciesForScene(path, settings, usageTags);

            var dependencies = new string[sceneInfo.referencedObjects.Count];

            for (var i = 0; i < sceneInfo.referencedObjects.Count; ++i)
                if (string.IsNullOrEmpty(sceneInfo.referencedObjects[i].filePath))
                    dependencies[i] = AssetDatabase.GUIDToAssetPath(sceneInfo.referencedObjects[i].guid.ToString());
                else
                    dependencies[i] = sceneInfo.referencedObjects[i].filePath;

            return dependencies;
        }

        private static string[] ScenePaths() =>
            (from editorScene in EditorBuildSettings.scenes
                where editorScene.enabled
                select editorScene.path).ToArray();

        private static string[] ResourcesPaths()
        {
            string[] resourceDirectory = Directory.GetDirectories("Assets", "Resources", SearchOption.AllDirectories);

            List<string> resourcePaths = new List<string>();

            foreach (string directory in resourceDirectory)
                resourcePaths.AddRange(Directory.GetFiles(directory, "*", SearchOption.AllDirectories));

            return resourceDirectory;
        }
    }
}

//
// public class AddressablesValidationTests
// {
//     private static readonly string[] ALLOWED_GROUPS = { "HUDs", "Desktop" };
//
//     [Test]
//     public void CheckForDuplicates()
//     {
//         var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
//         var entries = settings.groups.SelectMany(g => g.entries).ToList();
//         var bundleToEntries = new Dictionary<string, List<AddressableAssetEntry>>();
//         var entriesToBundles = new Dictionary<AddressableAssetEntry, List<string>>();
//
//         // Build a dictionary that maps bundle names to the entries they contain
//         foreach (AddressableAssetEntry entry in entries)
//         {
//             if (entry.IsSubAsset || !entry.HasDependencies)
//                 continue;
//
//             var mainAsset = entry.MainAsset;
//             var dependencies = mainAsset.GetDependencies();
//
//             foreach (var dependency in dependencies)
//             {
//                 if (dependency == null)
//                     continue;
//
//                 var dependentEntry = settings.FindAssetEntry(dependency);
//
//                 if (dependentEntry == null || dependentEntry.IsSubAsset)
//                     continue;
//
//                 var bundleName = dependentEntry.parentGroup.GetBundleCachedName(dependentEntry, settings.GetUniqueAssetPath(dependency));
//
//                 if (bundleName == null)
//                     continue;
//
//                 if (!bundleToEntries.TryGetValue(bundleName, out var entryList))
//                 {
//                     entryList = new List<AddressableAssetEntry>();
//                     bundleToEntries[bundleName] = entryList;
//                 }
//
//                 entryList.Add(entry);
//
//                 if (!entriesToBundles.TryGetValue(entry, out var bundleList))
//                 {
//                     bundleList = new List<string>();
//                     entriesToBundles[entry] = bundleList;
//                 }
//
//                 bundleList.Add(bundleName);
//             }
//         }
//
//         // Find entries that are in multiple bundles
//         var duplicateEntries = new HashSet<AddressableAssetEntry>();
//
//         foreach (var entry in entries)
//         {
//             if (entry.IsSubAsset)
//                 continue;
//
//             if (entriesToBundles.TryGetValue(entry, out var bundleList) && bundleList.Count > 1) { duplicateEntries.Add(entry); }
//         }
//
//         // Find entries that are in the same bundle as other entries
//         var duplicateBundleEntries = new Dictionary<string, List<AddressableAssetEntry>>();
//
//         foreach (var kvp in bundleToEntries)
//         {
//             var bundleName = kvp.Key;
//             var entryList = kvp.Value;
//
//             if (entryList.Count > 1)
//             {
//                 foreach (var entry in entryList)
//                 {
//                     if (!duplicateBundleEntries.TryGetValue(bundleName, out var duplicateEntryList))
//                     {
//                         duplicateEntryList = new List<AddressableAssetEntry>();
//                         duplicateBundleEntries[bundleName] = duplicateEntryList;
//                     }
//
//                     duplicateEntryList.Add(entry);
//                 }
//             }
//         }
//
//         // Check for duplicates in the same bundle
//         var duplicateBundles = new HashSet<string>();
//
//         foreach (var kvp in duplicateBundleEntries)
//         {
//             var bundleName = kvp.Key;
//             var entryList = kvp.Value;
//             var uniqueEntries = entryList.Distinct().ToList();
//
//             if (uniqueEntries.Count < entryList.Count) { duplicateBundles.Add(bundleName); }
//         }
//     }
// }
//
