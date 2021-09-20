using System;
using UnityEngine;

namespace DCL
{
    /// <summary>
    /// Container for GlobalEvents. This object lifecycle is tied to the Environment lifecycle.
    /// So using this class ensures no event leaks happen between tests.
    ///
    /// Note that this is not using an interface because this can be mocked as a Stub.
    /// </summary>
    public class GlobalEvents
    {
        public event System.Action<Mesh> OnWillUploadMeshToGPU;

        internal void RaiseWillUploadMeshToGPU(Mesh mesh)
        {
            OnWillUploadMeshToGPU?.Invoke(mesh);
        }
    }
}