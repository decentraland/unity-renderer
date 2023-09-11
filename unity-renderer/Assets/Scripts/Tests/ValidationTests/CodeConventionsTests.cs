using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Tests.ValidationTests
{
    public class CodeConventionsTests
    {
        private static readonly string[] EXCLUDED_PATHS = { "/Editor/", "/Tests/", "/EditorTests/" };

        [Test]
        public void AvoidUsingCurrentCultureIgnoreCase_SingleTest()
        {
            foreach (string file in AllCSharpFiles())
            {
                string fileContent = File.ReadAllText(file);

                // Check if the file content contains the offending usage
                bool foundOffendingUsage = fileContent.Contains("StringComparison.CurrentCultureIgnoreCase");

                // Assert that there are no offending usages.
                Assert.IsFalse(foundOffendingUsage, $"File {Path.GetFileName(file)} uses StringComparison.CurrentCultureIgnoreCase. Please use StringComparison.OrdinalIgnoreCase instead.");
            }
        }

        private static IEnumerable<string> AllCSharpFiles() =>
            AssetDatabase.FindAssets("t:Script")
                         .Select(AssetDatabase.GUIDToAssetPath)
                         .Where(assetPath => Path.GetFileName(assetPath) != "AssemblyInfo.cs" && Path.GetExtension(assetPath) == ".cs" &&
                                             !assetPath.StartsWith("Packages/") && !EXCLUDED_PATHS.Any(assetPath.Contains) && !IsInEditorAssembly(assetPath));

        private static bool IsInEditorAssembly(string assetPath)
        {
            string directory = Path.GetDirectoryName(assetPath);

            while (directory != null && directory.StartsWith("Assets"))
            {
                string[] asmdefFiles = Directory.GetFiles(directory, "*.asmdef");

                if (asmdefFiles.Select(File.ReadAllText)
                               .Any(asmdefContent => asmdefContent.Contains("\"platforms\": [\"Editor\"]")))
                    return true;

                directory = Path.GetDirectoryName(directory);
            }

            return false;
        }
    }
}
