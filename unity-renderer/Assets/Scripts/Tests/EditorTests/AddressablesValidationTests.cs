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
        public void ValidateResourcesToAddressableDuplicateDependencies()
        {
            CheckResourcesDupeDependencies rule = new CheckResourcesDupeDependencies();
            List<AnalyzeRule.AnalyzeResult> duplicates = rule.RefreshAnalysis(AddressableAssetSettingsDefaultObject.Settings);

            foreach (AnalyzeRule.AnalyzeResult duplicate in duplicates) { Debug.Log(duplicate.resultName); }
        }

        [Test]
        public void ValidateScenesToAddressableDuplicateDependencies()
        {
            CheckSceneDupeDependencies rule = new CheckSceneDupeDependencies();
            List<AnalyzeRule.AnalyzeResult> duplicates = rule.RefreshAnalysis(AddressableAssetSettingsDefaultObject.Settings);

            Dictionary<string, (List<string> Scenes, List<string> Bundles)> entriesToGroups = new ();

            foreach (AnalyzeRule.AnalyzeResult duplicate in duplicates)
            {
                Debug.Log(duplicate.resultName);
                string[] dSplit = duplicate.resultName.Split(':');

                string dScene = dSplit[0];
                string dBundle = dSplit[1];
                string dAsset = dSplit[2];

                if (!entriesToGroups.ContainsKey(dAsset))
                    entriesToGroups.Add(dAsset, (new List<string> { dScene } ,new List<string> { dBundle }));
                else
                {
                    if (!entriesToGroups[dAsset].Scenes.Contains(dScene))
                        entriesToGroups[dAsset].Scenes.Add(dScene);

                    if (!entriesToGroups[dAsset].Bundles.Contains(dBundle))
                        entriesToGroups[dAsset].Bundles.Add(dBundle);
                }
            }

            StringBuilder message = new StringBuilder();

            foreach (var keyValuePair in entriesToGroups
                        .Where(keyValuePair => !EXCLUDED_FILE_TYPES.Contains(keyValuePair.Key.Split('.')[^1])))
            {
                message.Append(keyValuePair.Key + " - SCENES: ");

                foreach (string group in keyValuePair.Value.Scenes)
                    message.Append(group + ", ");

                message.Append(" - BUNDLES: ");
                foreach (string group in keyValuePair.Value.Bundles)
                    message.Append(group + ", ");


                message.Remove(message.Length - 2, 2);
                message.Append(";\n");
            }

            Assert.That(message.ToString(), Is.Empty, message: $"Found {message.ToString().Split(';').Length} assets being duplicated between scenes and bundles: \n{message}");
        }

        [Test]
        public void ValidateDuplicateBundleDependencies()
        {
            CheckBundleDupeDependencies rule = new CheckBundleDupeDependencies();
            List<AnalyzeRule.AnalyzeResult> duplicates = rule.RefreshAnalysis(AddressableAssetSettingsDefaultObject.Settings);

            Dictionary<string, List<string>> entriesToGroups = new ();

            foreach (AnalyzeRule.AnalyzeResult duplicate in duplicates)
            {
                Debug.Log(duplicate.resultName);
                string[] dSplit = duplicate.resultName.Split(':');

                string dBundle = Application.isEditor ? dSplit[^1] : dSplit[0];
                string dAsset = Application.isEditor ? dSplit[0] : dSplit[^1];

                if (!entriesToGroups.ContainsKey(dAsset))
                    entriesToGroups.Add(dAsset, new List<string> { dBundle });
                else if (!entriesToGroups[dAsset].Contains(dBundle))
                    entriesToGroups[dAsset].Add(dBundle);
            }

            StringBuilder message = new StringBuilder();

            foreach (var keyValuePair in entriesToGroups
                        .Where(keyValuePair => !EXCLUDED_FILE_TYPES.Contains(keyValuePair.Key.Split('.')[^1])))
            {
                message.Append(keyValuePair.Key + " - GROUPS: ");

                foreach (string group in keyValuePair.Value)
                    message.Append(group + ", ");

                message.Remove(message.Length - 2, 2);
                message.Append(";\n");
            }

            Assert.That(message.ToString(), Is.Empty, message: $"Found {message.ToString().Split(';').Length} assets being duplicated between groups: \n{message}");
        }
    }
}
