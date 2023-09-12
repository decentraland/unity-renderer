using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Tests.ValidationTests
{
    [Category("EditModeCI")]
    public class CodeConventionsTests
    {
        private static readonly string[] EXCLUDED_PATHS = { "/Editor/", "/Tests/", "/EditorTests/" };

        [Test]
        public void AvoidUsingCurrentCultureIgnoreCase()
        {
            // Arrange
            List<string> errorMessages = new List<string>();

            foreach (string file in AllRuntimeCSharpFiles())
            {
                string[] lines = File.ReadAllLines(file);

                // Act
                for (var i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Trim().StartsWith("//") || !lines[i].Contains("StringComparison.CurrentCultureIgnoreCase"))
                        continue;

                    errorMessages.Add($"File {Path.GetFileName(file)} on line {i + 1}");
                }

                // Assert
                Assert.AreEqual(0, errorMessages.Count, $"{errorMessages.Count} classes use StringComparison.CurrentCultureIgnoreCase. Please use StringComparison.OrdinalIgnoreCase instead.\n"
                                                        + $"{string.Join(",\n", errorMessages)}");
            }
        }

        private static IEnumerable<string> AllRuntimeCSharpFiles() =>
            AssetDatabase.FindAssets("t:Script")
                         .Select(AssetDatabase.GUIDToAssetPath)
                         .Where(assetPath => Path.GetFileName(assetPath) != "AssemblyInfo.cs" && Path.GetExtension(assetPath) == ".cs" &&
                                             !assetPath.StartsWith("Packages/") && !EXCLUDED_PATHS.Any(assetPath.Contains));
    }
}
