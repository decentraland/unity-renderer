using System;

namespace DCL
{
    public interface IService : IDisposable
    {
        void Initialize();
    }
}