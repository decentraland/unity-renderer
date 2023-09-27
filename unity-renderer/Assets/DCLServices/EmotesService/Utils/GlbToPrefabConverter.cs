using System.IO;
using UnityEditor;
using UnityEngine;

namespace DCLServices.EmotesService
{
    public class GlbToPrefabConverter : Editor
    {
        [MenuItem("Assets/Convert GLB to Prefab", true)]
        private static bool ValidateProcessGLBFile()
        {
            Object selectedObject = Selection.activeObject;

            if (selectedObject != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(selectedObject);
                return Path.GetExtension(assetPath).Equals(".glb", System.StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        [MenuItem("Assets/Convert GLB to Prefab")]
        private static void ProcessGLBFile()
        {
            Object selectedObject = Selection.activeObject;

            if (selectedObject != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(selectedObject);
                var customAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                GameObject assetInstance = Instantiate(customAsset);

                var assetDirectory = Path.GetDirectoryName(assetPath);
                var assetFolder = $"{assetDirectory}/{customAsset.name}/";

                var path = $"{Path.GetDirectoryName(assetPath)}/{customAsset.name}.prefab";
                PrefabUtility.SaveAsPrefabAsset(assetInstance, path);

                if (!Directory.Exists(assetFolder))
                    Directory.CreateDirectory(assetFolder);

                Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);

                foreach (Object subAsset in subAssets)
                {
                    if (subAsset is Mesh mesh)
                    {
                        string meshPath = assetFolder + mesh.name + ".asset";
                        AssetDatabase.RemoveObjectFromAsset(mesh);
                        AssetDatabase.CreateAsset(mesh, AssetDatabase.GenerateUniqueAssetPath(meshPath));
                    }
                    else if (subAsset is Material material)
                    {
                        string materialPath = assetFolder + material.name + ".mat";
                        AssetDatabase.RemoveObjectFromAsset(material);
                        AssetDatabase.CreateAsset(material, AssetDatabase.GenerateUniqueAssetPath(materialPath));
                    }
                    else if (subAsset is Texture2D)
                    {
                        Texture2D texture = subAsset as Texture2D;
                        string texturePath = assetFolder + texture.name + ".asset";
                        AssetDatabase.RemoveObjectFromAsset(texture);
                        AssetDatabase.CreateAsset(texture, AssetDatabase.GenerateUniqueAssetPath(texturePath));
                    }
                    else if (subAsset is AnimationClip animationClip)
                    {
                        string animationPath = assetFolder + animationClip.name + ".anim";
                        AssetDatabase.RemoveObjectFromAsset(animationClip);
                        AssetDatabase.CreateAsset(animationClip, AssetDatabase.GenerateUniqueAssetPath(animationPath));
                    }
                }

                // Destroy temporary instances
                DestroyImmediate(assetInstance);

                AssetDatabase.Refresh();
            }
        }
    }
}
