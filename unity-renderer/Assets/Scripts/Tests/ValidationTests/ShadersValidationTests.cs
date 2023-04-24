using NUnit.Framework;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Tests.ValidationTests
{
    public class ShadersValidationTests
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

                foreach (var depGuid in deps.Select(AssetDatabase.AssetPathToGUID))
                {
                    Material material = AssetDatabase.LoadAssetAtPath<Material>(depGuid);

                    if (material != null)
                    {
                        Debug.Log($"{material.name} uses {material.shader.name}", material);

                        // Debug.Log($"Asset '{assetPath}' uses material '{urpLitShader.name}'", asset);
                        break;
                    }
                }
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

                // if(
                // material.shader == urpLitShader.shader || material.shader.name == urpLitShader.name ||
                // material.shader.name == "Universal Render Pipeline/Lit")
                // material.shader.name.Contains("Standard"))

                Debug.Log($"{material.name} uses {material.shader.name}", material);
            }
        }
    }
}
