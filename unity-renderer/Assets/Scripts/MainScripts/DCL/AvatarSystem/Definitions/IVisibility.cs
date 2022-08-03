using System;
using System.Collections.Generic;
using UnityEngine;

namespace AvatarSystem
{
    public interface IVisibility : IDisposable
    {
        void Bind(Renderer combinedRenderer, List<Renderer> facialFeatures);
        void AddGlobalConstrain(string key);
        void RemoveGlobalConstrain(string key);

        void AddCombinedRendererConstrain(string key);
        void RemoveCombinedRendererConstrain(string key);

        void AddFacialFeaturesConstrain(string key);
        void RemoveFacialFeaturesConstrain(string key);

        bool IsGloballyVisible();
        bool IsMainRenderVisible();
        bool AreFacialFeaturesVisible();

    }
}