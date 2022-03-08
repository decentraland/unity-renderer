namespace UnityGLTF
{
    public static class GLTFSceneImporterUtils
    {
        public static int ComputeEstimatedMeshSize(UnityMeshData unityMeshData)
        {
            int result = 0;
            if (unityMeshData.Vertices != null)
                result += unityMeshData.Vertices.Length * 4 * 3;

            if (unityMeshData.Normals != null)
                result += unityMeshData.Normals.Length * 4 * 3;

            if (unityMeshData.Uv1 != null)
                result += unityMeshData.Uv1.Length * 4 * 2;

            if (unityMeshData.Uv2 != null)
                result += unityMeshData.Uv2.Length * 4 * 3;

            if (unityMeshData.Uv3 != null)
                result += unityMeshData.Uv3.Length * 4 * 3;

            if (unityMeshData.Uv4 != null)
                result += unityMeshData.Uv4.Length * 4 * 3;

            if (unityMeshData.Colors != null)
                result += unityMeshData.Colors.Length * 4 * 4;

            if (unityMeshData.Tangents != null)
                result += unityMeshData.Tangents.Length * 4 * 4;

            if (unityMeshData.BoneWeights != null)
                result += unityMeshData.BoneWeights.Length * 4 * 8;

            if (unityMeshData.Triangles != null)
                result += unityMeshData.Triangles.Length * 4;

            return result;
        }
    }
}