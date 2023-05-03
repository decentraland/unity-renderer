﻿using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build.AnalyzeRules;
using UnityEngine;

public class AddressablesValidationTests
{
    private const string NO_ISSUES_FOUND = "No issues found";
    private static readonly string[] EXCLUDED_FILE_TYPES = { }; // "shader", "png", "jpg"

    [TestCase("Rendering")] [Category("EditModeCI")]
    public void ValidateFolderDoesNotHaveResourcesFolderInside(string folderName)
    {
        string folderPath = Application.dataPath + $"/{folderName}";
        var directory = new DirectoryInfo(folderPath);

        if (!directory.Exists)
            Assert.Fail($"{folderName} does not exist");

        bool hasResourcesFolder = directory.GetDirectories("*", SearchOption.AllDirectories).Any(subDirectory => subDirectory.Name == "Resources");
        Assert.IsFalse(hasResourcesFolder, $"{folderName} folder or its sub-folders contain Resources folder");
    }

    [Test] [Category("EditModeCI")]
    public void ValidateDuplicateBundleDependencies()
    {
        var rule = new CheckBundleDupeDependencies();
        List<AnalyzeRule.AnalyzeResult> duplicates = rule.RefreshAnalysis(AddressableAssetSettingsDefaultObject.Settings);

        if (duplicates[0].resultName == NO_ISSUES_FOUND)
            return;

        Dictionary<string, (List<string>, List<string> assets)> bundlesByAsset = GroupBundlesByDuplicatedAssets(duplicates, isCustomGroupsRule: true);
        string msg = CreateDuplicatesMessage(bundlesByAsset, EXCLUDED_FILE_TYPES);

        Assert.That(msg, Is.Empty,
            message: ComposeAssertMessage(msg, analyzeRule: "Check Duplicate Bundle Dependencies in Addressables->Analyze tool"));
    }

    [Test] [Category("EditModeCI")]
    public void ValidateScenesToAddressableDuplicateDependencies()
    {
        var rule = new CheckSceneDupeDependencies();
        List<AnalyzeRule.AnalyzeResult> duplicates = rule.RefreshAnalysis(AddressableAssetSettingsDefaultObject.Settings);

        if (duplicates[0].resultName.Contains(NO_ISSUES_FOUND))
            return;

        Dictionary<string, (List<string> Scenes, List<string> Bundles)> scenesAndBundlesByAsset = GroupScenesAndBundlesByDuplicatedAsset(duplicates);
        string msg = CreateScenesDuplicatesMessage(scenesAndBundlesByAsset, EXCLUDED_FILE_TYPES);

        Assert.That(msg, Is.Empty,
            message: ComposeAssertMessage(msg, analyzeRule: "Check Scene to Addressable Duplicate Dependencies in Addressables->Analyze tool"));
    }

    [Test] [Category("ToFix")]
    public void ValidateResourcesToAddressableDuplicateDependencies()
    {
        var rule = new CheckResourcesDupeDependencies();
        List<AnalyzeRule.AnalyzeResult> duplicates = rule.RefreshAnalysis(AddressableAssetSettingsDefaultObject.Settings);

        if (duplicates.Count == 0 || duplicates[0].resultName == NO_ISSUES_FOUND) return;

        Dictionary<string, (List<string>, List<string> assets)> bundlesByResource = GroupBundlesByDuplicatedAssets(duplicates);

        foreach (KeyValuePair<string,(List<string>, List<string> assets)> keyValuePair in bundlesByResource)
        {
            long size = 0;
            foreach (var asset in keyValuePair.Value.assets)
            {
                string path;

                if (asset.Contains("Assets"))
                    path = Application.dataPath + asset.Remove(0, "Assets".Length);
                else
                    path = Application.dataPath + Path.DirectorySeparatorChar + asset;

                size += new FileInfo(path).Length;
            }
            AssetDatabase.

            Debug.Log($"---- RESOURCE {keyValuePair.Key} of Count {keyValuePair.Value.assets.Count} of Size {size} ----");
        }
        string msg = CreateDuplicatesMessage(bundlesByResource, EXCLUDED_FILE_TYPES, 50);

        Assert.That(msg, Is.Empty,
            message: ComposeAssertMessage(msg, analyzeRule: "Check Resources to Addressable Duplicate Dependencies in Addressables->Analyze tool"));
    }

