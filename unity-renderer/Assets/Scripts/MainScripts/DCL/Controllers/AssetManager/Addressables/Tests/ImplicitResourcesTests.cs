using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[TestFixture]
public class ImplicitResourcesTest
{
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
                // if (!(line.Contains("const") && line.Contains("string")) && !line.Contains("Resources.Load"))
                //     continue;
                foreach (KeyValuePair<string, string> searchString in new Dictionary<string, string>(searchStrings))
                {
                    if (line.Contains(searchString.Value) && searchStrings.ContainsKey(searchString.Key))
                        searchStrings.Remove(searchString.Key);
                    // if (line.Contains("Resources.Load") && line.Contains(searchString.Value))
                    // {
                    //     result.Add(new { searchString, filePath, line, lineIndex = i + 1 });
                    //
                    //     if (searchStrings.ContainsKey(searchString.Key))
                    //         searchStrings.Remove(searchString.Key);
                    //
                    //     break;
                    // }
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
