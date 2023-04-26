using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build.AnalyzeRules;
using UnityEngine;

public class AddressablesValidationTests
{
    private static readonly string[] EXCLUDED_FILE_TYPES = { }; // "shader", "png", "jpg"
    private const string NO_ISSUES_FOUND = "No issues found";

    [TestCase("Rendering")][Category("EditModeCI")]
    public void ValidateFolderDoesNotHaveResourcesFolderInside(string folderName)
    {
        string folderPath = Application.dataPath + $"/{folderName}";
        DirectoryInfo directory = new DirectoryInfo(folderPath);

        if (!directory.Exists)
            Assert.Fail($"{folderName} does not exist");

        bool hasResourcesFolder = directory.GetDirectories("*", SearchOption.AllDirectories).Any(subDirectory => subDirectory.Name == "Resources");
        Assert.IsFalse(hasResourcesFolder, $"{folderName} folder or its sub-folders contain Resources folder");
    }


    [Test][Category("EditModeCI")]
    public void ValidateDuplicateBundleDependencies()
    {
        CheckBundleDupeDependencies rule = new CheckBundleDupeDependencies();
        List<AnalyzeRule.AnalyzeResult> duplicates = rule.RefreshAnalysis(AddressableAssetSettingsDefaultObject.Settings);

        if (duplicates[0].resultName == NO_ISSUES_FOUND)
            return;

        Dictionary<string, List<string>> bundlesByAsset = GroupBundlesByDuplicatedAssets(duplicates, isCustomGroupsRule: true);
        string msg = CreateDuplicatesMessage(bundlesByAsset, EXCLUDED_FILE_TYPES);

        Assert.That(msg, Is.Empty,
            message: ComposeAssertMessage(msg, analyzeRule: "Check Duplicate Bundle Dependencies"));
    }

    [Test][Category("ToFix")]
    public void ValidateResourcesToAddressableDuplicateDependencies()
    {
        CheckResourcesDupeDependencies rule = new CheckResourcesDupeDependencies();
        List<AnalyzeRule.AnalyzeResult> duplicates = rule.RefreshAnalysis(AddressableAssetSettingsDefaultObject.Settings);

        if (duplicates[0].resultName == NO_ISSUES_FOUND)
            return;

        Dictionary<string, List<string>> bundlesByResource = GroupBundlesByDuplicatedAssets(duplicates);
        string msg = CreateDuplicatesMessage(bundlesByResource, EXCLUDED_FILE_TYPES);

        Assert.That(msg, Is.Empty,
            message: ComposeAssertMessage(msg, analyzeRule: "Check Resources to Addressable Duplicate Dependencies"));
    }

    [Test][Category("ToFix")]
    public void ValidateScenesToAddressableDuplicateDependencies()
    {
        CheckSceneDupeDependencies rule = new CheckSceneDupeDependencies();
        List<AnalyzeRule.AnalyzeResult> duplicates = rule.RefreshAnalysis(AddressableAssetSettingsDefaultObject.Settings);

        if (duplicates[0].resultName == NO_ISSUES_FOUND)
            return;

        Dictionary<string, (List<string> Scenes, List<string> Bundles)> scenesAndBundlesByAsset = GroupScenesAndBundlesByDuplicatedAsset(duplicates);
        string msg = CreateScenesDuplicatesMessage(scenesAndBundlesByAsset, EXCLUDED_FILE_TYPES);

        Assert.That(msg, Is.Empty,
            message: ComposeAssertMessage(msg, analyzeRule: "Check Scene to Addressable Duplicate Dependencies"));
    }

    private static Dictionary<string, List<string>> GroupBundlesByDuplicatedAssets(List<AnalyzeRule.AnalyzeResult> duplicates, bool isCustomGroupsRule = false)
    {
        Dictionary<string, List<string>> bundlesByAsset = new ();

        foreach (AnalyzeRule.AnalyzeResult duplicate in duplicates)
        {
            string[] dSplit = duplicate.resultName.Split(':');

            string dAsset = isCustomGroupsRule && Application.isBatchMode ? dSplit[^1] : dSplit[0];
            string dBundle = dSplit[1];

            if (!bundlesByAsset.ContainsKey(dAsset))
                bundlesByAsset.Add(dAsset, new List<string> { dBundle });
            else if (!bundlesByAsset[dAsset].Contains(dBundle))
                bundlesByAsset[dAsset].Add(dBundle);
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

    private static string CreateDuplicatesMessage(Dictionary<string, List<string>> bundlesByAsset, string[] excludedFileTypesFilter)
    {
        StringBuilder message = new StringBuilder();

        foreach (var keyValuePair in bundlesByAsset
                    .Where(keyValuePair => !excludedFileTypesFilter.Contains(keyValuePair.Key.Split('.')[^1])))
        {
            message.Append("ASSET: " + keyValuePair.Key);

            message.Append(" - BUNDLES: ");

            foreach (string bundle in keyValuePair.Value)
                message.Append(bundle.Split('.')[0] + ", ");

            message.Remove(message.Length - 2, 2);
            message.Append(";\n");
        }

        return message.ToString();
    }

    private static string CreateScenesDuplicatesMessage(Dictionary<string, (List<string> Scenes, List<string> Bundles)> scenesAndBundlesByAsset, string[] excludedFileTypesFilter)
    {
        StringBuilder message = new StringBuilder();

        foreach (var keyValuePair in scenesAndBundlesByAsset
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
