using System;
using UnityEngine;

namespace AvatarSystem
{
    public interface IVisibility : IDisposable
    {
        void Bind(Renderer combinedRenderer, Renderer[] facialFeatures);
        void SetExplicitVisibility(bool explicitVisibility);
        void SetLoadingReady(bool loadingReady);
        void SetCombinedRendererVisibility(bool combinedRendererVisibility);
        void SetFacialFeaturesVisibility(bool facialFeaturesVisibility);
    }
}