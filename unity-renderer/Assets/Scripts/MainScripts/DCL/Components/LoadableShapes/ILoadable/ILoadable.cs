using System;

namespace DCL.Components
{
    public interface ILoadable
    {
        System.Action OnSuccess { get; set; }
        System.Action<Exception> OnFail { get; set; }

        void Load(string url);
    }
}