using System;
using UnityEngine;

namespace AvatarSystem
{
    public interface IVisibility : IDisposable
    {
        void Bind(Renderer combinedRenderer, Renderer[] facialFeatures);
        public void AddGlobalConstrain(string key);
        public void RemoveGlobalConstrain(string key);

        public void AddCombinedRendererConstrain(string key);
        public void RemoveCombinedRendererConstrain(string key);

        public void AddFacialFeaturesConstrain(string key);
        public void RemoveFacialFeaturesConstrain(string key);
    }
}