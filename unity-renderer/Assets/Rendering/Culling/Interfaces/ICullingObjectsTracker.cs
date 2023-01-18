using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Rendering
{
    public interface ICullingObjectsTracker : IDisposable
    {
        void SetIgnoredLayersMask(int ignoredLayersMask);
        void MarkDirty();
        bool IsDirty();
        IReadOnlyList<Renderer> GetRenderers();
        IReadOnlyList<SkinnedMeshRenderer> GetSkinnedRenderers();
        Animation[] GetAnimations();
        IEnumerator PopulateRenderersList();
        void ForcePopulateRenderersList();
    }
}
