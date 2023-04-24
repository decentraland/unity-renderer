using DCL.HUD.Common;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Tests.ValidationTests
{
    public class PrefabsValidationTests
    {
        private static readonly string[] ASSETS_FOLDER_PATHS = { "Assets" };

        [Test]
        public void ValidateShadersInAssets()
        {
            Material urpLitShader = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Lit.mat");
            string[] guids = AssetDatabase.FindAssets("t:Object", ASSETS_FOLDER_PATHS);

            // Get the GUID for the given material
            string materialGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(urpLitShader));

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

                string[] deps = AssetDatabase.GetDependencies(assetPath, true);

                if (deps.Select(AssetDatabase.AssetPathToGUID).Any(depGuid => depGuid == materialGuid))
                    Debug.Log($"Asset '{assetPath}' uses material '{urpLitShader.name}'", asset);
            }

            Assert.IsTrue(true);
        }

        [Test]
        public void ValidateShadersInMaterials()
        {
            Material urpLitShader = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Lit.mat");
            string[] guids = AssetDatabase.FindAssets("t:Material", ASSETS_FOLDER_PATHS);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

                if(material.shader == urpLitShader.shader || material.shader.name == urpLitShader.name || material.shader.name == "Universal Render Pipeline/Lit")
                    Debug.Log($"{material.name} uses {material.shader.name}", material);
            }
        }

        [TestCaseSource(nameof(AllPrefabPaths))]
        public void ValidateShowHideAnimators(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            var unityAnimators =
                from showHideAnimator in prefab.GetComponentsInChildren<ShowHideAnimator>()
                where showHideAnimator.GetComponent<Animator>() != null
                select $"{showHideAnimator.gameObject.name}";

            Assert.That(unityAnimators, Is.Empty, "Unity animator is presented on several child objects that have DCL ShowHideAnimator component (DOTween-based)");
        }

        [TestCaseSource(nameof(AllPrefabPaths))]
        public void ScrollsIncludesSensibilityMultiplierByPlatform(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            var scrollsWithoutSensibility = from scroll in prefab.GetComponentsInChildren<ScrollRect>(true)
                where scroll.GetComponent<ScrollRectSensitivityHandler>() == null && scroll.GetComponent<DynamicScrollSensitivity>() == null
                select $"{scroll.gameObject.name}";

            Assert.That(scrollsWithoutSensibility, Is.Empty);
        }

        private static IEnumerable<string> AllPrefabPaths() =>
            AssetDatabase
               .FindAssets("t:Prefab", ASSETS_FOLDER_PATHS)
               .Select(AssetDatabase.GUIDToAssetPath);
    }
}
