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
        private static readonly string[] EXCLUDED_FILE_TYPES = { }; // "shader", "png", "jpg"

        [Test]
        public void ValidateDuplicateBundleDependencies()
        {
            CheckBundleDupeDependencies rule = new CheckBundleDupeDependencies();
            List<AnalyzeRule.AnalyzeResult> duplicates = rule.RefreshAnalysis(AddressableAssetSettingsDefaultObject.Settings);

            Dictionary<string, List<string>> bundlesByAsset = GroupDuplicatesByAssets(duplicates, isCustomGroupsRule: true);
            string msg = CreateDuplicatesMessage(bundlesByAsset);

            Assert.That(msg, Is.Empty,
                message: ComposeAssertMessage(msg, analyzeRule: "Check Duplicate Bundle Dependencies"));
        }

        [Test]
        public void ValidateResourcesToAddressableDuplicateDependencies()
        {
            CheckResourcesDupeDependencies rule = new CheckResourcesDupeDependencies();
            List<AnalyzeRule.AnalyzeResult> duplicates = rule.RefreshAnalysis(AddressableAssetSettingsDefaultObject.Settings);

            Dictionary<string, List<string>> bundlesByResource = GroupDuplicatesByAssets(duplicates);
            string msg = CreateDuplicatesMessage(bundlesByResource);

            Assert.That(msg, Is.Empty,
                message: ComposeAssertMessage(msg, analyzeRule: "Check Resources to Addressable Duplicate Dependencies"));
        }

        [Test]
        public void ValidateScenesToAddressableDuplicateDependencies()
        {
            CheckSceneDupeDependencies rule = new CheckSceneDupeDependencies();
            List<AnalyzeRule.AnalyzeResult> duplicates = rule.RefreshAnalysis(AddressableAssetSettingsDefaultObject.Settings);

            Dictionary<string, (List<string> Scenes, List<string> Bundles)> scenesAndBundlesByAsset = GroupScenesAndBundlesByDuplicatedAsset(duplicates);
            string msg = CreateScenesDuplicatesMessage(scenesAndBundlesByAsset);

            Assert.That(msg, Is.Empty,
                message: ComposeAssertMessage(msg, analyzeRule: "Check Scene to Addressable Duplicate Dependencies"));
        }

        private static Dictionary<string, List<string>> GroupDuplicatesByAssets(List<AnalyzeRule.AnalyzeResult> duplicates, bool isCustomGroupsRule = false)
        {
            Dictionary<string, List<string>> bundlesByAsset = new ();

            foreach (AnalyzeRule.AnalyzeResult duplicate in duplicates)
            {
                string[] dSplit = duplicate.resultName.Split(':');

                string dAsset = isCustomGroupsRule && Application.isBatchMode ? dSplit[^1] : dSplit[0];
                string dBundle = isCustomGroupsRule && Application.isBatchMode ? dSplit[0] : dSplit[^1];

                if (!bundlesByAsset.ContainsKey(dAsset))
                    bundlesByAsset.Add(dAsset, new List<string> { dBundle });
                else if (!bundlesByAsset[dAsset].Contains(dBundle))
                    bundlesByAsset[dAsset].Add(dBundle);
            }

            return bundlesByAsset;
        }

        private static Dictionary<string, (List<string> Scenes, List<string> Bundles)> GroupScenesAndBundlesByDuplicatedAsset(List<AnalyzeRule.AnalyzeResult> duplicates)
        {
            Dictionary<string, (List<string> Scenes, List<string> Bundles)> scenesAndBundlesByAsset = new();

            foreach (AnalyzeRule.AnalyzeResult duplicate in duplicates)
            {
                string[] dSplit = duplicate.resultName.Split(':');

                string dScene = dSplit[0];
                string dBundle = dSplit[1];
                string dAsset = dSplit[2];

                if (!scenesAndBundlesByAsset.ContainsKey(dAsset))
                    scenesAndBundlesByAsset.Add(dAsset, (new List<string> { dScene }, new List<string> { dBundle }));
                else
                {
                    if (!scenesAndBundlesByAsset[dAsset].Scenes.Contains(dScene))
                        scenesAndBundlesByAsset[dAsset].Scenes.Add(dScene);

                    if (!scenesAndBundlesByAsset[dAsset].Bundles.Contains(dBundle))
                        scenesAndBundlesByAsset[dAsset].Bundles.Add(dBundle);
                }
            }

            return scenesAndBundlesByAsset;
        }

        private static string CreateDuplicatesMessage(Dictionary<string, List<string>> bundlesByAsset)
        {
            StringBuilder message = new StringBuilder();

            foreach (var keyValuePair in bundlesByAsset
                        .Where(keyValuePair => !EXCLUDED_FILE_TYPES.Contains(keyValuePair.Key.Split('.')[^1])))
            {
                message.Append("ASSET: " + keyValuePair.Key + " - GROUPS: ");

                foreach (string group in keyValuePair.Value)
                    message.Append(group + ", ");

                message.Remove(message.Length - 2, 2);
                message.Append(";\n");
            }

            return message.ToString();
        }

        private static string CreateScenesDuplicatesMessage(Dictionary<string, (List<string> Scenes, List<string> Bundles)> scenesAndBundlesByAsset)
        {
            StringBuilder message = new StringBuilder();

            foreach (var keyValuePair in scenesAndBundlesByAsset
                        .Where(keyValuePair => !EXCLUDED_FILE_TYPES.Contains(keyValuePair.Key.Split('.')[^1])))
            {
                message.Append("ASSET: " + keyValuePair.Key + " - SCENES: ");

                foreach (string group in keyValuePair.Value.Scenes)
                    message.Append(group + ", ");

                message.Append("- BUNDLES: ");

                foreach (string group in keyValuePair.Value.Bundles)
                    message.Append(group + ", ");

                message.Remove(message.Length - 2, 2);
                message.Append(";\n");
            }

            return message.ToString();
        }

        private static string ComposeAssertMessage(string message, string analyzeRule) =>
            $"Found {message.Split(';').Length} assets being duplicated between Addressables Custom Groups. "
            + $"Run Addressables Analyze (inside Unity) with {analyzeRule} rule and fix them."
            + $"Duplicates are: \n{message}";
    }
}
