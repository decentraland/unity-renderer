using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Rendering
{
    public interface ICullingObjectsTracker : IDisposable
    {
        void AddRenderers(ICollection<Renderer> inRenderers);
        void SetIgnoredLayersMask(int ignoredLayersMask);
        void MarkDirty();
        bool IsDirty();
        ICollection<Renderer> GetRenderers();
        ICollection<SkinnedMeshRenderer> GetSkinnedRenderers();
        Animation[] GetAnimations();
        IEnumerator PopulateRenderersList();
        void ForcePopulateRenderersList(bool includeInactives);
    }
}
