using DCL.Models;
using System;

namespace DCL.Components
{
    public abstract class LoadWrapper
    {
        public bool useVisualFeedback = true;
        public bool initialVisibility = true;
        public bool alreadyLoaded = false;

        public IDCLEntity entity;

        public abstract void Load(string url, Action<LoadWrapper> OnSuccess, Action<LoadWrapper> OnFail);
        public abstract void Unload();
    }
}