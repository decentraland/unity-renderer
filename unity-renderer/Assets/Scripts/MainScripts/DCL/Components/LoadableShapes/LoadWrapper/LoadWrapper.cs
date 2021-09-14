using DCL.Models;
using System;

namespace DCL.Components
{
    public abstract class LoadWrapper
    {
        public bool useVisualFeedback = false; // TODO: temporarily disabled, re-enable when GLTFs hologram loading gets fixed
        public bool initialVisibility = false; // TODO: temporarily disabled, re-enable when GLTFs hologram loading gets fixed
        public bool alreadyLoaded = false;

        public IDCLEntity entity;

        public abstract void Load(string url, Action<LoadWrapper> OnSuccess, Action<LoadWrapper> OnFail);
        public abstract void Unload();
    }
}