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
            foreach (string file in AllCSharpFiles())
            {
                string fileContent = File.ReadAllText(file);

                // Act
                bool foundOffendingUsage = fileContent.Contains("StringComparison.CurrentCultureIgnoreCase");

                // Assert
                Assert.IsFalse(foundOffendingUsage, $"File {Path.GetFileName(file)} uses StringComparison.CurrentCultureIgnoreCase. Please use StringComparison.OrdinalIgnoreCase instead.");
            }
        }

        private static IEnumerable<string> AllCSharpFiles() =>
            AssetDatabase.FindAssets("t:Script")
                         .Select(AssetDatabase.GUIDToAssetPath)
                         .Where(assetPath => Path.GetFileName(assetPath) != "AssemblyInfo.cs" && Path.GetExtension(assetPath) == ".cs" &&
                                             !assetPath.StartsWith("Packages/") && !EXCLUDED_PATHS.Any(assetPath.Contains));
    }
}
