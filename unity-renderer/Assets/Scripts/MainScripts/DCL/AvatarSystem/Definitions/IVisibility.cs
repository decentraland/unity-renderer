using System;
using UnityEngine;

namespace AvatarSystem
{
    public interface IVisibility : IDisposable
    {
        void Bind(Renderer[] renderers);
        void SetExplicitVisibility(bool explicitVisibility);
        void SetLoadingReady(bool loadingReady);
    }
}