    private static Dictionary<string, (List<string> bundles, List<string> assets)> GroupBundlesByDuplicatedAssets(List<AnalyzeRule.AnalyzeResult> duplicates, bool isCustomGroupsRule = false)
    {
        Dictionary<string, (List<string> bundles, List<string> assets)> bundlesByAsset = new ();

        foreach (AnalyzeRule.AnalyzeResult duplicate in duplicates)
        {
            string[] dSplit = duplicate.resultName.Split(':');

            string dResource = isCustomGroupsRule && Application.isBatchMode ? dSplit[^1] : dSplit[0];
            string dBundle = dSplit[1];
            string dAsset = isCustomGroupsRule && Application.isBatchMode ? dSplit[0] : dSplit[^1];

            if (!bundlesByAsset.ContainsKey(dResource))
                bundlesByAsset.Add(dResource, (new List<string> { dBundle }, new List<string> { dAsset }));
            else
            {
                if (!bundlesByAsset[dResource].bundles.Contains(dBundle))
                    bundlesByAsset[dResource].bundles.Add(dBundle);

                if (!bundlesByAsset[dResource].assets.Contains(dAsset))
                    bundlesByAsset[dResource].assets.Add(dAsset);
            }
        }

        return bundlesByAsset;
    }

    private static Dictionary<string, (List<string> Scenes, List<string> Bundles)> GroupScenesAndBundlesByDuplicatedAsset(List<AnalyzeRule.AnalyzeResult> duplicates)
    {
        Dictionary<string, (List<string> Scenes, List<string> Bundles)> scenesAndBundlesByAsset = new ();

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

    private static string CreateDuplicatesMessage(Dictionary<string, (List<string>, List<string> assets)> bundlesByAsset, string[] excludedFileTypesFilter, int minDuplicatesAmount = 0)
    {
        var message = new StringBuilder();

        foreach (KeyValuePair<string, (List<string>, List<string> assets)> keyValuePair in bundlesByAsset
                    .Where(keyValuePair => !excludedFileTypesFilter.Contains(keyValuePair.Key.Split('.')[^1])))
        {
            if (keyValuePair.Value.Item2.Count <= minDuplicatesAmount)
                continue;

            message.Append("ASSET: " + keyValuePair.Key);

            message.Append(" - AMOUNT: " + keyValuePair.Value.Item2.Count);

            message.Append(" - BUNDLES: ");

            foreach (string bundle in keyValuePair.Value.Item1)
                message.Append(bundle.Split('.')[0] + ", ");

            message.Remove(message.Length - 2, 2);
            message.Append(";\n");
        }

        return message.ToString();
    }

    private static string CreateScenesDuplicatesMessage(Dictionary<string, (List<string> Scenes, List<string> Bundles)> scenesAndBundlesByAsset, string[] excludedFileTypesFilter)
    {
        var message = new StringBuilder();

        foreach (KeyValuePair<string, (List<string> Scenes, List<string> Bundles)> keyValuePair in scenesAndBundlesByAsset
                    .Where(keyValuePair => !excludedFileTypesFilter.Contains(keyValuePair.Key.Split('.')[^1])))
        {
            message.Append("ASSET: " + keyValuePair.Key);

            message.Append(" - SCENES: ");

            foreach (string scene in keyValuePair.Value.Scenes)
                message.Append(scene + ", ");

            message.Append("- BUNDLES: ");

            foreach (string bundle in keyValuePair.Value.Bundles)
                message.Append(bundle.Split('.')[0] + ", ");

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
