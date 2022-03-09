namespace UnityGLTF
{
    public static class GLTFSceneImporterUtils
    {
        /// <summary>
        /// This method returns the approximate mesh size that result from the given UnityMeshData object.
        /// The calculation is not exact and is set to overshoot the true size by a small margin.
        /// </summary>
        /// <param name="unityMeshData">A UnityMeshData object to compute the memory size.</param>
        /// <returns>The estimated memory size in bytes.</returns>
        public static long ComputeEstimatedMeshSize(UnityMeshData unityMeshData)
        {
            long result = 4080; // NOTE(Brian): I found an overhead of 4080 bytes in isolated tests.
            const long WORD_SIZE = 4; // word size = 4 bytes 

            if (unityMeshData.Vertices != null)
                result += unityMeshData.Vertices.Length * WORD_SIZE * 3;

            if (unityMeshData.Normals != null)
                result += unityMeshData.Normals.Length * WORD_SIZE * 3;

            if (unityMeshData.Uv1 != null)
                result += unityMeshData.Uv1.Length * WORD_SIZE * 2;

            if (unityMeshData.Uv2 != null)
                result += unityMeshData.Uv2.Length * WORD_SIZE * 2;

            if (unityMeshData.Uv3 != null)
                result += unityMeshData.Uv3.Length * WORD_SIZE * 2;

            if (unityMeshData.Uv4 != null)
                result += unityMeshData.Uv4.Length * WORD_SIZE * 2;

            if (unityMeshData.Colors != null)
                result += unityMeshData.Colors.Length * WORD_SIZE * 4;

            if (unityMeshData.Tangents != null)
                result += unityMeshData.Tangents.Length * WORD_SIZE * 4;

            if (unityMeshData.BoneWeights != null)
                result += unityMeshData.BoneWeights.Length * WORD_SIZE * 8;

            if (unityMeshData.Triangles != null)
                result += unityMeshData.Triangles.Length * WORD_SIZE;

            // For some reason we have to duplicate the result.
            // Maybe is because the actual word size is long?
            return result * 2; 
        }
    }
}