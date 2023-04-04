using DCL.Models;
using System;

namespace DCL
{
    public abstract class LoadWrapper
    {
        public bool initialVisibility = true;
        public bool alreadyLoaded = false;

        public IDCLEntity entity;

        public abstract void Load(string url, Action<LoadWrapper> OnSuccess, Action<LoadWrapper, Exception> OnFail);
        public abstract void Unload();
    }
}
