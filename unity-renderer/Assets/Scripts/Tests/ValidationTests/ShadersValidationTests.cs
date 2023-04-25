using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static DefaultNamespace.TestsUtils;

namespace Tests.ValidationTests
{
    [Category("EditModeCI")]
    public class ShadersValidationTests
    {
        private const string URP_LIT_MATERIAL_PATH = "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Lit.mat";

        [TestCaseSource(nameof(AllContainerAssetsInAssetsFolder))]
        public void ValidateAssetsShouldNotUseURPLitMaterial(string assetPath)
        {
            string forbiddenMaterialGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(
                    AssetDatabase.LoadAssetAtPath<Material>(URP_LIT_MATERIAL_PATH)));

            string[] deps = AssetDatabase.GetDependencies(assetPath, true);

            Assert.IsFalse(
                deps.Select(AssetDatabase.AssetPathToGUID)
                    .Any(depGuid => depGuid == forbiddenMaterialGuid), message: $"Asset {assetPath} has dependencies on forbidden material {URP_LIT_MATERIAL_PATH}. Replace with DCL version of it.");
        }

        [TestCaseSource(nameof(AllMaterialsInAssetsFolder))]
        public void ValidateMaterialShouldNotUseURPLitShader(string materialPath)
        {
            Shader urpLitShader = AssetDatabase.LoadAssetAtPath<Material>(URP_LIT_MATERIAL_PATH).shader;

            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

            Assert.That(material.shader, Is.Not.EqualTo(urpLitShader), message: $"Material {materialPath} uses forbidden shader {urpLitShader.name}. Replace with DCL version of it.");
        }

        private static IEnumerable<string> AllMaterialsInAssetsFolder() =>
            AllAssetsAtPaths(assetTypes: "t:Material", ASSETS_FOLDER_PATH);

        private static IEnumerable<string> AllContainerAssetsInAssetsFolder() =>
            AllAssetsAtPaths(assetTypes: "t:Model t:Prefab t:Scene", ASSETS_FOLDER_PATH);
    }
}
