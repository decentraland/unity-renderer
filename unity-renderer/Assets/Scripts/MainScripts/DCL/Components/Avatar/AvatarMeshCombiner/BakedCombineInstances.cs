using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL
{
    public class BakedCombineInstances : IDisposable
    {
        private List<Mesh> bakedMeshes = new List<Mesh>();

        public void Dispose()
        {
            foreach ( var mesh in bakedMeshes )
            {
                Object.Destroy(mesh);
            }
        }

        public void Bake(List<CombineInstance> combineInstances, List<SkinnedMeshRenderer> renderers)
        {
            int combineInstancesCount = combineInstances.Count;

            for ( int i = 0; i < combineInstancesCount; i++)
            {
                Mesh mesh = new Mesh();

                // Important note: It seems that mesh normals are scaled by the matrix when using BakeMesh.
                //                 This is wrong and and shouldn't happen, so we have to arrange them manually.
                //
                //                 We DON'T do this yet because the meshes can be read-only, so the original
                //                 normals can't be extracted. For normals, visual artifacts are minor because
                //                 toon shader doesn't use any kind of normal mapping.
                renderers[i].BakeMesh(mesh, true);

                var combinedInstance = combineInstances[i];
                combinedInstance.mesh = mesh;
                bakedMeshes.Add(mesh);

                combineInstances[i] = combinedInstance;
            }
        }
    }
}