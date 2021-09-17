using System;
using UnityEngine;

namespace DCL
{
    public interface IGlobalAssetEvents
    {
        event System.Action<Mesh> OnWillUploadMeshToGPU;

        internal void RaiseWillUploadMeshToGPU(Mesh mesh);
    }

    public class GlobalAssetEvents : IGlobalAssetEvents
    {
        public event System.Action<Mesh> OnWillUploadMeshToGPU;

        // Explicit implementation has to be used because it's declared as internal in the interface
        void IGlobalAssetEvents.RaiseWillUploadMeshToGPU(Mesh mesh)
        {
            OnWillUploadMeshToGPU?.Invoke(mesh);
        }
    }
}