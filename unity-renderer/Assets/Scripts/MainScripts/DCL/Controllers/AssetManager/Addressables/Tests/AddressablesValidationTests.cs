using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[Category("EditModeCI")]
public class AddressablesValidationTests
{
    [TestCase("Scripts/MainScripts/DCL/Controllers/HUD/NotificationHUD")]
    public void ValidateFolderDoesNotHaveResourcesFolderInside(string folderName)
    {
        string folderPath = Application.dataPath + $"/{folderName}";
        var directory = new DirectoryInfo(folderPath);

        if (!directory.Exists)
            Assert.Fail($"{folderName} does not exist");

        bool hasResourcesFolder = directory.GetDirectories("*", SearchOption.AllDirectories).Any(subDirectory => subDirectory.Name == "Resources");
        Assert.IsFalse(hasResourcesFolder, $"{folderName} folder or its sub-folders contain Resources folder");
    }

    [Test]
    public void ValidateImplicitAssetsInResourcesFolder()
    {
        List<string> implicitAssets = FindResourcesLoadCalls(GetResourcesAssets());

        foreach (string asset in implicitAssets)
            Debug.Log(asset);

        Assert.That(implicitAssets, Is.Empty);
    }

    private static List<string> FindResourcesLoadCalls(Dictionary<string, string> searchStrings)
    {
        string[] csFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);

        var result = new List<dynamic>();

        foreach (string filePath in csFiles)
        {
            var lines = File.ReadLines(filePath).ToList();

            for (var i = 0; i < lines.Count; i++)
            {
                string line = lines[i];

                if (!(line.Contains("const") && line.Contains("string")) && !line.Contains("Load"))
                    continue;

                foreach (KeyValuePair<string, string> searchString in new Dictionary<string, string>(searchStrings))
                {
                    if (line.Contains("const") && line.Contains("string") && line.Contains(searchString.Value)
                        && searchStrings.ContainsKey(searchString.Key))
                        searchStrings.Remove(searchString.Key);

                    if (line.Contains("Load") && line.Contains(searchString.Value))
                    {
                        result.Add(new { searchString, filePath, line, lineIndex = i + 1 });

                        if (searchStrings.ContainsKey(searchString.Key))
                            searchStrings.Remove(searchString.Key);
                    }
                }
            }
        }

        return searchStrings.Keys.ToList();
    }

    private static Dictionary<string, string> GetResourcesAssets(string searchPath = "Assets")
    {
        var list = new Dictionary<string, string>();

        foreach (string resourcesDirectory in Directory.GetDirectories(searchPath, "Resources", SearchOption.AllDirectories))
        foreach (string file in Directory.GetFiles(resourcesDirectory, "*", SearchOption.AllDirectories))
        {
            if (!file.EndsWith(".meta"))
            {
                string assetPath = file.Replace(Application.dataPath, "Assets").Replace("\\", "/");
                list.TryAdd(assetPath, Path.GetFileNameWithoutExtension(assetPath));
            }
        }

        return list;
    }
}
