using System;
using System.Collections.Generic;
using UnityEngine;

namespace AvatarSystem
{
    public interface ILOD : IDisposable
    {
        int lodIndex { get; }
        void SetDependencies(Renderer combinedAvatar, IEnumerable<Renderer> facialFeatures);
        void SetLodIndex(int lodIndex, bool inmediate = false);
    }
}