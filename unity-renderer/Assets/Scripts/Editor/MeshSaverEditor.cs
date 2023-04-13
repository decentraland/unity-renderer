using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class MeshSaverEditor
    {
        [MenuItem("CONTEXT/SkinnedMeshRenderer/Save Mesh")]
        public static void SaveMeshFromSkinnedMeshRenderer(MenuCommand menuCommand)
        {
            SkinnedMeshRenderer renderer = menuCommand.context as SkinnedMeshRenderer;
            Mesh mesh = renderer!.sharedMesh;
            SaveMesh(mesh);
        }

        [MenuItem("CONTEXT/MeshFilter/Save Mesh")]
        public static void SaveMeshFromMeshFilter(MenuCommand menuCommand)
        {
            MeshFilter meshFilter = menuCommand.context as MeshFilter;
            Mesh mesh = meshFilter!.sharedMesh;
            SaveMesh(mesh);
        }

        private static void SaveMesh(Mesh mesh)
        {
            string path = EditorUtility.SaveFilePanel("Save Separate Mesh Asset", "Assets/", mesh.name, "asset");
            if (string.IsNullOrEmpty(path)) return;

            path = FileUtil.GetProjectRelativePath(path);
            Mesh meshToSave = Object.Instantiate(mesh);

            MeshUtility.Optimize(meshToSave);
            AssetDatabase.CreateAsset(meshToSave, path);
            AssetDatabase.SaveAssets();
        }
    }
}
